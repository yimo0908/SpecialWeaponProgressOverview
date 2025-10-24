using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer
{
    public class SkysteelDrawer(
        List<List<uint>> skysteelWeaponId,
        List<uint> skysteelWeaponJobIdList,
        Dictionary<uint, List<int>> skysteelWeaponProcess)
    {
        private static readonly ExcelSheet<ClassJob>
            ClassJobSheet = PluginService.DataManager.GetExcelSheet<ClassJob>();

        public void Draw()
        {
            ImGui.Text("");
            ImGui.BeginTable("SkysteelWeaponChart", skysteelWeaponId.Count + 1,
                             ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("职业");
            ImGui.TableSetupColumn("天钢工具");
            ImGui.TableSetupColumn("天钢工具+1");
            ImGui.TableSetupColumn("龙诗工具");
            ImGui.TableSetupColumn("改良型龙诗工具");
            ImGui.TableSetupColumn("天诗工具");
            ImGui.TableSetupColumn("天工工具");
            ImGui.TableHeadersRow();

            foreach (var jobId in skysteelWeaponJobIdList)
            {
                var line = skysteelWeaponProcess[jobId];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(ClassJobSheet.GetRow(jobId).Name.ExtractText());

                for (var j = 0; j < line.Count; j++)
                {
                    ImGui.TableNextColumn();
                    DrawMethod.DrawWeaponCell(line[j], skysteelWeaponId[j][DataBase.LifeJobIndex[jobId]]);
                }
            }

            ImGui.EndTable();
        }
    }
}
