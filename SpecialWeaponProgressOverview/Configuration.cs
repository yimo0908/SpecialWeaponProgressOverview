using System;
using Dalamud.Configuration;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public void Save()
    {
        PluginService.PluginInterface.SavePluginConfig(this);
    }
}
