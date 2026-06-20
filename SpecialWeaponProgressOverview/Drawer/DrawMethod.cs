using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Textures;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer;

public static class DrawMethod
{
    private static readonly ExcelSheet<Item> ItemSheet = PluginService.DataManager.GetExcelSheet<Item>();
    
    private static void CopyItemNameToClipboard(uint itemId)
    {
        var itemName = ItemSheet.GetRow(itemId).Name.ExtractText();
        ImGui.SetClipboardText(itemName);
    }

    private static object GetTextureHandle(object wrap)
    {
        if (wrap == null) return IntPtr.Zero;

        var t = wrap.GetType();

        // Try common property names
        foreach (var name in new[] { "ImGuiHandle", "NativePtr", "NativePointer", "Native", "TextureId", "Handle", "Id" })
        {
            var prop = t.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
            {
                var v = prop.GetValue(wrap);
                if (v != null) return v;
            }
        }

        // Try fields
        foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var fv = f.GetValue(wrap);
            if (fv != null) return fv;
        }

        return IntPtr.Zero;
    }


    
    private static void PrintItemPayload(uint itemId)
    {
        var itemRow = ItemSheet.GetRow(itemId);
        var payloadList = new List<Payload>
        {
            new UIForegroundPayload((ushort)(0x223 + itemRow.Rarity * 2)),
            new UIGlowPayload((ushort)(0x224 + itemRow.Rarity * 2)),
            new ItemPayload(itemId, false),
            new UIForegroundPayload(500),
            new UIGlowPayload(501),
            new TextPayload($"{(char)SeIconChar.LinkMarker}"),
            new UIForegroundPayload(0),
            new UIGlowPayload(0),
            new TextPayload(itemRow.Name.ExtractText()),
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
        var itemRow = ItemSheet.GetRow(itemId);
        var iconId = itemRow.Icon;
        var texture = PluginService.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId)).GetWrapOrEmpty();

        var iconSize = new Vector2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight());
        var tintColor = count > 0
            ? new Vector4(1, 1, 1, 1)
            : new Vector4(0.35f, 0.35f, 0.35f, 1);

        var handle = GetTextureHandle(texture);
        ImGui.Image((dynamic)handle, iconSize, Vector2.Zero, Vector2.One, tintColor, Vector4.Zero);
        ImGui.SameLine();
        ImGui.TextColored(nameColor ?? tintColor, itemRow.Name.ExtractText());

        if (ImGui.IsItemClicked())
        {
            CopyItemNameToClipboard(itemId);
            PrintItemPayload(itemId);
        }
    }

}
