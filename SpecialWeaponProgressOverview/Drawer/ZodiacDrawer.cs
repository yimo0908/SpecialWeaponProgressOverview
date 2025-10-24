using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer
{
    public class ZodiacDrawer(List<List<uint>> zodiacWeaponId, List<uint> zodiacWeaponJobIdList, 
                                                         Dictionary<uint, List<int>> zodiacWeaponProcess)
    {
        private static readonly ExcelSheet<ClassJob> ClassJobSheet = PluginService.DataManager.GetExcelSheet<ClassJob>();

        public void Draw()
        {
            ImGui.BeginTable("ZodiacWeaponChart", zodiacWeaponId.Count + 1,
                             ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("职业");
            ImGui.TableSetupColumn("半成品");
            ImGui.TableSetupColumn("本体");
            ImGui.TableSetupColumn("天极");
            ImGui.TableSetupColumn("魂晶");
            ImGui.TableSetupColumn("魂灵");
            ImGui.TableSetupColumn("新星");
            ImGui.TableSetupColumn("镇魂");
            ImGui.TableSetupColumn("黄道武器");
            ImGui.TableSetupColumn("黄道武器·本我");
            ImGui.TableHeadersRow();
            
            foreach (var jobId in zodiacWeaponJobIdList)
            {
                var line = zodiacWeaponProcess[jobId];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(ClassJobSheet.GetRow(jobId).Name.ExtractText());

                for (var j = 0; j < line.Count; j++)
                {
                    ImGui.TableNextColumn();
                    DrawMethod.DrawWeaponCell(line[j], zodiacWeaponId[j][DataBase.NewJobIndex[jobId]]);
                }
            }

            ImGui.EndTable();
        }
    }
}
