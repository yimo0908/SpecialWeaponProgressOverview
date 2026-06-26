using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Drawer;

/// <summary>统一武器进度表格绘制基类，替代 9 个重复的 Drawer。</summary>
public class WeaponSeriesDrawer
{
    private static readonly ExcelSheet<ClassJob> ClassJobSheet =
        PluginService.DataManager.GetExcelSheet<ClassJob>();

    /// <summary>职业名称缓存，避免每帧重复查表。</summary>
    private static readonly Dictionary<uint, string> _jobNameCache = new();

    private readonly WeaponSeriesInfo                     _seriesInfo;
    private readonly Dictionary<uint, List<int>>          _process;
    private readonly Func<uint, int>?                     _getItemCountTotal;

    public WeaponSeriesDrawer(
        WeaponSeriesInfo                seriesInfo,
        Dictionary<uint, List<int>>     process,
        Func<uint, int>?                getItemCountTotal = null)
    {
        _seriesInfo         = seriesInfo;
        _process            = process;
        _getItemCountTotal  = getItemCountTotal;
    }

    public void Draw()
    {
        var info = _seriesInfo;

        // 材料需求
        if (info.HasMaterialComputation && _getItemCountTotal != null)
        {
            var materialText = ComputeMaterialNeeds();
            if (!string.IsNullOrEmpty(materialText))
                ImGui.Text(materialText);
        }

        var tableLabel = $"{info.DisplayName}WeaponChart";
        ImGui.BeginTable(tableLabel, info.PhaseNames.Count + 1,
                         ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg);

        ImGui.TableSetupColumn("职业");
        foreach (var phaseName in info.PhaseNames)
            ImGui.TableSetupColumn(phaseName);
        ImGui.TableHeadersRow();

        foreach (var jobId in info.JobIdList)
        {
            if (!_process.TryGetValue(jobId, out var line))
                continue;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(GetJobName(jobId));

            for (var j = 0; j < line.Count; j++)
            {
                ImGui.TableNextColumn();
                var jobIndex = info.ResolveJobIndex(jobId, j);
                if (jobIndex < 0 || jobIndex >= info.WeaponIdStages[j].Count)
                {
                    ImGui.Text(info.Series == WeaponSeries.Ultimate
                        ? "你别急，小吉还在做！"
                        : "—");
                    continue;
                }
                var itemId = info.WeaponIdStages[j][jobIndex];
                var nameColor = info.Series != WeaponSeries.Ultimate
                                && j == line.Count - 1
                                && line[j] > 0
                    ? new Vector4(0.2f, 1f, 0.2f, 1f)
                    : (Vector4?)null;
                DrawMethod.DrawWeaponCell(line[j], itemId, nameColor);
            }
        }

        ImGui.EndTable();
    }

    /// <summary>获取职业名称（带缓存）。</summary>
    private static string GetJobName(uint jobId)
    {
        if (_jobNameCache.TryGetValue(jobId, out var name))
            return name;

        name = ClassJobSheet.GetRow(jobId).Name.ExtractText();
        _jobNameCache[jobId] = name;
        return name;
    }

    private string? ComputeMaterialNeeds()
    {
        var info = _seriesInfo;
        if (_getItemCountTotal == null) return null;

        return info.Series switch
        {
            WeaponSeries.Bozja         => Compute.ComputeNeedsBozja(info, _getItemCountTotal),
            WeaponSeries.Mandervillous => Compute.ComputeNeedsMandervillous(info, _getItemCountTotal),
            WeaponSeries.Phantom       => Compute.ComputeNeedsPhantom(info, _getItemCountTotal),
            WeaponSeries.Eureka        => Compute.ComputeNeedsEureka(info, _getItemCountTotal),
            _                          => null,
        };
    }
}
