using System.Collections.Generic;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Drawer;

public class EurekaDrawer : WeaponSeriesDrawer
{
    public EurekaDrawer(
        List<List<uint>> weaponIds,
        List<uint> jobIdList,
        Dictionary<uint, List<int>> process)
        : base(WeaponSeriesInfo.Eureka, process) { }
}
