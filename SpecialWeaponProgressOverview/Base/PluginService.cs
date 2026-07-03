using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace SpecialWeaponProgressOverview.Base;

public class PluginService
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; } = null!;
    [PluginService] public static IDataManager              DataManager     { get; set; } = null!;
    [PluginService] public static ICommandManager           CommandManager  { get; set; } = null!;
    [PluginService] public static IChatGui                  ChatGui         { get; set; } = null!;
    [PluginService] public static IPluginLog                PluginLog       { get; set; } = null!;
    [PluginService] public static IObjectTable              ObjectTable     { get; set; } = null!;
    [PluginService] public static IClientState              ClientState     { get; set; } = null!;
    [PluginService] public static ICondition                Condition       { get; set; } = null!;
    [PluginService] public static ITextureProvider           TextureProvider { get; set; } = null!;
    [PluginService] public static IGameGui                   GameGui        { get; set; } = null!;
    [PluginService] public static IFramework                 Framework      { get; set; } = null!;
    [PluginService] public static IGameInventory              GameInventory  { get; set; } = null!;

    /// <summary>共享 Item 表，避免多处重复 GetExcelSheet 调用。</summary>
    public static ExcelSheet<Item> ItemSheet { get; private set; } = null!;

    private static bool _isInitialized;

    public static void Init(IDalamudPluginInterface pi)
    {
        if (_isInitialized)
            return;

        try
        {
            pi.Create<PluginService>();
            ItemSheet = DataManager.GetExcelSheet<Item>();
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            PluginLog?.Error($"初始化 {nameof(PluginService)} 失败", ex);
            throw;
        }
    }
}
