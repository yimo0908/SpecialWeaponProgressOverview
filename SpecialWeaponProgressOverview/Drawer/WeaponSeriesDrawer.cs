using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
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

        // 材料需求（像素风格分段标题 + 内容，材料名前带图标）
        if (info.HasMaterialComputation && _getItemCountTotal != null)
        {
            var materialResult = ComputeMaterialNeeds();
            if (materialResult != null)
            {
                ImGui.TextColored(PixelStyle.Accent, "▌ 材料需求");
                ImGuiHelpers.ScaledDummy(2);
                PixelStyle.DrawSeparator();
                ImGuiHelpers.ScaledDummy(2);

                foreach (var section in materialResult.Sections)
                {
                    ImGui.Text($"  {section.Prefix}:");
                    ImGui.SameLine();

                    for (var i = 0; i < section.Entries.Count; i++)
                    {
                        var (itemId, count) = section.Entries[i];
                        var name = DrawMethod.GetItemName(itemId);

                        // 数量 + 材料图标 + 名称
                        ImGui.TextColored(PixelStyle.Default, $"{count}个");
                        ImGui.SameLine();
                        DrawMethod.DrawItemIcon(itemId);
                        ImGui.SameLine();
                        ImGui.TextColored(PixelStyle.Default, name);

                        if (i < section.Entries.Count - 1)
                        {
                            ImGui.SameLine();
                            ImGui.TextColored(PixelStyle.Dim, ",");
                            ImGui.SameLine();
                        }
                    }
                }

                // 页脚（神典石等纯文本，无图标）
                if (!string.IsNullOrEmpty(materialResult.Footer))
                    ImGui.TextColored(PixelStyle.Default, $"  {materialResult.Footer}");

                ImGuiHelpers.ScaledDummy(4);
            }
        }

        // 进度表格标题：工具系列（天钢/莫雯/宇宙）显示"工具进度"，其余显示"武器进度"
        var isToolSeries = info.Series is WeaponSeries.Skysteel or WeaponSeries.Splendorous or WeaponSeries.Cosmic;
        ImGui.TextColored(PixelStyle.Accent, isToolSeries ? "▌ 工具进度" : "▌ 武器进度");
        ImGuiHelpers.ScaledDummy(2);
        PixelStyle.DrawSeparator();
        ImGuiHelpers.ScaledDummy(2);

        var tableLabel = $"{info.DisplayName}WeaponChart";
        var cellWidth = DrawMethod.WeaponCellWidth;
        var rowHeight = DrawMethod.WeaponCellHeight;
        var colCount = info.PhaseNames.Count + 2; // +1 职业， +1 末尾空列
        if (ImGui.BeginTable(tableLabel, colCount,
                         ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("职业", ImGuiTableColumnFlags.WidthFixed, cellWidth);
            foreach (var phaseName in info.PhaseNames)
                ImGui.TableSetupColumn(phaseName, ImGuiTableColumnFlags.WidthFixed, cellWidth);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, cellWidth);

            // 手动绘制表头行（支持自动换行+居中）
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers, rowHeight);
            ImGui.TableNextColumn();
            DrawCenteredWrappedText("职业", cellWidth, rowHeight);
            foreach (var phaseName in info.PhaseNames)
            {
                ImGui.TableNextColumn();
                DrawCenteredWrappedText(phaseName, cellWidth, rowHeight);
            }
            ImGui.TableNextColumn(); // 末尾空列表头

            foreach (var jobId in info.JobIdList)
            {
                if (!_process.TryGetValue(jobId, out var line))
                    continue;

                ImGui.TableNextRow(ImGuiTableRowFlags.None, rowHeight);

                // 职业名列：文本居中 + 自动换行（每行5字符）
                ImGui.TableNextColumn();
                DrawCenteredWrappedText(GetJobName(jobId), cellWidth, rowHeight);

                for (var j = 0; j < line.Count; j++)
                {
                    ImGui.TableNextColumn();
                    var jobIndex = info.ResolveJobIndex(jobId, j);
                    if (jobIndex < 0 || jobIndex >= info.WeaponIdStages[j].Count)
                    {
                        var placeholder = info.Series == WeaponSeries.Ultimate
                            ? "你别急，小吉还在做！"
                            : "—";
                        DrawCenteredWrappedText(placeholder, cellWidth, rowHeight);
                        continue;
                    }
                    var itemId = info.WeaponIdStages[j][jobIndex];
                    var nameColor = line[j] > 0
                                    && (info.Series == WeaponSeries.Ultimate || info.IsFinalStage(j))
                        ? PixelStyle.Green
                        : (Vector4?)null;
                    // 图标在单元格内垂直居中
                    var iconSize = DrawMethod.WeaponIconSize;
                    var vertOffset = Math.Max(0f, (rowHeight - iconSize) / 2f);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + vertOffset);
                    DrawMethod.DrawWeaponCell(line[j], itemId, nameColor);
                }

                // 末尾空列
                ImGui.TableNextColumn();
            }

            ImGui.EndTable();
        }
    }

    /// <summary>在单元格内绘制居中且自动换行的文本（每行最多5个字符）。</summary>
    private static void DrawCenteredWrappedText(string text, float cellWidth, float cellHeight)
    {
        const int maxCharsPerLine = 5;

        // 直接计算行数，无需分配 List
        var lineCount = text.Length == 0 ? 1 : (text.Length + maxCharsPerLine - 1) / maxCharsPerLine;

        var lineHeight = ImGui.GetTextLineHeight();
        var totalHeight = lineCount * lineHeight;
        var vertOffset = Math.Max(0f, (cellHeight - totalHeight) / 2f);

        // 垂直居中起点
        var startY = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(startY + vertOffset);

        // 逐段截取并绘制，避免 List<string> 分配
        for (var i = 0; i < text.Length; i += maxCharsPerLine)
        {
            var len = Math.Min(maxCharsPerLine, text.Length - i);
            var line = text.Substring(i, len);
            var textSize = ImGui.CalcTextSize(line);
            var horizOffset = Math.Max(0f, (cellWidth - textSize.X) / 2f);
            var curX = ImGui.GetCursorPosX();
            ImGui.SetCursorPosX(curX + horizOffset);
            ImGui.TextColored(PixelStyle.Default, line);
        }

        // 空字符串占位
        if (text.Length == 0)
            ImGui.TextColored(PixelStyle.Default, "");
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

    private Compute.MaterialNeedsResult? ComputeMaterialNeeds()
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
