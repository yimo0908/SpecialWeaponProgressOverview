using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer
{
    public class EurekaDrawer(
        List<List<uint>> eurekaWeaponId,
        List<uint> eurekaWeaponJobIdList,
        Dictionary<uint, List<int>> eurekaWeaponProcess)
    {
        private static readonly ExcelSheet<ClassJob>
            ClassJobSheet = PluginService.DataManager.GetExcelSheet<ClassJob>();

        public void Draw()
        {
            ImGui.BeginTable("EurekaWeaponChart", eurekaWeaponId.Count + 1,
                             ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("职业");
            ImGui.TableSetupColumn("禁地兵装");
            ImGui.TableSetupColumn("禁地兵装+1");
            ImGui.TableSetupColumn("禁地兵装+2");
            ImGui.TableSetupColumn("常风");
            ImGui.TableSetupColumn("恒冰");
            ImGui.TableSetupColumn("恒冰+1");
            ImGui.TableSetupColumn("元素");
            ImGui.TableSetupColumn("元素+1");
            ImGui.TableSetupColumn("元素+2");
            ImGui.TableSetupColumn("涌火");
            ImGui.TableSetupColumn("丰水");
            ImGui.TableSetupColumn("丰水+1");
            ImGui.TableSetupColumn("新兵装");
            ImGui.TableSetupColumn("优雷卡");
            ImGui.TableSetupColumn("优雷卡·改");
            ImGui.TableHeadersRow();

            foreach (var jobId in eurekaWeaponJobIdList)
            {
                var line = eurekaWeaponProcess[jobId];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(ClassJobSheet.GetRow(jobId).Name.ExtractText());

                for (var j = 0; j < line.Count; j++)
                {
                    ImGui.TableNextColumn();
                    DrawMethod.DrawWeaponCell(line[j], eurekaWeaponId[j][DataBase.JobIndex[jobId]]);
                }
            }

            ImGui.EndTable();
        }
    }
}
