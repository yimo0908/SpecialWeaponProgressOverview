using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace SpecialWeaponProgressOverview.Base;

public class PluginService
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; } = null!;
    [PluginService] public static IDataManager DataManager { get; set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; set; } = null!;
    [PluginService] public static IClientState ClientState { get; set; } = null!;
    [PluginService] public static ICondition Condition { get; set; } = null!;

    internal static bool IsInitialized;
    public static void Init(IDalamudPluginInterface pi)
    {
        if (IsInitialized)
        {
            PluginLog.Info("Services already initialized, skipping");
        }
        IsInitialized = true;
        try
        {
            pi.Create<PluginService>();
        }
        catch (Exception ex)
        {
            PluginLog.Error($"Error initialising {nameof(PluginService)}", ex);
        }
    }
}
