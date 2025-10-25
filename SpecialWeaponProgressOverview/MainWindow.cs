using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Drawer;

namespace SpecialWeaponProgressOverview;

public class MainWindow : Window, IDisposable
{
    private Process? process;
    private ZodiacDrawer? zodiacDrawer;
    private AnimaDrawer? animaDrawer;
    private EurekaDrawer? eurekaDrawer;
    private BozjaDrawer? bozjaDrawer;
    private MandervillousDrawer? mandervillousDrawer;
    private PhantomDrawer? phantomDrawer;
    private SkysteelDrawer? skysteelDrawer;
    private SplendorousDrawer? splendorousDrawer;
    private UltimateDrawer? ultimateDrawer;

    public delegate int ItemCountDelegate(uint itemId);

    internal static void Init()
    {
        Inventory.Init();
    }

    public class ItemInfo(uint itemId, uint quantity)
    {
        public uint ItemId { get; set; } = itemId;
        public uint Quantity { get; set; } = quantity;
    }

    private int GetItemCountTotal(uint itemId) => Inventory.GetItemCountTotal(itemId);

    public MainWindow()
        : base("SpecialWeaponProgressOverview", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public void Dispose()
    {
        Inventory.Dispose();
        GC.SuppressFinalize(this);
    }

    private static readonly Dictionary<string, List<List<uint>>> WeaponIds = new()
    {
        { "Zodiac", DataBase.ZodiacWeaponId },
        { "Anima", DataBase.AnimaWeaponId },
        { "Eureka", DataBase.EurekaWeaponId },
        { "Bozja", DataBase.BozjaWeaponId },
        { "Mandervillous", DataBase.MandervillousWeaponId },
        { "Phantom", DataBase.PhantomWeaponId },
        { "Skysteel", DataBase.SkysteelWeaponId },
        { "Splendorous", DataBase.SplendorousWeaponId },
        { "Ultimate", DataBase.UltimateWeaponId }
    };

    private static readonly Dictionary<string, List<uint>> WeaponJobIds = new()
    {
        { "Anima", DataBase.AnimaWeaponJobIdList },
        { "Zodiac", DataBase.ZodiacWeaponJobIdList },
        { "Eureka", DataBase.EurekaWeaponJobIdList },
        { "Bozja", DataBase.BozjaWeaponJobIdList },
        { "Mandervillous", DataBase.MandervillousWeaponJobIdList },
        { "Phantom", DataBase.PhantomWeaponJobIdList },
        { "Skysteel", DataBase.SkysteelWeaponJobIdList },
        { "Splendorous", DataBase.SplendorousWeaponJobIdList },
        { "Ultimate", DataBase.UltimateWeaponJobIdList }
    };
    
    private readonly Dictionary<string, Dictionary<uint, List<int>>> weaponProcess = new()
    {
        { "zodiac", new Dictionary<uint, List<int>>() },
        { "anima", new Dictionary<uint, List<int>>() },
        { "eureka", new Dictionary<uint, List<int>>() },
        { "bozja", new Dictionary<uint, List<int>>() },
        { "mandervillous", new Dictionary<uint, List<int>>() },
        { "phantom", new Dictionary<uint, List<int>>() },
        { "skysteel", new Dictionary<uint, List<int>>() },
        { "splendorous", new Dictionary<uint, List<int>>() },
        { "ultimate", new Dictionary<uint, List<int>>() }
    };

    private readonly string[] specialWeaponSeriesList =
    [
        "未选中", "古武", "魂武", "优武", "义武", "曼武", "幻武", "天钢", "莫雯", "绝境战",
    ];

    private int selectedWeaponSeriesIndex;

    public override void Draw()
    {
        if (zodiacDrawer == null)
        {
            InitChart();
        }

        var localPlayer = PluginService.ClientState.LocalPlayer;
        if (localPlayer is null)
        {
            ImGui.Text("未获取到角色信息");
            return;
        }

        ImGui.Text($"Is Allagan Tools available: {Inventory.ATools}");
        ImGui.Text($"登录后首次查询前请先交互传唤铃，以获取雇员背包数据");
        ImGui.Text($"点一下数字能获取对应武器名字（然后打开item search可以查预览）");
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("item search没开ipc也没开指令交互");
        }

        ImGui.Combo("武器系列##选武器", ref selectedWeaponSeriesIndex, specialWeaponSeriesList, 6);
        if (selectedWeaponSeriesIndex != 0)
        {
            var weaponType = selectedWeaponSeriesIndex switch
            {
                1 => "zodiac",
                2 => "anima",
                3 => "eureka",
                4 => "bozja",
                5 => "mandervillous",
                6 => "phantom",
                7 => "skysteel",
                8 => "splendorous",
                9 => "ultimate",
                _ => throw new ArgumentOutOfRangeException(),
            };

            var currentProcess = weaponProcess[weaponType];
            _ = selectedWeaponSeriesIndex switch
            {
                1 => ExecuteAndDraw(() => process?.GetProcessData(1, WeaponIds["Zodiac"], WeaponJobIds["Zodiac"], ref currentProcess), DrawZodiac),
                2 => ExecuteAndDraw(() => process?.GetProcessData(2, WeaponIds["Anima"], WeaponJobIds["Anima"], ref currentProcess), DrawAnima),
                3 => ExecuteAndDraw(() => process?.GetProcessData(3, WeaponIds["Eureka"], WeaponJobIds["Eureka"], ref currentProcess), DrawEureka),
                4 => ExecuteAndDraw(() => process?.GetProcessData(4, WeaponIds["Bozja"], WeaponJobIds["Bozja"], ref currentProcess), DrawBozja),
                5 => ExecuteAndDraw(() => process?.GetProcessData(5, WeaponIds["Mandervillous"], WeaponJobIds["Mandervillous"], ref currentProcess), DrawMandervillous),
                6 => ExecuteAndDraw(() => process?.GetProcessData(6, WeaponIds["Phantom"], WeaponJobIds["Phantom"], ref currentProcess), DrawPhantom),
                7 => ExecuteAndDraw(() => process?.GetProcessData(7, WeaponIds["Skysteel"], WeaponJobIds["Skysteel"], ref currentProcess), DrawSkysteel),
                8 => ExecuteAndDraw(() => process?.GetProcessData(8, WeaponIds["Splendorous"], WeaponJobIds["Splendorous"], ref currentProcess), DrawSplendorous),
                9 => ExecuteAndDraw(() => process?.GetProcessData(9, WeaponIds["Ultimate"], WeaponJobIds["Ultimate"], ref currentProcess), DrawUltimate),
                _ => throw new ArgumentOutOfRangeException(),
            };

            weaponProcess[weaponType] = currentProcess;
        }

        object? ExecuteAndDraw(Action processData, Action draw)
        {
            processData();
            draw();
            return null;
        }
    }

    public void InitChart()
    {
        process = new Process(GetItemCountTotal);
        for (var i = 1; i <= 9; i++)
        {
            var weaponType = i switch
            {
                1 => "zodiac",
                2 => "anima",
                3 => "eureka",
                4 => "bozja",
                5 => "mandervillous",
                6 => "phantom",
                7 => "skysteel",
                8 => "splendorous",
                9 => "ultimate",
                _ => throw new ArgumentOutOfRangeException(),
            };

            weaponProcess[weaponType] = process.InitChart(i);
        }

        zodiacDrawer = new ZodiacDrawer(WeaponIds["Zodiac"], WeaponJobIds["Zodiac"], weaponProcess["zodiac"]);
        animaDrawer = new AnimaDrawer(WeaponIds["Anima"], WeaponJobIds["Anima"], weaponProcess["anima"]);
        eurekaDrawer = new EurekaDrawer(WeaponIds["Eureka"], WeaponJobIds["Eureka"], weaponProcess["eureka"]);
        bozjaDrawer = new BozjaDrawer(WeaponIds["Bozja"], WeaponJobIds["Bozja"], weaponProcess["bozja"], GetItemCountTotal);
        mandervillousDrawer = new MandervillousDrawer(WeaponIds["Mandervillous"], WeaponJobIds["Mandervillous"], weaponProcess["mandervillous"], GetItemCountTotal);
        phantomDrawer = new PhantomDrawer(WeaponIds["Phantom"], WeaponJobIds["Phantom"], weaponProcess["phantom"], GetItemCountTotal);
        skysteelDrawer = new SkysteelDrawer(WeaponIds["Skysteel"], WeaponJobIds["Skysteel"], weaponProcess["skysteel"]);
        splendorousDrawer = new SplendorousDrawer(WeaponIds["Splendorous"], WeaponJobIds["Splendorous"], weaponProcess["splendorous"]);
        ultimateDrawer = new UltimateDrawer(WeaponIds["Ultimate"], WeaponJobIds["Ultimate"], weaponProcess["ultimate"]);
    }

    private void DrawZodiac() => zodiacDrawer?.Draw();

    private void DrawAnima() => animaDrawer?.Draw();

    private void DrawEureka() => eurekaDrawer?.Draw();

    private void DrawBozja() => bozjaDrawer?.Draw();

    private void DrawMandervillous() => mandervillousDrawer?.Draw();

    private void DrawPhantom() => phantomDrawer?.Draw();

    private void DrawSkysteel() => skysteelDrawer?.Draw();

    private void DrawSplendorous() => splendorousDrawer?.Draw();

    private void DrawUltimate() => ultimateDrawer?.Draw();
}
