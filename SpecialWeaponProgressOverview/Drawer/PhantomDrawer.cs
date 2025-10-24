using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer
{
    public class PhantomDrawer(
        List<List<uint>> phantomWeaponId,
        List<uint> phantomWeaponJobIdList,
        Dictionary<uint, List<int>> phantomWeaponProcess)
    {
        private static readonly ExcelSheet<ClassJob>
            ClassJobSheet = PluginService.DataManager.GetExcelSheet<ClassJob>();

        public void Draw()
        {
            ImGui.Text($"{Compute.ComputeNeedsPhantom(phantomWeaponProcess, phantomWeaponJobIdList)}");
            ImGui.BeginTable("PhantomWeaponChart", phantomWeaponId.Count + 1,
                             ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("职业");
            ImGui.TableSetupColumn(label: "幻境武器·半影");
            ImGui.TableSetupColumn(label: "幻境武器·本影");
            ImGui.TableHeadersRow();

            foreach (var jobId in phantomWeaponJobIdList)
            {
                var line = phantomWeaponProcess[jobId];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(ClassJobSheet.GetRow(jobId).Name.ExtractText());

                for (var j = 0; j < line.Count; j++)
                {
                    ImGui.TableNextColumn();
                    DrawMethod.DrawWeaponCell(line[j], phantomWeaponId[j][DataBase.JobIndex[jobId]]);
                }
            }

            ImGui.EndTable();
        }
    }
}
