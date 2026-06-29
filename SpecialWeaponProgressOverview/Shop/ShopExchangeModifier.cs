using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Shop;

/// <summary>
/// 监听 ShopExchangeCurrency 商店 UI，在曼武/幻武素材行的持有数量列
/// 追加显示 "/共需数量"。
/// </summary>
public sealed unsafe class ShopExchangeModifier : IDisposable
{
    private static readonly ExcelSheet<Item> ItemSheet =
        PluginService.DataManager.GetExcelSheet<Item>();

    /// <summary>素材名称 → ItemId（静态）</summary>
    private static readonly Dictionary<string, uint> NameToItemId = BuildNameToItemId();

    /// <summary>itemId → 共需数量（运行时计算）</summary>
    private Dictionary<uint, int> _materialRequired = new();

    private bool _shopWasVisible;
    private DateTime _lastComputeTime = DateTime.MinValue;

    public ShopExchangeModifier()
    {
        PluginService.Framework.Update += OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        var addonPtr = PluginService.GameGui.GetAddonByName("ShopExchangeCurrency", 1);
        if (addonPtr.IsNull || !addonPtr.IsVisible)
        {
            _shopWasVisible = false;
            return;
        }

        var addon = (AtkUnitBase*)addonPtr.Address;

        if (!_shopWasVisible || (DateTime.Now - _lastComputeTime).TotalSeconds >= 5)
        {
            EnsureMaterialRequiredComputed();
            _lastComputeTime = DateTime.Now;
        }
        _shopWasVisible = true;

        ModifyShopText(addon);
    }

    private void EnsureMaterialRequiredComputed()
    {
        _materialRequired.Clear();

        foreach (var series in new[] { WeaponSeries.Mandervillous, WeaponSeries.Phantom })
        {
            var info = WeaponSeriesInfo.All[series];
            var recipes = info.Series switch
            {
                WeaponSeries.Mandervillous => DataBase.MandervillousMaterialRecipes,
                WeaponSeries.Phantom       => DataBase.PhantomMaterialRecipes,
                _ => null,
            };
            if (recipes == null) continue;

            var (materialIds, neededList, _) =
                Compute.ComputeMaterialNeedsCore(info, recipes, Inventory.GetItemCountTotal);

            for (var i = 0; i < materialIds.Count; i++)
                _materialRequired[materialIds[i]] = neededList[i];
        }
    }

    /// <summary>
    /// 遍历商店列表行，修改素材行的持有数量文本。
    /// 结构：addon → 列表容器(Type≥1000) → 数据行(Type≥1000) → 文本节点
    /// </summary>
    private void ModifyShopText(AtkUnitBase* addon)
    {
        var uld = addon->UldManager;
        for (var i = 0; i < uld.NodeListCount; i++)
        {
            var node = uld.NodeList[i];
            if (node == null || (ushort)node->Type < 1000) continue;

            var compNode = (AtkComponentNode*)node;
            if (compNode->Component == null) continue;

            var compUld = compNode->Component->UldManager;
            for (var j = 0; j < compUld.NodeListCount; j++)
            {
                var child = compUld.NodeList[j];
                if (child == null || (ushort)child->Type < 1000) continue;

                var rowComp = (AtkComponentNode*)child;
                if (rowComp->Component != null)
                    ProcessRow(rowComp);
            }
        }
    }

    private void ProcessRow(AtkComponentNode* row)
    {
        var rowUld = row->Component->UldManager;
        var textNodes = new List<(nint node, string text)>();
        CollectTextNodes(rowUld.NodeList, rowUld.NodeListCount, textNodes);

        // 查找素材名称 → itemId → 共需量
        nint nameNodePtr = 0;
        int required = 0;

        foreach (var (nodePtr, text) in textNodes)
        {
            if (string.IsNullOrEmpty(text)) continue;
            foreach (var kvp in NameToItemId)
            {
                if (!text.Contains(kvp.Key)) continue;
                if (!_materialRequired.TryGetValue(kvp.Value, out required)) break;
                nameNodePtr = nodePtr;
                break;
            }
            if (nameNodePtr != 0) break;
        }

        if (nameNodePtr == 0) return;

        // 查找持有数量文本并修改
        foreach (var (nodePtr, text) in textNodes)
        {
            if (nodePtr == nameNodePtr) continue;

            var numStr = text;
            var slashIdx = text.IndexOf('/');
            if (slashIdx >= 0)
                numStr = text[..slashIdx];

            if (!int.TryParse(numStr, out _)) continue;

            var newText = $"{numStr}/{required}";
            if (text != newText)
                SetNodeText((AtkTextNode*)nodePtr, newText);
            break;
        }
    }

    private static void CollectTextNodes(
        AtkResNode** nodeList, int count,
        List<(nint node, string text)> result)
    {
        for (var i = 0; i < count; i++)
        {
            var node = nodeList[i];
            if (node == null) continue;

            if (node->Type == NodeType.Text)
            {
                var textNode = (AtkTextNode*)node;
                result.Add(((nint)textNode, textNode->NodeText.ToString()));
            }
            else if ((ushort)node->Type >= 1000)
            {
                var compNode = (AtkComponentNode*)node;
                if (compNode->Component != null)
                    CollectTextNodes(
                        compNode->Component->UldManager.NodeList,
                        compNode->Component->UldManager.NodeListCount,
                        result);
            }
        }
    }

    private static void SetNodeText(AtkTextNode* textNode, string text)
    {
        var bytes = new byte[Encoding.UTF8.GetByteCount(text) + 1];
        Encoding.UTF8.GetBytes(text, 0, text.Length, bytes, 0);

        fixed (byte* ptr = bytes)
        {
            textNode->SetText(ptr);
        }
    }

    private static Dictionary<string, uint> BuildNameToItemId()
    {
        var dict = new Dictionary<string, uint>();
        foreach (var stage in DataBase.MandervillousMaterialRecipes)
            foreach (var (itemId, _) in stage)
                dict.TryAdd(ItemSheet.GetRow(itemId).Name.ExtractText(), itemId);
        foreach (var stage in DataBase.PhantomMaterialRecipes)
            foreach (var (itemId, _) in stage)
                dict.TryAdd(ItemSheet.GetRow(itemId).Name.ExtractText(), itemId);
        return dict;
    }

    public void Dispose()
    {
        PluginService.Framework.Update -= OnFrameworkUpdate;
    }
}
