using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/pover";

    public readonly WindowSystem WindowSystem = new("SpecialWeaponProgressOverview");
    private MainWindow MainWindow { get; init; }
    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        MainWindow = new MainWindow();
        PluginService.Init(pluginInterface);
        WindowSystem.AddWindow(MainWindow);

        PluginService.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open main page"
        });

        pluginInterface.UiBuilder.Draw += DrawUi;
        pluginInterface.UiBuilder.OpenMainUi += ToggleMainWindow;
        MainWindow.Init();
        MainWindow.InitChart();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        MainWindow.Dispose();

        PluginService.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainWindow();
    }

    private void DrawUi() => WindowSystem.Draw();

    public void ToggleMainWindow() => MainWindow.Toggle();

}
