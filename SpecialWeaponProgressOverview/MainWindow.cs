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

    internal static void Init()
    {
        Inventory.Init();
    }

    public class ItemInfo(uint itemId, uint quantity)
    {
        public uint ItemId { get; set; } = itemId;
        public uint Quantity { get; set; } = quantity;
    }

    private static int GetItemCountTotal(uint itemId) => Inventory.GetItemCountTotal(itemId);

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

        var localPlayer = PluginService.ObjectTable.LocalPlayer;
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

        ImGui.Combo("武器系列##选武器", ref selectedWeaponSeriesIndex, specialWeaponSeriesList, specialWeaponSeriesList.Length);

        if (selectedWeaponSeriesIndex > 0 && selectedWeaponSeriesIndex < specialWeaponSeriesList.Length)
        {
            var weaponType = GetWeaponType(selectedWeaponSeriesIndex);
            var currentProcess = weaponProcess[weaponType];

            // 执行数据处理
            process?.GetProcessData(selectedWeaponSeriesIndex, WeaponIds[GetWeaponDisplayName(weaponType)], WeaponJobIds[GetWeaponDisplayName(weaponType)], ref currentProcess);

            // 绘制对应界面
            DrawWeaponType(weaponType);

            weaponProcess[weaponType] = currentProcess;
        }
    }

    private static string GetWeaponType(int index)
    {
        return index switch
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
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
    }

    private static string GetWeaponDisplayName(string weaponType)
    {
        return weaponType switch
        {
            "zodiac" => "Zodiac",
            "anima" => "Anima",
            "eureka" => "Eureka",
            "bozja" => "Bozja",
            "mandervillous" => "Mandervillous",
            "phantom" => "Phantom",
            "skysteel" => "Skysteel",
            "splendorous" => "Splendorous",
            "ultimate" => "Ultimate",
            _ => throw new ArgumentOutOfRangeException(nameof(weaponType)),
        };
    }

    private void DrawWeaponType(string weaponType)
    {
        switch (weaponType)
        {
            case "zodiac":
                zodiacDrawer?.Draw();
                break;
            case "anima":
                animaDrawer?.Draw();
                break;
            case "eureka":
                eurekaDrawer?.Draw();
                break;
            case "bozja":
                bozjaDrawer?.Draw();
                break;
            case "mandervillous":
                mandervillousDrawer?.Draw();
                break;
            case "phantom":
                phantomDrawer?.Draw();
                break;
            case "skysteel":
                skysteelDrawer?.Draw();
                break;
            case "splendorous":
                splendorousDrawer?.Draw();
                break;
            case "ultimate":
                ultimateDrawer?.Draw();
                break;
        }
    }

    public void InitChart()
    {
        process = new Process(GetItemCountTotal);

        // 初始化所有武器类型的处理数据
        for (var i = 1; i <= 9; i++)
        {
            var weaponType = GetWeaponType(i);
            weaponProcess[weaponType] = Process.InitChart(i);
        }

        // 初始化所有Drawer
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
}
