using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Drawer;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview;

public class MainWindow : Window, IDisposable
{
    private Process? _process;

    private readonly Dictionary<WeaponSeries, Dictionary<uint, List<int>>> _weaponProcess = new();
    private readonly Dictionary<WeaponSeries, WeaponSeriesDrawer>         _drawers      = new();
    private int  _selectedWeaponSeriesIndex;
    private bool _needsRefresh = true;
    private bool _needsNewComment = true;
    private string _currentComment = "";

    /// <summary>总览页环形进度缓冲区，避免每帧分配。</summary>
    private (string name, float progress, string countText)[] _phaseProgresses = [];

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

    private static int GetItemCountTotal(uint itemId) => Inventory.GetItemCountTotal(itemId);

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
        _process = new Process(GetItemCountTotal);

        foreach (var kvp in WeaponSeriesInfo.All)
        {
            var series = kvp.Key;
            var info   = kvp.Value;

            _weaponProcess[series] = Process.InitChart(info);

            var getItemCount = info.HasMaterialComputation
                ? new Func<uint, int>(GetItemCountTotal)
                : null;

            _drawers[series] = new WeaponSeriesDrawer(info, _weaponProcess[series], getItemCount);
        }
    }

    public override void Draw()
    {
        var localPlayer = PluginService.ObjectTable.LocalPlayer;
        if (localPlayer is null)
        {
            ImGui.Text("未获取到角色信息");
            return;
        }

        ImGui.Text($"Is Allagan Tools available: {Inventory.ATools}");
        ImGui.Text("登录后首次查询前请先交互传唤铃，以获取雇员背包数据");
        ImGui.SameLine();
        if (ImGui.Button("刷新"))
        {
            Inventory.RefreshCache();
            _needsRefresh = true;
        }
        ImGui.SameLine();
        if (Inventory.DataCached)
            ImGui.TextColored(new Vector4(0.2f, 1f, 0.2f, 1f), "（数据已缓存）");
        else
            ImGui.TextDisabled("（数据未缓存）");
        ImGui.Text("点一下数字能获取对应武器名字（然后打开item search可以查预览）");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("item search没开ipc也没开指令交互");

        // 横向 Tab 导航
        if (ImGui.BeginTabBar("WeaponTabs", ImGuiTabBarFlags.FittingPolicyScroll))
        {
            for (var i = 0; i < WeaponSeriesInfo.TabNames.Length; i++)
            {
                if (ImGui.BeginTabItem(WeaponSeriesInfo.TabNames[i]))
                {
                    _selectedWeaponSeriesIndex = i;
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }

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

        var info = WeaponSeriesInfo.All[series];

        // 绘制对应界面
        if (_drawers.TryGetValue(series, out var drawer))
            drawer.Draw();
    }

    /// <summary>绘制总览页面：各武器系列进度统计 + 总进度评价。</summary>
    private void DrawOverview()
    {
        // 使用可滚动子窗口容纳所有内容（父窗口无滚动条）
        ImGui.BeginChild("OverviewScroll", new Vector2(-1, -1), true);

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

            // 系列标题
            ImGui.Text($"{info.ChineseName}（{info.DisplayName}）");

            // 基础直线进度条（带进度颜色）
            var barProgress = seriesTotal > 0 ? (float)seriesOwned / seriesTotal : 0f;
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, GetProgressColor(barProgress));
            ImGui.ProgressBar(barProgress, new Vector2(-1, 22f),
                              $"{seriesOwned} / {seriesTotal}");
            ImGui.PopStyleColor();

            // 绝境战：分副本统计进度，用环形进度框展示
            if (series == WeaponSeries.Ultimate)
            {
                ImGui.Indent();
                ImGui.Spacing();

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

                ImGui.Unindent();
            }

            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 6)); // 系列之间的间隔
        }

        // ---- 总进度评价（放大 + 渐变色） ----
        var overallProgress = totalPossible > 0 ? (double)totalOwned / totalPossible : 0d;
        var overallPct      = (int)(overallProgress * 100);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.SetWindowFontScale(1.8f);

        // 从红(0%) 到 黄(50%) 到 绿(100%)
        var totalColor = overallProgress >= 1.0
            ? new Vector4(0.2f, 1f, 0.2f, 1f)
            : new Vector4((float)(1.0 - overallProgress), (float)overallProgress, 0f, 1f);
        ImGui.TextColored(totalColor, $"总进度: {totalOwned} / {totalPossible}（{overallPct}%）");

        if (_needsNewComment)
        {
            _currentComment = ProgressComments.GetRandomComment(overallPct);
            _needsNewComment = false;
        }
        ImGui.TextColored(totalColor, _currentComment);

        ImGui.SetWindowFontScale(1.0f);

        ImGui.EndChild();
    }

    /// <summary>绘制单个环形进度框（3× 放大，副本名 + 进度数据在环内）。</summary>
    private static void DrawCircularProgress(float progress, string name, string countText,
                                              float cellWidth, float cellHeight)
    {
        var cursor    = ImGui.GetCursorScreenPos();
        var drawList  = ImGui.GetWindowDrawList();

        const float ringDiameter  = 138f;   // 3× 原尺寸
        const float ringThickness = 12f;
        var   ringRadius          = ringDiameter / 2f;

        // 圆环中心
        var ringCenterY = cursor.Y + 18f + ringRadius;
        var ringCenterX = cursor.X + cellWidth / 2f;

        // 背景填充圆（半透明深灰）
        var fillCol = ImGui.GetColorU32(new Vector4(0.15f, 0.15f, 0.15f, 0.5f));
        drawList.AddCircleFilled(new Vector2(ringCenterX, ringCenterY), ringRadius + 3f, fillCol, 48);

        // 背景圆环（暗灰色）
        var bgCol = ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.5f, 0.35f));
        drawList.PathArcTo(new Vector2(ringCenterX, ringCenterY), ringRadius, 0, MathF.PI * 2, 48);
        drawList.PathStroke(bgCol, ImDrawFlags.None, ringThickness);

        // 进度弧段（从 12 点钟方向顺时针）
        if (progress > 0.001f)
        {
            var startAngle = -MathF.PI / 2f;
            var endAngle   = startAngle + MathF.PI * 2f * Math.Clamp(progress, 0f, 1f);

            var progCol = GetProgressColor(progress);
            drawList.PathArcTo(new Vector2(ringCenterX, ringCenterY), ringRadius,
                               startAngle, endAngle, 48);
            drawList.PathStroke(progCol, ImDrawFlags.None, ringThickness);
        }

        // 副本名称（圆环内上方）
        var nameSize = ImGui.CalcTextSize(name);
        drawList.AddText(
            new Vector2(ringCenterX - nameSize.X / 2f, ringCenterY - 16f),
            ImGui.GetColorU32(new Vector4(0.95f, 0.95f, 0.95f, 1f)), name);

        // 进度数据（副本名称下方，圆环内）
        var countSize = ImGui.CalcTextSize(countText);
        drawList.AddText(
            new Vector2(ringCenterX - countSize.X / 2f, ringCenterY + 4f),
            ImGui.GetColorU32(new Vector4(0.75f, 0.75f, 0.75f, 1f)), countText);

        // 占位
        ImGui.Dummy(new Vector2(cellWidth, cellHeight));
    }

    /// <summary>根据进度返回对应颜色：红(&lt;25%)→黄(&lt;50%)→蓝(&lt;100%)→绿(=100%)。</summary>
    private static uint GetProgressColor(float progress) => progress switch
    {
        >= 1f    => ImGui.GetColorU32(new Vector4(0.2f,  1f,   0.2f,  1f)),
        >= 0.5f  => ImGui.GetColorU32(new Vector4(0.3f,  0.7f,  1f,   1f)),
        >= 0.25f => ImGui.GetColorU32(new Vector4(1f,   0.85f, 0.2f,  1f)),
        _        => ImGui.GetColorU32(new Vector4(1f,   0.3f,  0.3f,  1f)),
    };
}
