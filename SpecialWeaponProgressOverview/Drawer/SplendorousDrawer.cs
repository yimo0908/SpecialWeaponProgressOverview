using System.Collections.Generic;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Drawer;

public class SplendorousDrawer : WeaponSeriesDrawer
{
    public SplendorousDrawer(
        List<List<uint>> weaponIds,
        List<uint> jobIdList,
        Dictionary<uint, List<int>> process)
        : base(WeaponSeriesInfo.Splendorous, process) { }
}
