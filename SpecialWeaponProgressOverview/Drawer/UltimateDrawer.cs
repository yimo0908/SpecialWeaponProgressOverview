using System.Collections.Generic;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Drawer;

public class UltimateDrawer : WeaponSeriesDrawer
{
    public UltimateDrawer(
        List<List<uint>> weaponIds,
        List<uint> jobIdList,
        Dictionary<uint, List<int>> process)
        : base(WeaponSeriesInfo.Ultimate, process) { }
}
