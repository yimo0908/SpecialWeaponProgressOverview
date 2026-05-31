using System.Collections.Generic;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Drawer;

public class ZodiacDrawer : WeaponSeriesDrawer
{
    public ZodiacDrawer(
        List<List<uint>> weaponIds,
        List<uint> jobIdList,
        Dictionary<uint, List<int>> process)
        : base(WeaponSeriesInfo.Zodiac, process) { }
}
