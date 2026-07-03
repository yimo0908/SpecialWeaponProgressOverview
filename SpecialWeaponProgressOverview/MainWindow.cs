using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Drawer;
using SpecialWeaponProgressOverview.Models;
using SpecialWeaponProgressOverview.Shop;

namespace SpecialWeaponProgressOverview;

public class MainWindow : Window, IDisposable
{
    private Process? _process;

    private readonly Dictionary<WeaponSeries, Dictionary<uint, List<int>>> _weaponProcess = new();
    private readonly Dictionary<WeaponSeries, WeaponSeriesDrawer>         _drawers      = new();
    private int  _selectedWeaponSeriesIndex;
    private int  _pendingTabSwitch = -1;
    private bool _needsRefresh = true;
    private bool _needsNewComment = true;
    private string _currentComment = "";

    /// <summary>总览页环形进度缓冲区，避免每帧分配。</summary>
    private (string name, float progress, string countText)[] _phaseProgresses = [];

    /// <summary>商店界面优化开关引用，由 Plugin 注入。</summary>
    public ShopExchangeModifier? ShopModifier { get; set; }

    private const int PushedColorCount = 18;
    private const int PushedVarCount = 8;

    public MainWindow()
        : base("SpecialWeaponProgressOverview", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public override void OnOpen()
    {
        _needsNewComment = true;
    }

    private void OnCacheAutoRefreshed()
    {
        _needsRefresh = true;
    }

    public void Dispose()
    {
        Inventory.CacheAutoRefreshed -= OnCacheAutoRefreshed;
        Inventory.Dispose();
        GC.SuppressFinalize(this);
    }

    public void InitChart()
    {
        Inventory.Init();
        Inventory.CacheAutoRefreshed += OnCacheAutoRefreshed;
        _process = new Process(Inventory.GetItemCountTotal);

        foreach (var kvp in WeaponSeriesInfo.All)
        {
            var series = kvp.Key;
            var info   = kvp.Value;

            _weaponProcess[series] = Process.InitChart(info);

            var getItemCount = info.HasMaterialComputation
                ? new Func<uint, int>(Inventory.GetItemCountTotal)
                : null;

            _drawers[series] = new WeaponSeriesDrawer(info, _weaponProcess[series], getItemCount);
        }
    }

    // ---- PreDraw / PostDraw：全局像素风格样式推送 ----

    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg,        PixelStyle.WindowBg);
        ImGui.PushStyleColor(ImGuiCol.ChildBg,         PixelStyle.ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Border,          PixelStyle.Border);
        ImGui.PushStyleColor(ImGuiCol.Separator,       PixelStyle.Dim);
        ImGui.PushStyleColor(ImGuiCol.FrameBg,         PixelStyle.ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Button,          PixelStyle.ButtonBg);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,   PixelStyle.TabHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,    PixelStyle.TabActive);
        ImGui.PushStyleColor(ImGuiCol.Text,            PixelStyle.Default);
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram,   PixelStyle.Accent);
        ImGui.PushStyleColor(ImGuiCol.TableHeaderBg,   PixelStyle.TabActive);
        ImGui.PushStyleColor(ImGuiCol.TableBorderStrong, PixelStyle.Border);
        ImGui.PushStyleColor(ImGuiCol.TableBorderLight,  PixelStyle.Dim);
        ImGui.PushStyleColor(ImGuiCol.TableRowBg,      PixelStyle.ChildBg);
        ImGui.PushStyleColor(ImGuiCol.TableRowBgAlt,   PixelStyle.WindowBg);
        ImGui.PushStyleColor(ImGuiCol.Header,          PixelStyle.TabActive);
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered,   PixelStyle.TabHover);
        ImGui.PushStyleColor(ImGuiCol.HeaderActive,    PixelStyle.TabActive);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,    0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,     0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,     0f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding,      0f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding,       0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,   1f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,  1f);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar(PushedVarCount);
        ImGui.PopStyleColor(PushedColorCount);
    }

    // ---- 主绘制 ----

    public override void Draw()
    {
        using var fontScope = PluginService.PluginInterface.UiBuilder.MonoFontHandle.Push();

        var localPlayer = PluginService.ObjectTable.LocalPlayer;
        if (localPlayer is null)
        {
            ImGui.TextColored(PixelStyle.Gray, "未获取到角色信息");
            return;
        }

        DrawStatusBar();
        ImGuiHelpers.ScaledDummy(2);

        DrawPixelTabs();
        ImGuiHelpers.ScaledDummy(2);

        // 仅在需要时（首次加载/点击刷新后）一次性处理所有系列的数据
        if (_needsRefresh)
        {
            foreach (var kvp in WeaponSeriesInfo.All)
            {
                var s   = kvp.Key;
                var inf = kvp.Value;
                var currentProcess  = _weaponProcess[s];
                _process!.GetProcessData(inf, currentProcess);
                _weaponProcess[s] = currentProcess;
            }

            _needsRefresh = false;
        }

        var series = WeaponSeriesInfo.FromComboIndex(_selectedWeaponSeriesIndex);
        if (series == WeaponSeries.None)
        {
            DrawOverview();
            return;
        }

        // 武器系列详情页：可滚动子窗口
        using var child = ImRaii.Child("##weaponSeriesContent", Vector2.Zero, true);
        if (!child.Success) return;

        if (_drawers.TryGetValue(series, out var drawer))
            drawer.Draw();
    }

    // ---- 状态栏 ----

    private void DrawStatusBar()
    {
        // 提示文字在前
        ImGui.Text("登录后首次查询前请先交互传唤铃，以获取雇员背包数据");

        // Allagan Tools 状态 + 缓存状态 紧跟提示文字
        ImGui.SameLine();
        ImGui.Text("AT IPC:");
        ImGui.SameLine();
        ImGui.TextColored(Inventory.ATools ? PixelStyle.Green : PixelStyle.Red,
                          Inventory.ATools ? "就绪" : "未就绪");

        ImGui.SameLine();
        ImGui.Text( "缓存:");
        ImGui.SameLine();
        if (Inventory.DataCached)
            ImGui.TextColored(PixelStyle.Green, "已缓存");
        else
            ImGui.TextColored(PixelStyle.Gray, "未缓存");

        // 刷新按钮紧跟缓存状态后面，稍微突出
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Button,        PixelStyle.TabActive);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, PixelStyle.Accent with { W = 0.3f });
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,  PixelStyle.Accent with { W = 0.5f });
        ImGui.PushStyleColor(ImGuiCol.Text,          PixelStyle.Accent);
        if (ImGui.SmallButton("刷新"))
        {
            Inventory.RefreshCache();
            _needsRefresh = true;
        }
        ImGui.PopStyleColor(4);

        // 右对齐：商店界面优化复选框
        var shopLabel = "商店界面优化";
        var checkboxWidth = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.X
                          + ImGui.CalcTextSize(shopLabel).X;
        ImGui.SameLine(ImGui.GetContentRegionMax().X - checkboxWidth);
        var shopEnabled = ShopModifier?.Enabled ?? false;
        if (ImGui.Checkbox(shopLabel, ref shopEnabled) && ShopModifier != null)
            ShopModifier.Enabled = shopEnabled;

        // 第二行提示
        ImGui.Text("点一下数字能获取对应武器名字（然后打开item search可以查预览）");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("item search没开ipc也没开指令交互");
    }

    // ---- 像素风格 Tab ----

    private void DrawPixelTabs()
    {
        // 处理来自总览页点击的 Tab 跳转
        if (_pendingTabSwitch >= 0)
        {
            _selectedWeaponSeriesIndex = _pendingTabSwitch;
            _pendingTabSwitch = -1;
        }

        for (int i = 0; i < WeaponSeriesInfo.TabNames.Length; i++)
        {
            if (i > 0)
            {
                // 自动换行：如果下一个按钮放不下就换行
                var nextWidth = ImGui.CalcTextSize($" {WeaponSeriesInfo.TabNames[i]} ").X
                              + ImGui.GetStyle().FramePadding.X * 2
                              + ImGui.GetStyle().ItemSpacing.X;
                if (ImGui.GetCursorPosX() + nextWidth <= ImGui.GetContentRegionMax().X)
                    ImGui.SameLine();
            }

            var active = i == _selectedWeaponSeriesIndex;

            ImGui.PushStyleColor(ImGuiCol.Button,        active ? PixelStyle.TabActive : new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, active ? PixelStyle.TabActive : PixelStyle.TabHover);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive,  active ? PixelStyle.TabActive : PixelStyle.TabHover);
            ImGui.PushStyleColor(ImGuiCol.Text,          active ? PixelStyle.Accent    : PixelStyle.Dim);

            if (ImGui.SmallButton($" {WeaponSeriesInfo.TabNames[i]} ##pixelTab{i}"))
                _selectedWeaponSeriesIndex = i;

            ImGui.PopStyleColor(4);
        }
    }

    // ---- 总览页 ----

    /// <summary>绘制总览页面：各武器系列进度统计 + 总进度评价。</summary>
    private void DrawOverview()
    {
        using var child = ImRaii.Child("##overviewScroll", Vector2.Zero, true);
        if (!child.Success) return;

        var totalPossible = 0;
        var totalOwned    = 0;

        foreach (var kvp in WeaponSeriesInfo.All)
        {
            var series  = kvp.Key;
            var info    = kvp.Value;

            var currentProcess = _weaponProcess[series];

            // 只统计最终阶段（最后一级武器）的拥有情况
            // 绝境战例外：各副本之间相互独立，统计所有副本的总和
            var seriesOwned = 0;
            var seriesTotal = 0;

            if (series == WeaponSeries.Ultimate)
            {
                // 绝境战：合计所有副本的可用武器数
                for (var j = 0; j < info.PhaseNames.Count; j++)
                {
                    foreach (var jobId in info.JobIdList)
                    {
                        var jobIndex = info.ResolveJobIndex(jobId, j);
                        if (jobIndex < 0 || jobIndex >= info.WeaponIdStages[j].Count)
                            continue;

                        seriesTotal++;

                        if (currentProcess.TryGetValue(jobId, out var jobProcess) &&
                            j < jobProcess.Count && jobProcess[j] > 0)
                            seriesOwned++;
                    }
                }
            }
            else
            {
                // 其他系列：统计最终阶段（优武特例：优雷卡和优雷卡·改均算最终阶段）
                foreach (var jobId in info.JobIdList)
                {
                    if (!currentProcess.TryGetValue(jobId, out var jobProcess))
                        continue;

                    // 只要拥有任一最终阶段武器即视为已完成
                    for (var j = 0; j < jobProcess.Count; j++)
                    {
                        if (info.IsFinalStage(j) && jobProcess[j] > 0)
                        {
                            seriesOwned++;
                            break;
                        }
                    }
                }
                seriesTotal = info.JobIdList.Count;
            }

            totalOwned    += seriesOwned;
            totalPossible += seriesTotal;

            // 系列标题（可点击跳转至对应 Tab）
            var tabIndex = (int)series;

            // 可点击的分段标题
            var titleText = $"▌ {info.ChineseName}";
            var titlePos  = ImGui.GetCursorScreenPos();
            var titleSize = ImGui.CalcTextSize(titleText);
            ImGui.TextColored(PixelStyle.Accent, titleText);
            if (ImGui.IsMouseHoveringRect(titlePos, titlePos + titleSize) &&
                ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                _pendingTabSwitch = tabIndex;

            ImGuiHelpers.ScaledDummy(1);

            // 基础直线进度条（带进度颜色，可点击跳转）
            var barProgress = seriesTotal > 0 ? (float)seriesOwned / seriesTotal : 0f;
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, PixelStyle.GetProgressColor(barProgress));
            ImGui.ProgressBar(barProgress, new Vector2(-1, 22f),
                              $"{seriesOwned} / {seriesTotal}");
            ImGui.PopStyleColor();
            if (ImGui.IsItemClicked())
                _pendingTabSwitch = tabIndex;

            // 绝境战：分副本统计进度，用环形进度框展示
            if (series == WeaponSeries.Ultimate)
            {
                ImGuiHelpers.ScaledDummy(2);

                // 收集各副本数据
                var phaseCount = info.PhaseNames.Count;
                if (_phaseProgresses.Length < phaseCount)
                    _phaseProgresses = new (string name, float progress, string countText)[phaseCount];

                for (var j = 0; j < phaseCount; j++)
                {
                    var jobsInPhase  = 0;
                    var ownedInPhase = 0;

                    foreach (var jobId in info.JobIdList)
                    {
                        var jobIndex = info.ResolveJobIndex(jobId, j);
                        if (jobIndex < 0 || jobIndex >= info.WeaponIdStages[j].Count)
                            continue;

                        jobsInPhase++;

                        if (currentProcess.TryGetValue(jobId, out var jobProcess) &&
                            j < jobProcess.Count && jobProcess[j] > 0)
                            ownedInPhase++;
                    }

                    var p = jobsInPhase > 0 ? (float)ownedInPhase / jobsInPhase : 0f;
                    _phaseProgresses[j] = (info.PhaseNames[j], p, $"{ownedInPhase}/{jobsInPhase}");
                }

                const float cellWidth  = 145f;
                const float cellHeight = 168f;

                for (var j = 0; j < phaseCount; j++)
                {
                    var (name, p, countText) = _phaseProgresses[j];
                    DrawCircularProgress(p, name, countText, cellWidth, cellHeight);

                    if ((j + 1) % 7 != 0 && j < phaseCount - 1)
                        ImGui.SameLine(0, 20f);
                }
            }

            ImGuiHelpers.ScaledDummy(2);
            PixelStyle.DrawSeparator();
            ImGuiHelpers.ScaledDummy(4);
        }

        // ---- 总进度评价（放大 + 渐变色） ----
        var overallProgress = totalPossible > 0 ? (double)totalOwned / totalPossible : 0d;
        var overallPct      = (int)(overallProgress * 100);

        ImGuiHelpers.ScaledDummy(2);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(2);

        // 总进度标题（无分隔线）
        ImGui.TextColored(PixelStyle.Accent, "▌ 总进度");

        ImGui.SetWindowFontScale(1.8f);

        // 从红(0%) 到 黄(50%) 到 绿(100%)
        var totalColor = overallProgress >= 1.0
            ? new Vector4(0.2f, 1f, 0.2f, 1f)
            : new Vector4((float)(1.0 - overallProgress), (float)overallProgress, 0f, 1f);
        ImGui.TextColored(totalColor, $"{totalOwned} / {totalPossible}（{overallPct}%）");

        if (_needsNewComment)
        {
            _currentComment = ProgressComments.GetRandomComment(overallPct);
            _needsNewComment = false;
        }
        ImGui.TextColored(totalColor, _currentComment);

        ImGui.SetWindowFontScale(1.0f);
    }

    /// <summary>绘制单个环形进度框（副本名 + 进度数据在环内）。</summary>
    private static void DrawCircularProgress(float progress, string name, string countText,
                                              float cellWidth, float cellHeight)
    {
        var cursor    = ImGui.GetCursorScreenPos();
        var drawList  = ImGui.GetWindowDrawList();

        const float ringDiameter  = 138f;
        const float ringThickness = 12f;
        var   ringRadius          = ringDiameter / 2f;

        // 圆环中心
        var ringCenterY = cursor.Y + 18f + ringRadius;
        var ringCenterX = cursor.X + cellWidth / 2f;

        // 背景填充圆（半透明深灰）
        var fillCol = ImGui.GetColorU32(new Vector4(0.06f, 0.06f, 0.08f, 0.8f));
        drawList.AddCircleFilled(new Vector2(ringCenterX, ringCenterY), ringRadius + 3f, fillCol, 48);

        // 背景圆环（暗灰色）
        var bgCol = ImGui.GetColorU32(new Vector4(0.22f, 0.22f, 0.28f, 0.5f));
        drawList.PathArcTo(new Vector2(ringCenterX, ringCenterY), ringRadius, 0, MathF.PI * 2, 48);
        drawList.PathStroke(bgCol, ImDrawFlags.None, ringThickness);

        // 进度弧段（从 12 点钟方向顺时针）
        if (progress > 0.001f)
        {
            var startAngle = -MathF.PI / 2f;
            var endAngle   = startAngle + MathF.PI * 2f * Math.Clamp(progress, 0f, 1f);

            var progCol = PixelStyle.GetProgressColor(progress);
            drawList.PathArcTo(new Vector2(ringCenterX, ringCenterY), ringRadius,
                               startAngle, endAngle, 48);
            drawList.PathStroke(progCol, ImDrawFlags.None, ringThickness);
        }

        // 副本名称（圆环内上方）
        var nameSize = ImGui.CalcTextSize(name);
        drawList.AddText(
            new Vector2(ringCenterX - nameSize.X / 2f, ringCenterY - 16f),
            ImGui.GetColorU32(PixelStyle.Default), name);

        // 进度数据（副本名称下方，圆环内）
        var countSize = ImGui.CalcTextSize(countText);
        drawList.AddText(
            new Vector2(ringCenterX - countSize.X / 2f, ringCenterY + 4f),
            ImGui.GetColorU32(PixelStyle.Dim), countText);

        // 占位
        ImGui.Dummy(new Vector2(cellWidth, cellHeight));
    }
}
