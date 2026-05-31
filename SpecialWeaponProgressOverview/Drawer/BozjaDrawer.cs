using System;
using System.Collections.Generic;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Drawer;

public class BozjaDrawer : WeaponSeriesDrawer
{
    public BozjaDrawer(
        List<List<uint>> weaponIds,
        List<uint> jobIdList,
        Dictionary<uint, List<int>> process,
        Func<uint, int> getItemCountTotal)
        : base(WeaponSeriesInfo.Bozja, process, getItemCountTotal) { }
}
