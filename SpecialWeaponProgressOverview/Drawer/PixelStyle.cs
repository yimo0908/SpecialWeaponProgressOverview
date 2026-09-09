using System;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;

namespace SpecialWeaponProgressOverview.Drawer;

/// <summary>像素风格 UI 共享样式常量与辅助方法，消除多文件重复定义。</summary>
public static class PixelStyle
{
    // ---- 窗口/控件背景色 ----
    public static readonly Vector4 WindowBg  = new(0.06f, 0.06f, 0.08f, 0.96f);
    public static readonly Vector4 ChildBg   = new(0.03f, 0.03f, 0.05f, 1f);
    public static readonly Vector4 Border    = new(0.22f, 0.22f, 0.28f, 0.5f);
    public static readonly Vector4 ButtonBg  = new(0.1f, 0.1f, 0.14f, 1f);
    public static readonly Vector4 TabActive = new(0.12f, 0.12f, 0.16f, 1f);
    public static readonly Vector4 TabHover  = new(0.08f, 0.08f, 0.12f, 1f);

    // ---- 文字色 ----
    public static readonly Vector4 Dim       = new(0.35f, 0.35f, 0.4f, 1f);
    public static readonly Vector4 Accent    = new(0.4f, 0.85f, 1.0f, 1f);
    public static readonly Vector4 Default   = new(0.85f, 0.85f, 0.85f, 1f);
    public static readonly Vector4 Gray      = new(0.45f, 0.45f, 0.5f, 1f);
    public static readonly Vector4 Green     = new(0.2f, 0.9f, 0.25f, 1f);
    public static readonly Vector4 Red       = new(1.0f, 0.35f, 0.35f, 1f);

    // ---- 进度色 ----
    private static readonly Vector4 ProgressComplete = new(0.2f, 0.9f, 0.25f, 1f);
    private static readonly Vector4 ProgressHigh     = new(0.3f, 0.7f, 1f, 1f);
    private static readonly Vector4 ProgressMid      = new(1f, 0.78f, 0.25f, 1f);
    private static readonly Vector4 ProgressLow      = new(1f, 0.35f, 0.35f, 1f);

    // ---- 图标 ----
    public static readonly Vector4 IconBorder = new(0.22f, 0.22f, 0.28f, 0.8f);

    /// <summary>ImGui 单线程 UI，可安全复用 StringBuilder 避免每帧分配。</summary>
    private static readonly StringBuilder _sb = new();

    /// <summary>像素风格分隔线：用 ─ 字符填满可用宽度。</summary>
    public static void DrawSeparator()
    {
        var avail = ImGui.GetContentRegionAvail().X;
        var charWidth = ImGui.CalcTextSize("─").X;
        if (charWidth <= 0) return;
        var count = Math.Max(1, (int)(avail / charWidth));

        _sb.Clear();
        _sb.Append('─', count);
        ImGui.TextColored(Dim, _sb.ToString());
    }

    /// <summary>根据进度返回对应颜色 (uint)：红(&lt;25%)→黄(&lt;50%)→蓝(&lt;100%)→绿(=100%)。</summary>
    public static uint GetProgressColor(float progress) => progress switch
    {
        >= 1f    => ImGui.GetColorU32(ProgressComplete),
        >= 0.5f  => ImGui.GetColorU32(ProgressHigh),
        >= 0.25f => ImGui.GetColorU32(ProgressMid),
        _        => ImGui.GetColorU32(ProgressLow),
    };
}
