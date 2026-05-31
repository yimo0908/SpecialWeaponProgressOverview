using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Ipc;
using FFXIVClientStructs.FFXIV.Client.Game;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Data;

public static class Inventory
{
    private static ICallGateSubscriber<uint, ulong, uint, uint>? _itemCountIpc;
    private static ICallGateSubscriber<bool, bool>?              _initializedEvent;
    private static ICallGateSubscriber<bool>?                    _isInitialized;

    public static bool AToolsInstalled =>
        PluginService.PluginInterface.InstalledPlugins.Any(
            x => x.InternalName is "Allagan Tools" or "InventoryTools");

    public static bool ATools => AToolsInstalled && _isInitialized?.InvokeFunc() == true;

    internal static void Init()
    {
        _initializedEvent = PluginService.PluginInterface.GetIpcSubscriber<bool, bool>("AllaganTools.Initialized");
        _isInitialized    = PluginService.PluginInterface.GetIpcSubscriber<bool>("AllaganTools.IsInitialized");
        _initializedEvent.Subscribe(SetupIpc);
        PluginService.ClientState.Logout += ClearRetainerCache;
        SetupIpc(true);
    }

    private static void ClearRetainerCache(int _, int __) => RetainerData.Clear();

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

    public static unsafe int GetRetainerItemCount(uint itemId, bool tryCache = true)
    {
        if (!ATools) return 0;
        if (!PluginService.ClientState.IsLoggedIn || PluginService.Condition[ConditionFlag.OnFreeTrial])
            return 0;

        try
        {
            if (tryCache && HasCached(itemId))
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

            return GetCachedSum(itemId);
        }
        catch (Exception ex)
        {
            PluginService.PluginLog?.Warning($"获取雇员背包数据异常: {ex.Message}");
            return 0;
        }
    }

    private static bool HasCached(uint itemId) =>
        RetainerData.Values.Any(d => d.ContainsKey(itemId));

    private static int GetCachedSum(uint itemId) =>
        (int)RetainerData.Values
            .SelectMany(d => d.Values)
            .Where(x => x.ItemId == itemId)
            .Sum(x => x.Quantity);

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
    }
}
