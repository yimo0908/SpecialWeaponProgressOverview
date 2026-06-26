using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/pover";

    public readonly WindowSystem WindowSystem = new("SpecialWeaponProgressOverview");
    private readonly MainWindow _mainWindow;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        _mainWindow = new MainWindow();
        PluginService.Init(pluginInterface);

        WindowSystem.AddWindow(_mainWindow);

        PluginService.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open main page"
        });

        pluginInterface.UiBuilder.Draw         += DrawUi;
        pluginInterface.UiBuilder.OpenMainUi   += ToggleMainWindow;

        _mainWindow.InitChart();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        _mainWindow.Dispose();
        PluginService.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args) => ToggleMainWindow();

    private void DrawUi() => WindowSystem.Draw();

    private void ToggleMainWindow() => _mainWindow.Toggle();
}
