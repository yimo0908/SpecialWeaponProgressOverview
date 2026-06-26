using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using FFXIVClientStructs.FFXIV.Client.Game;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Data;

public static class Inventory
{
    /// <summary>是否已完成 AT.IPC 全部数据缓存，缓存后不再调用 AT.IPC。</summary>
    public static bool DataCached { get; private set; }

    private static ICallGateSubscriber<uint, ulong, uint, uint>? _itemCountIpc;
    private static ICallGateSubscriber<bool, bool>?              _initializedEvent;
    private static ICallGateSubscriber<bool>?                    _isInitialized;

    private static bool _aToolsInstalled;
    private static bool _aToolsInstalledChecked;

    /// <summary>物品总数缓存：itemId → 全雇员合计数量。RefreshCache 时预计算。</summary>
    private static readonly Dictionary<uint, int> _itemTotalCache = new();

    public static bool AToolsInstalled
    {
        get
        {
            if (_aToolsInstalledChecked) return _aToolsInstalled;
            _aToolsInstalled = PluginService.PluginInterface.InstalledPlugins.Any(
                x => x.InternalName is "Allagan Tools" or "InventoryTools");
            _aToolsInstalledChecked = true;
            return _aToolsInstalled;
        }
    }

    public static bool ATools
    {
        get
        {
            if (!AToolsInstalled) return false;
            try
            {
                return _isInitialized?.InvokeFunc() == true;
            }
            catch (IpcNotReadyError)
            {
                return false;
            }
        }
    }

    internal static void Init()
    {
        DataCached = false;

        // 始终注册 IPC 事件订阅，避免因插件加载顺序错过 Allagan Tools 的 Initialized 事件
        _initializedEvent = PluginService.PluginInterface.GetIpcSubscriber<bool, bool>("AllaganTools.Initialized");
        _isInitialized    = PluginService.PluginInterface.GetIpcSubscriber<bool>("AllaganTools.IsInitialized");
        _initializedEvent.Subscribe(SetupIpc);
        PluginService.ClientState.Logout += ClearRetainerCache;

        try
        {
            if (AToolsInstalled && _isInitialized?.InvokeFunc() == true)
            {
                // Allagan Tools 已就绪，立即建立 IPC 连接
                SetupIpc(true);
            }
            else
            {
                PluginService.PluginLog.Debug("Allagan Tools 尚未就绪，等待 Initialized 事件");
            }
        }
        catch (IpcNotReadyError)
        {
            // Allagan Tools 已安装但 IPC 方法尚未注册，等 Initialized 事件触发后再连接
            PluginService.PluginLog.Debug("Allagan Tools 的 IPC 尚未就绪，等待 Initialized 事件");
        }
    }

    /// <summary>确保 AT.IPC 已连接。处理 AT 在本插件之后加载、Initialized 事件未触发的情况。</summary>
    private static void EnsureIpcConnected()
    {
        if (_itemCountIpc != null) return;
        if (!AToolsInstalled) return;

        try
        {
            _isInitialized ??= PluginService.PluginInterface.GetIpcSubscriber<bool>("AllaganTools.IsInitialized");
            if (_isInitialized.InvokeFunc())
            {
                SetupIpc(true);
                PluginService.PluginLog.Debug("延迟连接 AT.IPC 成功");
            }
        }
        catch (IpcNotReadyError)
        {
            // AT IPC 仍未就绪
        }
    }

    /// <summary>清空并重新从 AT.IPC 拉取所有武器及材料数据到本地缓存，缓存后页面只读缓存不再调用 IPC。</summary>
    public static unsafe void RefreshCache()
    {
        EnsureIpcConnected();

        if (!ATools || _itemCountIpc == null)
        {
            PluginService.PluginLog.Debug("AT.IPC 未就绪，跳过刷新");
            return;
        }

        if (!PluginService.ClientState.IsLoggedIn || PluginService.Condition[ConditionFlag.OnFreeTrial])
            return;

        RetainerData.Clear();
        _itemTotalCache.Clear();
        DataCached = false;

        // 收集所有需要缓存的物品 ID（武器 + 材料）
        var allItemIds = new HashSet<uint>();
        foreach (var kvp in WeaponSeriesInfo.All)
        {
            foreach (var stage in kvp.Value.WeaponIdStages)
            {
                foreach (var itemId in stage)
                    allItemIds.Add(itemId);
            }
        }

        // 从配方表自动收集所有材料 ID，避免与 DataBase 重复维护
        foreach (var recipe in DataBase.BozjaMaterialRecipes
                     .Concat(DataBase.MandervillousMaterialRecipes)
                     .Concat(DataBase.PhantomMaterialRecipes)
                     .Concat(DataBase.EurekaMaterialRecipes)
                     .SelectMany(stage => stage))
        {
            allItemIds.Add(recipe.ItemId);
        }

        // 先遍历雇员，再遍历物品：每个雇员一次性查询所有物品
        for (var i = 0u; i < 10; i++)
        {
            var retainer   = RetainerManager.Instance()->GetRetainerBySortedIndex(i);
            var retainerId = retainer->RetainerId;
            if (retainerId == 0 || !retainer->Available)
                continue;

            if (!RetainerData.TryGetValue(retainerId, out var dict))
            {
                dict = new Dictionary<uint, ItemInfo>();
                RetainerData[retainerId] = dict;
            }

            foreach (var itemId in allItemIds)
            {
                var count = GetRetainerInventoryItem(itemId, retainerId);
                dict[itemId] = new ItemInfo(itemId, count);
            }
        }

        // 预计算每个物品的全雇员合计数量，后续查询直接读缓存
        foreach (var dict in RetainerData.Values)
            foreach (var info in dict.Values)
            {
                _itemTotalCache.TryGetValue(info.ItemId, out var existing);
                _itemTotalCache[info.ItemId] = existing + (int)info.Quantity;
            }

        DataCached = true;
        PluginService.PluginLog.Debug(
            $"AT.IPC 缓存刷新完成：{RetainerData.Count} 个雇员，{allItemIds.Count} 个物品");
    }

    private static void ClearRetainerCache(int _, int __)
    {
        RetainerData.Clear();
        _itemTotalCache.Clear();
        DataCached = false;
    }

    private static void SetupIpc(bool _)
    {
        _itemCountIpc = PluginService.PluginInterface.GetIpcSubscriber<uint, ulong, uint, uint>("AllaganTools.ItemCount");
    }

    // ---- 雇员背包缓存 ----
    public static readonly Dictionary<ulong, Dictionary<uint, ItemInfo>> RetainerData = new();

    private static uint GetRetainerInventoryItem(uint itemId, ulong retainerId)
    {
        if (!ATools || _itemCountIpc == null) return 0;

        return _itemCountIpc.InvokeFunc(itemId, retainerId, 10000)
             + _itemCountIpc.InvokeFunc(itemId, retainerId, 10001)
             + _itemCountIpc.InvokeFunc(itemId, retainerId, 10002)
             + _itemCountIpc.InvokeFunc(itemId, retainerId, 10003)
             + _itemCountIpc.InvokeFunc(itemId, retainerId, 10004)
             + _itemCountIpc.InvokeFunc(itemId, retainerId, 10005)
             + _itemCountIpc.InvokeFunc(itemId, retainerId, 10006)
             + _itemCountIpc.InvokeFunc(itemId, retainerId, (uint)InventoryType.RetainerCrystals);
    }

    public static unsafe int GetRetainerItemCount(uint itemId)
    {
        EnsureIpcConnected();

        if (!ATools) return 0;
        if (!PluginService.ClientState.IsLoggedIn || PluginService.Condition[ConditionFlag.OnFreeTrial])
            return 0;

        // 已通过 RefreshCache 完成全量缓存后，只读缓存，不再调用 AT.IPC
        if (DataCached)
            return GetCachedSum(itemId);

        try
        {
            if (HasCached(itemId))
                return GetCachedSum(itemId);

            for (var i = 0u; i < 10; i++)
            {
                var retainer = RetainerManager.Instance()->GetRetainerBySortedIndex(i);
                var retainerId = retainer->RetainerId;
                if (retainerId == 0 || !retainer->Available) continue;

                if (!RetainerData.TryGetValue(retainerId, out var dict))
                {
                    dict = new Dictionary<uint, ItemInfo>();
                    RetainerData[retainerId] = dict;
                }

                if (!dict.TryGetValue(itemId, out var info))
                {
                    info = new ItemInfo(itemId, 0);
                    dict[itemId] = info;
                }

                info.Quantity = GetRetainerInventoryItem(itemId, retainerId);
            }

            // 重新计算该物品的全雇员合计并缓存
            var sum = 0;
            foreach (var dict in RetainerData.Values)
                if (dict.TryGetValue(itemId, out var info2))
                    sum += (int)info2.Quantity;
            _itemTotalCache[itemId] = sum;
            return sum;
        }
        catch (Exception ex)
        {
            PluginService.PluginLog?.Warning($"获取雇员背包数据异常: {ex.Message}");
            return 0;
        }
    }

    private static bool HasCached(uint itemId) =>
        _itemTotalCache.ContainsKey(itemId);

    private static int GetCachedSum(uint itemId) =>
        _itemTotalCache.TryGetValue(itemId, out var total) ? total : 0;

    public static unsafe int GetItemCountTotal(uint itemId)
    {
        var countInRetainers   = GetRetainerItemCount(itemId);
        var inventoryManager   = InventoryManager.Instance();
        var countInBag         = inventoryManager->GetInventoryItemCount(itemId);
        var countInSaddleBag   = inventoryManager->GetItemCountInContainer(itemId, InventoryType.SaddleBag1)
                               + inventoryManager->GetItemCountInContainer(itemId, InventoryType.SaddleBag2);
        return countInRetainers + countInBag + countInSaddleBag;
    }

    public static void Dispose()
    {
        _initializedEvent?.Unsubscribe(SetupIpc);
        PluginService.ClientState.Logout -= ClearRetainerCache;
        _initializedEvent = null;
        _isInitialized    = null;
        _itemCountIpc     = null;
        _aToolsInstalledChecked = false;
    }
}
