using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer
{
    public class AnimaDrawer(
        List<List<uint>> animaWeaponId,
        List<uint> animaWeaponJobIdList,
        Dictionary<uint, List<int>> animaWeaponProcess)
    {
        private static readonly ExcelSheet<ClassJob>
            ClassJobSheet = PluginService.DataManager.GetExcelSheet<ClassJob>();

        public void Draw()
        {
            ImGui.BeginTable("AnimaWeaponChart", animaWeaponId.Count + 1,
                             ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("职业");
            ImGui.TableSetupColumn("元灵武器·元灵");
            ImGui.TableSetupColumn("元灵武器·觉醒");
            ImGui.TableSetupColumn("新元灵武器");
            ImGui.TableSetupColumn("元灵武器·超导");
            ImGui.TableSetupColumn("百炼成钢的元灵武器");
            ImGui.TableSetupColumn("元灵武器·灵慧");
            ImGui.TableSetupColumn("真元灵武器");
            ImGui.TableSetupColumn("真元灵武器·灵光");
            ImGui.TableHeadersRow();

            foreach (var jobId in animaWeaponJobIdList)
            {
                var line = animaWeaponProcess[jobId];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(ClassJobSheet.GetRow(jobId).Name.ExtractText());

                for (var j = 0; j < line.Count; j++)
                {
                    ImGui.TableNextColumn();
                    DrawMethod.DrawWeaponCell(line[j], animaWeaponId[j][DataBase.JobIndex[jobId]]);
                }
            }

            ImGui.EndTable();
        }
    }
}
