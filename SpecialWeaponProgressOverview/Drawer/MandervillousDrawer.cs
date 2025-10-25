using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer
{
    public class MandervillousDrawer(
        List<List<uint>> mandervillousWeaponId,
        List<uint> mandervillousWeaponJobIdList,
        Dictionary<uint, List<int>> mandervillousWeaponProcess,
        MainWindow.ItemCountDelegate getItemCountTotal)
    {
        private static readonly ExcelSheet<ClassJob>
            ClassJobSheet = PluginService.DataManager.GetExcelSheet<ClassJob>();

        public void Draw()
        {
            var funcAdapter = new System.Func<uint, int>(id => getItemCountTotal(id));
            ImGui.Text(
                $"{Compute.ComputeNeedsMandervillous(mandervillousWeaponId, mandervillousWeaponJobIdList, funcAdapter)}");
            ImGui.BeginTable("MandervillousWeaponChart", mandervillousWeaponId.Count + 1,
                             ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("职业");
            ImGui.TableSetupColumn("曼德维尔武器");
            ImGui.TableSetupColumn("曼德维尔武器·惊异");
            ImGui.TableSetupColumn("曼德维尔武器·威严");
            ImGui.TableSetupColumn("曼德维尔武器·盈满");
            ImGui.TableHeadersRow();

            foreach (var jobId in mandervillousWeaponJobIdList)
            {
                var line = mandervillousWeaponProcess[jobId];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(ClassJobSheet.GetRow(jobId).Name.ExtractText());

                for (var j = 0; j < line.Count; j++)
                {
                    ImGui.TableNextColumn();
                    DrawMethod.DrawWeaponCell(line[j], mandervillousWeaponId[j][DataBase.JobIndex[jobId]]);
                }
            }

            ImGui.EndTable();
        }
    }
}
