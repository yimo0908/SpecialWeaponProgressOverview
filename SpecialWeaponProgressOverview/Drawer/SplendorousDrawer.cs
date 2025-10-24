using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer
{
    public class SplendorousDrawer(
        List<List<uint>> splendorousWeaponId,
        List<uint> splendorousWeaponJobIdList,
        Dictionary<uint, List<int>> splendorousWeaponProcess)
    {
        private static readonly ExcelSheet<ClassJob>
            ClassJobSheet = PluginService.DataManager.GetExcelSheet<ClassJob>();

        public void Draw()
        {
            ImGui.Text("");
            ImGui.BeginTable("SplendorousWeaponChart", splendorousWeaponId.Count + 1,
                             ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("职业");
            ImGui.TableSetupColumn("卓越工具");
            ImGui.TableSetupColumn("改良型卓越工具");
            ImGui.TableSetupColumn("水晶工具");
            ImGui.TableSetupColumn("乔菈水晶工具");
            ImGui.TableSetupColumn("乔菈卓绝工具");
            ImGui.TableSetupColumn("诺弗兰特远见工具");
            ImGui.TableSetupColumn("领航星工具");
            ImGui.TableHeadersRow();

            foreach (var jobId in splendorousWeaponJobIdList)
            {
                var line = splendorousWeaponProcess[jobId];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(ClassJobSheet.GetRow(jobId).Name.ExtractText());

                for (var j = 0; j < line.Count; j++)
                {
                    ImGui.TableNextColumn();
                    DrawMethod.DrawWeaponCell(line[j], splendorousWeaponId[j][DataBase.LifeJobIndex[jobId]]);
                }
            }

            ImGui.EndTable();
        }
    }
}
