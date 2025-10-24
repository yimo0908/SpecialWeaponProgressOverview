using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer
{
    public class BozjaDrawer(
        List<List<uint>> bozjaWeaponId,
        List<uint> bozjaWeaponJobIdList,
        Dictionary<uint, List<int>> bozjaWeaponProcess,
        InventoryWindow.ItemCountDelegate getItemCountTotal)
    {
        private static readonly ExcelSheet<ClassJob>
            ClassJobSheet = PluginService.DataManager.GetExcelSheet<ClassJob>();
        
        public void Draw()
        {
            var funcAdapter = new System.Func<uint, int>(id => getItemCountTotal(id));
            ImGui.Text(
                $"{Compute.ComputeNeedsBozja(bozjaWeaponProcess, bozjaWeaponId, bozjaWeaponJobIdList, funcAdapter)}");
            ImGui.BeginTable("BozjaWeaponChart", bozjaWeaponId.Count + 1,
                             ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("职业");
            ImGui.TableSetupColumn("义军武器");
            ImGui.TableSetupColumn("改良型义军武器");
            ImGui.TableSetupColumn("回忆");
            ImGui.TableSetupColumn("裁决");
            ImGui.TableSetupColumn("改良型裁决");
            ImGui.TableSetupColumn("女王武器");
            ImGui.TableHeadersRow();

            foreach (var jobId in bozjaWeaponJobIdList)
            {
                var line = bozjaWeaponProcess[jobId];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(ClassJobSheet.GetRow(jobId).Name.ExtractText());

                for (var j = 0; j < line.Count; j++)
                {
                    ImGui.TableNextColumn();
                    DrawMethod.DrawWeaponCell(line[j], bozjaWeaponId[j][DataBase.JobIndex[jobId]]);
                }
            }

            ImGui.EndTable();
        }
    }
}
