using System;
using System.Collections.Generic;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Drawer;

public class MandervillousDrawer : WeaponSeriesDrawer
{
    public MandervillousDrawer(
        List<List<uint>> weaponIds,
        List<uint> jobIdList,
        Dictionary<uint, List<int>> process,
        Func<uint, int> getItemCountTotal)
        : base(WeaponSeriesInfo.Mandervillous, process, getItemCountTotal) { }
}
