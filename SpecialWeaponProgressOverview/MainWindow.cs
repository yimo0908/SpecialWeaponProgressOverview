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
    // 向后兼容的委托类型别名
    public delegate int ItemCountDelegate(uint itemId);

    private Process? _process;

    private readonly Dictionary<WeaponSeries, Dictionary<uint, List<int>>> _weaponProcess = new();
    private readonly Dictionary<WeaponSeries, WeaponSeriesDrawer>         _drawers      = new();
    private int _selectedWeaponSeriesIndex;

    public MainWindow()
        : base("SpecialWeaponProgressOverview", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    internal static void Init()
    {
        Inventory.Init();
    }

    private static int GetItemCountTotal(uint itemId) => Inventory.GetItemCountTotal(itemId);

    public void Dispose()
    {
        Inventory.Dispose();
        GC.SuppressFinalize(this);
    }

    public void InitChart()
    {
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
        if (_process == null)
            InitChart();

        var localPlayer = PluginService.ObjectTable.LocalPlayer;
        if (localPlayer is null)
        {
            ImGui.Text("未获取到角色信息");
            return;
        }

        ImGui.Text($"Is Allagan Tools available: {Inventory.ATools}");
        ImGui.Text("登录后首次查询前请先交互传唤铃，以获取雇员背包数据");
        ImGui.Text("点一下数字能获取对应武器名字（然后打开item search可以查预览）");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("item search没开ipc也没开指令交互");

        ImGui.Combo("武器系列##选武器", ref _selectedWeaponSeriesIndex,
                    WeaponSeriesInfo.ComboNames, WeaponSeriesInfo.ComboNames.Length);

        var series = WeaponSeriesInfo.FromComboIndex(_selectedWeaponSeriesIndex);
        if (series == WeaponSeries.None) return;

        var info = WeaponSeriesInfo.All[series];

        // 执行数据处理
        var currentProcess = _weaponProcess[series];
        _process?.GetProcessData(info, ref currentProcess);
        _weaponProcess[series] = currentProcess;

        // 绘制对应界面
        if (_drawers.TryGetValue(series, out var drawer))
            drawer.Draw();
    }
}
