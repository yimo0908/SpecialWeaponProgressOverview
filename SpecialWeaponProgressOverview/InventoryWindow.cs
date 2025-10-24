using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using SpecialWeaponProgressOverview.Data;
using SpecialWeaponProgressOverview.Drawer;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview;

public class InventoryWindow : Window, IDisposable
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

    private int GetItemCountTotal(uint itemId) =>
        Inventory.GetItemCountTotal(itemId);

    public InventoryWindow()
        : base("InventoryWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose()
    {
        Inventory.Dispose();
        GC.SuppressFinalize(this);
    }

    private static readonly List<List<uint>> ZodiacWeaponId = DataBase.ZodiacWeaponId;
    private static readonly List<List<uint>> AnimaWeaponId = DataBase.AnimaWeaponId;
    private static readonly List<List<uint>> EurekaWeaponId = DataBase.EurekaWeaponId;
    private static readonly List<List<uint>> BozjaWeaponId = DataBase.BozjaWeaponId;
    private static readonly List<List<uint>> MandervillousWeaponId = DataBase.MandervillousWeaponId;
    private static readonly List<List<uint>> PhantomWeaponId = DataBase.PhantomWeaponId;
    private static readonly List<List<uint>> SkysteelWeaponId = DataBase.SkysteelWeaponId;
    private static readonly List<List<uint>> SplendorousWeaponId = DataBase.SplendorousWeaponId;
    private static readonly List<List<uint>> UltimateWeaponId = DataBase.UltimateWeaponId;

    private static readonly List<uint> AnimaWeaponJobIdList = DataBase.AnimaWeaponJobIdList;
    private static readonly List<uint> ZodiacWeaponJobIdList = DataBase.ZodiacWeaponJobIdList;
    private static readonly List<uint> EurekaWeaponJobIdList = DataBase.EurekaWeaponJobIdList;
    private static readonly List<uint> BozjaWeaponJobIdList = DataBase.BozjaWeaponJobIdList;
    private static readonly List<uint> MandervillousWeaponJobIdList = DataBase.MandervillousWeaponJobIdList;
    private static readonly List<uint> PhantomWeaponJobIdList = DataBase.PhantomWeaponJobIdList;
    private static readonly List<uint> SkysteelWeaponJobIdList = DataBase.SkysteelWeaponJobIdList;
    private static readonly List<uint> SplendorousWeaponJobIdList = DataBase.SplendorousWeaponJobIdList;
    private static readonly List<uint> UltimateWeaponJobIdList = DataBase.UltimateWeaponJobIdList;
    
    private Dictionary<uint, List<int>> zodiacWeaponProcess = new();
    private Dictionary<uint, List<int>> animaWeaponProcess = new();
    private Dictionary<uint, List<int>> eurekaWeaponProcess = new();
    private Dictionary<uint, List<int>> bozjaWeaponProcess = new();
    private Dictionary<uint, List<int>> mandervillousWeaponProcess = new();
    private Dictionary<uint, List<int>> phantomWeaponProcess = new();
    private Dictionary<uint, List<int>> skysteelWeaponProcess = new();
    private Dictionary<uint, List<int>> splendorousWeaponProcess = new();
    private Dictionary<uint, List<int>> ultimateWeaponProcess = new();

    private readonly string[] specialWeaponSeriesList =
    [
        "未选中", "古武", "魂武", "优武", "义武", "曼武", "幻武", "天钢", "莫雯", "绝境战"
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
            switch (selectedWeaponSeriesIndex)
            {
                case 1:
                {
                    process?.GetProcessData(1, ZodiacWeaponId, ZodiacWeaponJobIdList, ref zodiacWeaponProcess);
                    DrawZodiac();
                    break;
                }
                case 2:
                {
                    process?.GetProcessData(2, AnimaWeaponId, AnimaWeaponJobIdList, ref animaWeaponProcess);
                    DrawAnima();
                    break;
                }
                case 3:
                {
                    process?.GetProcessData(3, EurekaWeaponId, EurekaWeaponJobIdList, ref eurekaWeaponProcess);
                    DrawEureka();
                    break;
                }
                case 4:
                {
                    process?.GetProcessData(4, BozjaWeaponId, BozjaWeaponJobIdList, ref bozjaWeaponProcess);
                    DrawBozja();
                    break;
                }
                case 5:
                {
                    process?.GetProcessData(5, MandervillousWeaponId, MandervillousWeaponJobIdList,
                                            ref mandervillousWeaponProcess);
                    DrawMandervillous();
                    break;
                }
                case 6:
                {
                    process?.GetProcessData(6, PhantomWeaponId, PhantomWeaponJobIdList, ref phantomWeaponProcess);
                    DrawPhantom();
                    break;
                }
                case 7:
                {
                    process?.GetProcessData(7, SkysteelWeaponId, SkysteelWeaponJobIdList, ref skysteelWeaponProcess);
                    DrawSkysteel();
                    break;
                }
                case 8:
                {
                    process?.GetProcessData(8, SplendorousWeaponId, SplendorousWeaponJobIdList, ref splendorousWeaponProcess);
                    DrawSplendorous();
                    break;
                }
                case 9:
                {
                    process?.GetProcessData(9, UltimateWeaponId, UltimateWeaponJobIdList, ref ultimateWeaponProcess);
                    DrawUltimate();
                    break;
                }
            }
        }
    }

    public void InitChart()
    {
        process = new Process(GetItemCountTotal);

        for (var i = 1; i <= 9; i++)
        {
            switch (i)
            {
                case 1:
                    zodiacWeaponProcess = process.InitChart(i);
                    break;
                case 2:
                    animaWeaponProcess = process.InitChart(i);
                    break;
                case 3:
                    eurekaWeaponProcess = process.InitChart(i);
                    break;
                case 4:
                    bozjaWeaponProcess = process.InitChart(i);
                    break;
                case 5:
                    mandervillousWeaponProcess = process.InitChart(i);
                    break;
                case 6:
                    phantomWeaponProcess = process.InitChart(i);
                    break;
                case 7:
                    skysteelWeaponProcess = process.InitChart(i);
                    break;
                case 8:
                    splendorousWeaponProcess = process.InitChart(i);
                    break;
                case 9:
                    ultimateWeaponProcess = process.InitChart(i);
                    break;
            }
        }
        
        zodiacDrawer = new ZodiacDrawer(ZodiacWeaponId, ZodiacWeaponJobIdList, zodiacWeaponProcess);
        animaDrawer = new AnimaDrawer(AnimaWeaponId, AnimaWeaponJobIdList, animaWeaponProcess);
        eurekaDrawer = new EurekaDrawer(EurekaWeaponId, EurekaWeaponJobIdList, eurekaWeaponProcess);
        bozjaDrawer = new BozjaDrawer(BozjaWeaponId, BozjaWeaponJobIdList, bozjaWeaponProcess, GetItemCountTotal);
        mandervillousDrawer = new MandervillousDrawer(MandervillousWeaponId, MandervillousWeaponJobIdList, mandervillousWeaponProcess, GetItemCountTotal);
        phantomDrawer = new PhantomDrawer(PhantomWeaponId, PhantomWeaponJobIdList, phantomWeaponProcess);
        skysteelDrawer = new SkysteelDrawer(SkysteelWeaponId, SkysteelWeaponJobIdList, skysteelWeaponProcess);
        splendorousDrawer = new SplendorousDrawer(SplendorousWeaponId, SplendorousWeaponJobIdList, splendorousWeaponProcess);
        ultimateDrawer = new UltimateDrawer(UltimateWeaponId, UltimateWeaponJobIdList, ultimateWeaponProcess);
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
