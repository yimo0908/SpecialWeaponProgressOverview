using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Data.Providers;

namespace SpecialWeaponProgressOverview.Drawer;

public static class DrawMethod
{
    private static readonly ExcelSheet<Item> ItemSheet = PluginService.DataManager.GetExcelSheet<Item>();

    /// <summary>物品缓存条目：缓存元数据（名称、稀有度、图标 ID）+ 纹理引用。</summary>
    private sealed class ItemCacheEntry(string name, uint rarity, uint iconId)
    {
        public string      Name        { get; } = name;
        public uint        Rarity      { get; } = rarity;
        public uint        IconId      { get; } = iconId;
        public IconHandler IconHandler { get; } = new(iconId);
    }

    private static readonly Dictionary<uint, ItemCacheEntry> _itemCache = new();

    /// <summary>获取或创建物品缓存条目，首次访问后复用，避免每帧重复查表。</summary>
    private static ItemCacheEntry GetOrCreateCacheEntry(uint itemId)
    {
        if (_itemCache.TryGetValue(itemId, out var cached))
            return cached;

        if (!ItemSheet.TryGetRow(itemId, out var itemRow))
        {
            PluginService.PluginLog.Warning($"无法找到物品 ID {itemId}，使用占位条目");
            var fallback = new ItemCacheEntry($"(未知物品 #{itemId})", 0, 0);
            _itemCache[itemId] = fallback;
            return fallback;
        }

        var entry = new ItemCacheEntry(itemRow.Name.ExtractText(), itemRow.Rarity, itemRow.Icon);
        _itemCache[itemId] = entry;
        return entry;
    }

    private static void CopyItemNameToClipboard(string itemName)
    {
        ImGui.SetClipboardText(itemName);
    }

    private static void PrintItemPayload(uint itemId, in ItemCacheEntry entry)
    {
        var payloadList = new List<Payload>
        {
            new UIForegroundPayload((ushort)(0x223 + entry.Rarity * 2)),
            new UIGlowPayload((ushort)(0x224 + entry.Rarity * 2)),
            new ItemPayload(itemId, false),
            new UIForegroundPayload(500),
            new UIGlowPayload(501),
            new TextPayload($"{(char)SeIconChar.LinkMarker}"),
            new UIForegroundPayload(0),
            new UIGlowPayload(0),
            new TextPayload(entry.Name),
            new RawPayload([0x02, 0x27, 0x07, 0xCF, 0x01, 0x01, 0x01, 0xFF, 0x01, 0x03]),
            new RawPayload([0x02, 0x13, 0x02, 0xEC, 0x03])
        };
        var messagePayloads = new List<Payload>(payloadList);
        messagePayloads.AddRange([new TextPayload(" 已复制到剪切板")]);
        var fullMessage = new SeString(messagePayloads);
        PluginService.ChatGui.Print(new XivChatEntry { Message = fullMessage });
    }

    public static void DrawWeaponCell(int count, uint itemId, Vector4? nameColor = null)
    {
        var entry = GetOrCreateCacheEntry(itemId);

        var iconSize  = new Vector2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight());
        var tintColor = count > 0
            ? new Vector4(1, 1, 1, 1)
            : new Vector4(0.35f, 0.35f, 0.35f, 1);

        // 使用 IconHandler 缓存的 ISharedImmediateTexture，避免每帧重复调用 GetFromGameIcon
        var wrap = entry.IconHandler.GetIcon().GetWrapOrEmpty();

        ImGui.Image(wrap.Handle, iconSize, Vector2.Zero, Vector2.One, tintColor, Vector4.Zero);
        ImGui.SameLine();
        ImGui.TextColored(nameColor ?? tintColor, entry.Name);

        if (ImGui.IsItemClicked())
        {
            CopyItemNameToClipboard(entry.Name);
            PrintItemPayload(itemId, entry);
        }
    }
}
