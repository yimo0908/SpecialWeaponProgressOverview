using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Drawer
{
    public class UltimateDrawer(
        List<List<uint>> ultimateWeaponId,
        List<uint> ultimateWeaponJobIdList,
        Dictionary<uint, List<int>> ultimateWeaponProcess)
    {
        private static readonly ExcelSheet<ClassJob>
            ClassJobSheet = PluginService.DataManager.GetExcelSheet<ClassJob>();

        public void Draw()
        {
            ImGui.Text("");
            ImGui.BeginTable("UltimateWeaponChart", ultimateWeaponId.Count + 1,
                             ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);
            ImGui.TableSetupColumn("职业");
            ImGui.TableSetupColumn("绝巴哈");
            ImGui.TableSetupColumn("绝神兵");
            ImGui.TableSetupColumn("绝亚");
            ImGui.TableSetupColumn("绝龙诗");
            ImGui.TableSetupColumn("绝欧米茄");
            ImGui.TableSetupColumn("绝伊甸");
            ImGui.TableHeadersRow();

            foreach (var jobId in ultimateWeaponJobIdList)
            {
                var line = ultimateWeaponProcess[jobId];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(ClassJobSheet.GetRow(jobId).Name.ExtractText());

                for (var j = 0; j < line.Count; j++)
                {
                    // 使用与GetProcessData相同的索引逻辑确定jobIndex
                    int jobIndex = j < 5 ? DataBase.JobIndex[jobId] : DataBase.NewJobIndex[jobId];

                    // 检查索引是否在有效范围内
                    if (jobIndex >= ultimateWeaponId[j].Count)
                    {
                        // 超出数组范围的格子显示为空格
                        ImGui.TableNextColumn();
                        ImGui.Text("你别急，小吉还在做！");
                        continue;
                    }

                    ImGui.TableNextColumn();
                    DrawMethod.DrawWeaponCell(line[j], ultimateWeaponId[j][jobIndex]);
                }
            }

            ImGui.EndTable();
        }
    }
}
