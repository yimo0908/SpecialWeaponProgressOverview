using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Ipc;
using FFXIVClientStructs.FFXIV.Client.Game;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Data;

public static class Inventory
{
    private static ICallGateSubscriber<uint, ulong, uint, uint>? ItemCount;
    private static ICallGateSubscriber<bool, bool>? Initialized;
    private static ICallGateSubscriber<bool>? IsInitialized;
    
    public static bool AToolsInstalled
    {
        get
        {
            return PluginService.PluginInterface.InstalledPlugins.Any(x => x.InternalName is "Allagan Tools"
                                                                            or "InventoryTools");
        }
    }

    public static bool AToolsEnabled => AToolsInstalled && IsInitialized != null && IsInitialized.InvokeFunc();

    public static bool ATools => AToolsEnabled;

    internal static void Init()
    {
        Initialized = PluginService.PluginInterface.GetIpcSubscriber<bool, bool>("AllaganTools.Initialized");
        IsInitialized = PluginService.PluginInterface.GetIpcSubscriber<bool>("AllaganTools.IsInitialized");
        Initialized.Subscribe(SetupIpc);
        PluginService.ClientState.Logout += LogoutCacheClear;
        SetupIpc(true);
    }

    private static void LogoutCacheClear(int a, int b)
    {
        RetainerData.Clear();
    }

    private static void SetupIpc(bool obj)
    {
        ItemCount = PluginService.PluginInterface.GetIpcSubscriber<uint, ulong, uint, uint>("AllaganTools.ItemCount");
    }

    public static readonly Dictionary<ulong, Dictionary<uint, InventoryWindow.ItemInfo>> RetainerData =
        new();

    public static uint GetRetainerInventoryItem(uint itemId, ulong retainerId)
    {
        if (ATools && ItemCount != null)
        {
            return ItemCount.InvokeFunc(itemId, retainerId, 10000) +
                   ItemCount.InvokeFunc(itemId, retainerId, 10001) +
                   ItemCount.InvokeFunc(itemId, retainerId, 10002) +
                   ItemCount.InvokeFunc(itemId, retainerId, 10003) +
                   ItemCount.InvokeFunc(itemId, retainerId, 10004) +
                   ItemCount.InvokeFunc(itemId, retainerId, 10005) +
                   ItemCount.InvokeFunc(itemId, retainerId, 10006) +
                   ItemCount.InvokeFunc(itemId, retainerId, (uint)InventoryType.RetainerCrystals);
        }

        return 0;
    }

    public static unsafe int GetRetainerItemCount(uint itemId, bool tryCache = true)
    {
        if (ATools)
        {
            if (!PluginService.ClientState.IsLoggedIn || PluginService.Condition[ConditionFlag.OnFreeTrial]) return 0;

            try
            {
                if (tryCache)
                {
                    if (RetainerData.SelectMany(x => x.Value).Any(x => x.Key == itemId))
                    {
                        return (int)RetainerData.Values.SelectMany(x => x.Values).Where(x => x.ItemId == itemId)
                                                .Sum(x => x.Quantity);
                    }
                }

                for (var i = 0; i < 10; i++)
                {
                    var retainer = RetainerManager.Instance()->GetRetainerBySortedIndex((uint)i);

                    var retainerId = retainer->RetainerId;

                    if (retainerId > 0 && retainer->Available)
                    {
                        if (RetainerData.TryGetValue(retainerId, out var ret))
                        {
                            if (ret.TryGetValue(itemId, out var item))
                            {
                                item.ItemId = itemId;
                                item.Quantity = GetRetainerInventoryItem(itemId, retainerId);
                            }
                            else
                            {
                                ret.TryAdd(
                                    itemId,
                                    new InventoryWindow.ItemInfo(itemId, GetRetainerInventoryItem(itemId, retainerId)));
                            }
                        }
                        else
                        {
                            RetainerData.TryAdd(retainerId, new Dictionary<uint, InventoryWindow.ItemInfo>());
                            var newret = RetainerData[retainerId];
                            if (newret.TryGetValue(itemId, out var item))
                            {
                                item.ItemId = itemId;
                                item.Quantity = GetRetainerInventoryItem(itemId, retainerId);
                            }
                            else
                            {
                                newret.TryAdd(
                                    itemId,
                                    new InventoryWindow.ItemInfo(itemId, GetRetainerInventoryItem(itemId, retainerId)));
                            }
                        }
                    }
                }

                return (int)RetainerData.SelectMany(x => x.Value).Where(x => x.Key == itemId)
                                        .Sum(x => x.Value.Quantity);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        return 0;
    }

    public static unsafe int GetItemCountTotal(uint itemId)
    {
        var countInRetainers = GetRetainerItemCount(itemId);
        var inventoryManager = InventoryManager.Instance();
        var countInBag = inventoryManager->GetInventoryItemCount(itemId);
        var countInSaddleBag = inventoryManager->GetItemCountInContainer(itemId, InventoryType.SaddleBag1)
                               + inventoryManager->GetItemCountInContainer(itemId, InventoryType.SaddleBag2);
        return countInRetainers + countInBag + countInSaddleBag;
    }

    public static void Dispose()
    {
        Initialized?.Unsubscribe(SetupIpc);
        PluginService.ClientState.Logout -= LogoutCacheClear;
        Initialized = null;
        IsInitialized = null;
        ItemCount = null;
    }
}
