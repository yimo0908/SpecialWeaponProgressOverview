using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Data;

public static class Inventory
{
    /// <summary>是否已完成雇员/投影台缓存数据扫描，扫描后只读缓存不再遍历游戏结构。</summary>
    public static bool DataCached { get; private set; }

    /// <summary>物品总数缓存：itemId → 雇员 + 投影台合计数量。RefreshCache 时预计算。</summary>
    private static readonly Dictionary<uint, int> _itemTotalCache = new();

    /// <summary>所有追踪的武器 ItemId 集合，用于 ItemAdded 事件快速过滤。惰性初始化。</summary>
    private static HashSet<uint>? _trackedWeaponIds;

    /// <summary>同帧内多个 ItemAdded 合并为一次 RefreshCache 的待刷新标志。</summary>
    private static bool _refreshPending;

    /// <summary>缓存因武器变动自动刷新后触发，供 UI 层订阅以触发重算。</summary>
    public static event Action? CacheAutoRefreshed;

    /// <summary>游戏道具检索缓存是否可读（雇员背包与投影台的数据源）。</summary>
    public static unsafe bool ItemFinderReady => ItemFinderModule.Instance() != null;

    internal static void Init()
    {
        DataCached = false;

        PluginService.ClientState.Logout += ClearCachedData;
        PluginService.GameInventory.ItemAdded += OnItemAdded;
    }

    /// <summary>
    /// 重新扫描游戏 ItemFinder 缓存中的雇员背包与投影台数据到本地缓存，
    /// 缓存后页面只读缓存，不再重复遍历游戏内存结构。
    /// </summary>
    public static unsafe void RefreshCache()
    {
        if (!PluginService.ClientState.IsLoggedIn || PluginService.Condition[ConditionFlag.OnFreeTrial])
            return;

        var finder = ItemFinderModule.Instance();
        if (finder == null)
        {
            PluginService.PluginLog.Debug("游戏 ItemFinder 缓存不可用，跳过刷新");
            return;
        }

        RetainerData.Clear();
        _itemTotalCache.Clear();
        DataCached = false;

        var allItemIds = CollectTrackedItemIds();

        // 雇员背包：遍历 ItemFinder 缓存中每个雇员，按格累计数量
        var scannedRetainerCount = 0;
        foreach (var entry in finder->RetainerInventories)
        {
            if (entry.Item2.IsNull)
                continue;

            var retainerInventory = entry.Item2.Value;
            if (retainerInventory == null)
                continue;

            if (!RetainerData.TryGetValue(entry.Item1, out var dict))
            {
                dict = new Dictionary<uint, ItemInfo>();
                RetainerData[entry.Item1] = dict;
            }

            for (var slot = 0; slot < retainerInventory->ItemIds.Length; slot++)
            {
                var itemId = NormalizeItemId(retainerInventory->ItemIds[slot]);
                if (itemId == 0 || !allItemIds.Contains(itemId))
                    continue;

                var quantity = retainerInventory->ItemCount[slot];
                if (quantity == 0)
                    continue;

                AddItemQuantity(dict, itemId, quantity);
            }

            // 雇员装备位每件只占一格，直接按 1 计
            foreach (var rawItemId in retainerInventory->EquippedItemIds)
            {
                var itemId = NormalizeItemId(rawItemId);
                if (itemId == 0 || !allItemIds.Contains(itemId))
                    continue;

                AddItemQuantity(dict, itemId, 1);
            }

            scannedRetainerCount++;
        }

        // 投影台：每格存放一件武器/装备，命中即按 1 计
        var dresserMatchCount = 0;
        foreach (var rawItemId in finder->GlamourDresserItemIds)
        {
            var itemId = NormalizeItemId(rawItemId);
            if (itemId == 0 || !allItemIds.Contains(itemId))
                continue;

            _itemTotalCache.TryGetValue(itemId, out var existing);
            _itemTotalCache[itemId] = existing + 1;
            dresserMatchCount++;
        }

        // 预计算每个物品的雇员合计数量，后续查询直接读缓存
        foreach (var dict in RetainerData.Values)
            foreach (var info in dict.Values)
            {
                _itemTotalCache.TryGetValue(info.ItemId, out var existing);
                _itemTotalCache[info.ItemId] = existing + (int)info.Quantity;
            }

        DataCached = true;
        PluginService.PluginLog.Debug(
            $"ItemFinder 缓存扫描完成：{scannedRetainerCount} 个雇员，投影台命中 {dresserMatchCount} 件");
    }

    private static void ClearCachedData(int _, int __)
    {
        RetainerData.Clear();
        _itemTotalCache.Clear();
        DataCached = false;
    }

    /// <summary>
    /// IGameInventory.ItemAdded 回调：玩家获得追踪武器时（不含 move/split/merge 位置变动），
    /// 延迟到下一帧执行一次 RefreshCache。同帧内多次事件只触发一次。
    /// </summary>
    private static void OnItemAdded(GameInventoryEvent type, InventoryEventArgs data)
    {
        _trackedWeaponIds ??= BuildTrackedWeaponIds();
        if (!_trackedWeaponIds.Contains(data.Item.BaseItemId))
            return;

        if (_refreshPending) return;
        _refreshPending = true;
        PluginService.Framework.RunOnTick(ExecutePendingRefresh);
    }

    /// <summary>延迟执行缓存刷新并通知 UI 层。RunOnTick 保证在下一 Framework.Tick 执行。</summary>
    private static void ExecutePendingRefresh()
    {
        _refreshPending = false;
        RefreshCache();
        CacheAutoRefreshed?.Invoke();
    }

    private static HashSet<uint> BuildTrackedWeaponIds()
    {
        var set = new HashSet<uint>();
        foreach (var kvp in WeaponSeriesInfo.All)
            foreach (var stage in kvp.Value.WeaponIdStages)
                foreach (var itemId in stage)
                    set.Add(itemId);
        return set;
    }

    /// <summary>收集需要缓存的全部物品 ID：追踪武器 + 配方材料。</summary>
    private static HashSet<uint> CollectTrackedItemIds()
    {
        var allItemIds = BuildTrackedWeaponIds();

        // 从配方表自动收集所有材料 ID，避免与 DataBase 重复维护
        foreach (var recipe in DataBase.BozjaMaterialRecipes
                     .Concat(DataBase.MandervillousMaterialRecipes)
                     .Concat(DataBase.PhantomMaterialRecipes)
                     .Concat(DataBase.EurekaMaterialRecipes)
                     .SelectMany(stage => stage))
        {
            allItemIds.Add(recipe.ItemId);
        }

        return allItemIds;
    }

    /// <summary>把 ItemFinder 缓存中的 HQ/收藏品高位编码还原为基准 ItemId。</summary>
    private static uint NormalizeItemId(uint itemId)
        => itemId >= 1_000_000 ? itemId % 1_000_000 : itemId;

    private static void AddItemQuantity(Dictionary<uint, ItemInfo> dict, uint itemId, uint quantity)
    {
        if (dict.TryGetValue(itemId, out var info))
            info.Quantity += quantity;
        else
            dict[itemId] = new ItemInfo(itemId, quantity);
    }

    // ---- 雇员背包 + 投影台缓存 ----
    internal static readonly Dictionary<ulong, Dictionary<uint, ItemInfo>> RetainerData = new();

    /// <summary>获取某物品在雇员背包（含雇员装备位）与投影台中的缓存合计数量。</summary>
    private static unsafe int GetRetainerItemCount(uint itemId)
    {
        if (!ItemFinderReady) return 0;
        if (!PluginService.ClientState.IsLoggedIn || PluginService.Condition[ConditionFlag.OnFreeTrial])
            return 0;

        // 未完成全量缓存时，触发一次批量刷新（同步完成后 DataCached=true，后续调用直接读缓存）
        if (!DataCached)
        {
            try
            {
                RefreshCache();
            }
            catch (Exception ex)
            {
                PluginService.PluginLog?.Warning($"获取雇员/投影台缓存数据异常: {ex.Message}");
                return 0;
            }
        }

        return GetCachedSum(itemId);
    }

    private static int GetCachedSum(uint itemId) =>
        _itemTotalCache.TryGetValue(itemId, out var total) ? total : 0;

    public static unsafe int GetItemCountTotal(uint itemId)
    {
        var countInCachedStorage = GetRetainerItemCount(itemId);
        var inventoryManager     = InventoryManager.Instance();
        var countInBag           = inventoryManager->GetInventoryItemCount(itemId);
        var countInSaddleBag     = inventoryManager->GetItemCountInContainer(itemId, InventoryType.SaddleBag1)
                                 + inventoryManager->GetItemCountInContainer(itemId, InventoryType.SaddleBag2);
        return countInCachedStorage + countInBag + countInSaddleBag;
    }

    public static void Dispose()
    {
        PluginService.GameInventory.ItemAdded -= OnItemAdded;
        CacheAutoRefreshed = null;
        PluginService.ClientState.Logout -= ClearCachedData;
        _refreshPending = false;
    }
}
