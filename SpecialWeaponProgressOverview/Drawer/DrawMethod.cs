using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Data.Providers;

namespace SpecialWeaponProgressOverview.Drawer;

public static class DrawMethod
{
    /// <summary>物品缓存条目：缓存元数据（名称、稀有度、图标 ID）+ 纹理引用 + 预构建聊天消息。</summary>
    private sealed class ItemCacheEntry(uint itemId, string name, uint rarity, uint iconId)
    {
        public string      Name        { get; } = name;
        public uint        Rarity      { get; } = rarity;
        public uint        IconId      { get; } = iconId;
        public IconHandler IconHandler { get; } = new(iconId);
        public SeString    ChatMessage { get; } = BuildChatMessage(itemId, name, rarity);

        private static SeString BuildChatMessage(uint itemId, string name, uint rarity)
        {
            var payloads = new List<Payload>
            {
                new UIForegroundPayload((ushort)(0x223 + rarity * 2)),
                new UIGlowPayload((ushort)(0x224 + rarity * 2)),
                new ItemPayload(itemId, false),
                new UIForegroundPayload(500),
                new UIGlowPayload(501),
                new TextPayload($"{(char)SeIconChar.LinkMarker}"),
                new UIForegroundPayload(0),
                new UIGlowPayload(0),
                new TextPayload(name),
                new RawPayload([0x02, 0x27, 0x07, 0xCF, 0x01, 0x01, 0x01, 0xFF, 0x01, 0x03]),
                new RawPayload([0x02, 0x13, 0x02, 0xEC, 0x03]),
                new TextPayload(" 已复制到剪切板")
            };
            return new SeString(payloads);
        }
    }

    private static readonly Dictionary<uint, ItemCacheEntry> _itemCache = new();

    /// <summary>获取或创建物品缓存条目，首次访问后复用，避免每帧重复查表。</summary>
    private static ItemCacheEntry GetOrCreateCacheEntry(uint itemId)
    {
        if (_itemCache.TryGetValue(itemId, out var cached))
            return cached;

        if (!PluginService.ItemSheet.TryGetRow(itemId, out var itemRow))
        {
            PluginService.PluginLog.Warning($"无法找到物品 ID {itemId}，使用占位条目");
            var fallback = new ItemCacheEntry(itemId, $"(未知物品 #{itemId})", 0, 0);
            _itemCache[itemId] = fallback;
            return fallback;
        }

        var entry = new ItemCacheEntry(itemId, itemRow.Name.ExtractText(), itemRow.Rarity, itemRow.Icon);
        _itemCache[itemId] = entry;
        return entry;
    }

    private static void CopyItemNameToClipboard(string itemName)
    {
        ImGui.SetClipboardText(itemName);
    }

    private static void PrintItemPayload(in ItemCacheEntry entry)
    {
        PluginService.ChatGui.Print(new XivChatEntry { Message = entry.ChatMessage });
    }

    /// <summary>绘制指定物品的小图标（行内高度），复用缓存。</summary>
    public static void DrawItemIcon(uint itemId)
    {
        var entry  = GetOrCreateCacheEntry(itemId);
        var iconSize = new Vector2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight());
        var wrap = entry.IconHandler.GetIcon().GetWrapOrEmpty();
        ImGui.Image(wrap.Handle, iconSize, Vector2.Zero, Vector2.One,
                    new Vector4(1, 1, 1, 1), Vector4.Zero);
    }

    /// <summary>获取指定物品名称（复用缓存）。</summary>
    public static string GetItemName(uint itemId)
    {
        return GetOrCreateCacheEntry(itemId).Name;
    }

    /// <summary>武器图标边长（2倍行高）。</summary>
    public static float WeaponIconSize => ImGui.GetTextLineHeight() * 2f;

    /// <summary>武器单元格宽度（5倍行高），供 Drawer 设置列宽。</summary>
    public static float WeaponCellWidth => ImGui.GetTextLineHeight() * 5f;

    /// <summary>武器单元格高度（2.5倍行高）。</summary>
    public static float WeaponCellHeight => ImGui.GetTextLineHeight() * 2.5f;

    /// <summary>在当前单元格内居中绘制武器图标（2倍大小+边框）。</summary>
    public static void DrawWeaponCell(int count, uint itemId, Vector4? nameColor = null)
    {
        var entry = GetOrCreateCacheEntry(itemId);

        var iconSize  = new Vector2(WeaponIconSize, WeaponIconSize);
        // 像素风格：拥有时全亮，未拥有时暗淡
        var tintColor = count > 0
            ? new Vector4(1, 1, 1, 1)
            : PixelStyle.Dim;

        // 使用 IconHandler 缓存的 ISharedImmediateTexture，避免每帧重复调用 GetFromGameIcon
        var wrap = entry.IconHandler.GetIcon().GetWrapOrEmpty();

        // 居中绘制
        var avail = ImGui.GetContentRegionAvail().X;
        var offset = Math.Max(0f, (avail - iconSize.X) / 2f);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);

        ImGui.Image(wrap.Handle, iconSize, Vector2.Zero, Vector2.One, tintColor, PixelStyle.IconBorder);

        // 点击图标复制名称到剪贴板
        if (ImGui.IsItemClicked())
        {
            CopyItemNameToClipboard(entry.Name);
            PrintItemPayload(entry);
        }

        // 悬停显示武器名称
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(entry.Name);
    }
}
