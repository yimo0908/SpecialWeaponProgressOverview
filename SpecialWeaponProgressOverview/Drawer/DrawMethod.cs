using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer;

public class DrawMethod
{
    private static readonly ExcelSheet<Item> ItemSheet = PluginService.DataManager.GetExcelSheet<Item>();
    
    private static void CopyItemNameToClipboard(uint itemId)
    {
        var itemName = ItemSheet.GetRow(itemId).Name.ExtractText();
        ImGui.SetClipboardText(itemName);
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
    
    public static void DrawWeaponCell(int count, uint itemId)
    {
        Vector4 color = count > 0 ? new(0, 1, 0, 1) : new(0.5f, 0.5f, 0.5f, 1);
        string displayText = count > 0 ? "●" : "◯";
        ImGui.TextColored(color, displayText);

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextColored(color, ItemSheet.GetRow(itemId).Name.ToString());
            ImGui.EndTooltip();
        }

        if (ImGui.IsItemClicked())
        {
            CopyItemNameToClipboard(itemId);
            PrintItemPayload(itemId);
        }
    }
}
