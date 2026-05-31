using System.Collections.Generic;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Drawer;

public class SkysteelDrawer : WeaponSeriesDrawer
{
    public SkysteelDrawer(
        List<List<uint>> weaponIds,
        List<uint> jobIdList,
        Dictionary<uint, List<int>> process)
        : base(WeaponSeriesInfo.Skysteel, process) { }
}
