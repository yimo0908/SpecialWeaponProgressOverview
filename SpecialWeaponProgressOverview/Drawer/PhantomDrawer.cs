using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Data;

namespace SpecialWeaponProgressOverview.Drawer
{
    public class PhantomDrawer(
        List<List<uint>> phantomWeaponId,
        List<uint> phantomWeaponJobIdList,
        Dictionary<uint, List<int>> phantomWeaponProcess,
        MainWindow.ItemCountDelegate getItemCountTotal)
    {
        private static readonly ExcelSheet<ClassJob>
            ClassJobSheet = PluginService.DataManager.GetExcelSheet<ClassJob>();

        public void Draw()
        {
            var funcAdapter = new Func<uint, int>(id => getItemCountTotal(id));
            ImGui.Text($"{Compute.ComputeNeedsPhantom(phantomWeaponProcess, phantomWeaponJobIdList, funcAdapter)}");
            ImGui.BeginTable("PhantomWeaponChart", phantomWeaponId.Count + 1,
                             ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("职业");
            ImGui.TableSetupColumn(label: "幻境武器·半影");
            ImGui.TableSetupColumn(label: "幻境武器·本影");
            ImGui.TableSetupColumn(label: "幻境武器·黯影");
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
                    DrawMethod.DrawWeaponCell(line[j], phantomWeaponId[j][DataBase.NewJobIndex[jobId]]);
                }
            }

            ImGui.EndTable();
        }
    }
}
