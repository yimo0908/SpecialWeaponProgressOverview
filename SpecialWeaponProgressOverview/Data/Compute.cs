using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Data;

public static class Compute
{
    private static readonly ExcelSheet<Item> ItemSheet =
        PluginService.DataManager.GetExcelSheet<Item>();

    public static string ComputeNeedsBozja(
        List<List<uint>> bozjaWeaponId,
        List<uint> bozjaWeaponJobIdList,
        Func<uint, int> getItemCountTotal
    )
    {
        Dictionary<uint, List<int>> bozjaWeaponNeed = new();
        for (var i = 0; i < 17; i++) // JobsOfSpecialWeapon[4] = 17
        {
            bozjaWeaponNeed.Add(
                bozjaWeaponJobIdList[i],
                Enumerable.Repeat(0, bozjaWeaponId.Count).ToList()
            );
        }

        for (var i = 0; i < 17; i++)
        {
            for (var j = 0; j < bozjaWeaponId.Count; j++)
            {
                var curWeaponId = bozjaWeaponId[j][i];
                var curJobId = bozjaWeaponJobIdList[i];
                var curWeaponCount = getItemCountTotal(curWeaponId);
                if (curWeaponCount > 0)
                {
                    AddOneToTheFollowingIndex(bozjaWeaponNeed[curJobId], j);
                }
            }
        }

        var have = new List<int>();
        List<int> needs = [0, 0, 0, 0, 0, 0];
        foreach (var jobId in bozjaWeaponJobIdList)
        {
            for (var i = 0; i < bozjaWeaponId.Count; i++)
            {
                needs[i] += bozjaWeaponNeed[jobId][i];
            }
        }

        needs[0] *= 4;
        needs[1] *= 20;
        needs[2] *= 6;
        needs[3] *= 15;
        needs[4] *= 15;
        needs[5] *= 15;
        List<int> newNeeds = [needs[0], needs[1], needs[1], needs[1], needs[2], needs[3], needs[4], needs[5],];
        List<uint> needItemId = [30273u, 31573u, 31574u, 31575u, 31576u, 32956u, 32959u, 33767u];
        needs = newNeeds;
        foreach (var id in needItemId)
        {
            have.Add(getItemCountTotal(id));
        }

        var res = "需要";
        for (var i = 0; i < needs.Count; i++)
        {
            if (needs[i] == 0)
                continue;
            res += $"{needs[i]}个{ItemSheet.GetRow(needItemId[i]).Name.ExtractText()}, ";
        }

        res += "\n仍需";
        for (var i = 0; i < needs.Count; i++)
        {
            if (needs[i] == 0)
                continue;
            res += $"{needs[i] - have[i]}个{ItemSheet.GetRow(needItemId[i]).Name.ExtractText()}, ";
        }

        return res;
    }

    public static string ComputeNeedsMandervillous(
        List<List<uint>> mandervillousWeaponId,
        List<uint> mandervillousWeaponJobIdList,
        Func<uint, int> getItemCountTotal
    )
    {
        Dictionary<uint, List<int>> mandervillousWeaponNeed = new();
        for (var i = 0; i < 19; i++) // JobsOfSpecialWeapon[5] = 19
        {
            mandervillousWeaponNeed.Add(
                mandervillousWeaponJobIdList[i],
                Enumerable.Repeat(0, mandervillousWeaponId.Count).ToList()
            );
        }

        for (var i = 0; i < 19; i++)
        {
            for (var j = 0; j < mandervillousWeaponId.Count; j++)
            {
                var curWeaponId = mandervillousWeaponId[j][i];
                var curJobId = mandervillousWeaponJobIdList[i];
                var curWeaponCount = getItemCountTotal(curWeaponId);
                if (curWeaponCount > 0)
                {
                    AddOneToTheFollowingIndex(mandervillousWeaponNeed[curJobId], j);
                }
            }
        }

        List<int> have = [getItemCountTotal(38420u), getItemCountTotal(38940u), getItemCountTotal(40322u), getItemCountTotal(41032u),];
        List<int> needs = [0, 0, 0, 0];
        foreach (var jobId in mandervillousWeaponJobIdList)
        {
            for (var i = 0; i < mandervillousWeaponId.Count; i++)
            {
                needs[i] += 3 * mandervillousWeaponNeed[jobId][i];
            }
        }

        var res =
            $"需要: {needs[0]}个稀少陨石, {needs[1]}个稀少球粒陨石, {needs[2]}个稀少无球粒陨石, {needs[3]}个雏晶\n"
            + $"仍需: {needs[0] - have[0]}个稀少陨石, {needs[1] - have[1]}个稀少球粒陨石, {needs[2] - have[2]}个稀少无球粒陨石, {needs[3] - have[3]}个雏晶\n"
            + $"共计: {(needs[0] + needs[1] + needs[2] + needs[3]) * 500}诗学神典石";
        return res;
    }

    public static string ComputeNeedsPhantom(
        Dictionary<uint, List<int>> phantomWeaponProcess,
        List<uint> phantomWeaponJobIdList,
        Func<uint, int> getItemCountTotal,
        List<uint>? materialItemIds = null,
        List<int>? requiredPerPhase = null
    )
    {
        // 默认与原实现行为一致
        // 新增第三阶段所需材料 50058，且每阶段均需 3 个
        materialItemIds ??= new List<uint> { 47750u, 46850u, 50058u };
        requiredPerPhase ??= new List<int> { 3, 3, 3 };

        var n = Math.Min(materialItemIds.Count, requiredPerPhase.Count);
        var needs = Enumerable.Repeat(0, n).ToList();
        var have = materialItemIds.Take(n).Select(id => getItemCountTotal(id)).ToList();

        foreach (var jobId in phantomWeaponJobIdList)
        {
            if (!phantomWeaponProcess.TryGetValue(jobId, out var process))
                process = new List<int>();

            for (var p = 0; p < n; p++)
            {
                // 当阶段 p 未拥有，且没有任何更高阶段 (q > p) 已拥有时，计入该阶段需求
                var curVal = p < process.Count ? process[p] : 0;
                if (curVal > 0)
                    continue;

                var laterOwned = false;
                for (var q = p + 1; q < n; q++)
                {
                    var qVal = q < process.Count ? process[q] : 0;
                    if (qVal > 0)
                    {
                        laterOwned = true;
                        break;
                    }
                }

                if (!laterOwned)
                    needs[p] += requiredPerPhase[p];
            }
        }

        // 构建输出文本
        var needParts = new List<string>();
        var remainParts = new List<string>();
        for (var i = 0; i < n; i++)
        {
            var name = ItemSheet.GetRow(materialItemIds[i]).Name.ExtractText();
            needParts.Add($"{needs[i]}个{name}");
            var remaining = Math.Max(0, needs[i] - have[i]);
            remainParts.Add($"{remaining}个{name}");
        }

        var totalMissing = Enumerable.Range(0, n).Sum(i => Math.Max(0, needs[i] - have[i]));
        var totalCost = totalMissing * 500;

        var res =
            $"需要: {string.Join(", ", needParts)}\n"
            + $"仍需: {string.Join(", ", remainParts)}\n"
            + $"共计: {totalCost}天道神典石";
        return res;
    }

    private static void AddOneToTheFollowingIndex(List<int> array, int currentIndex)
    {
        for (var i = currentIndex + 1; i < array.Count; i++)
        {
            array[i] += 1;
        }
    }
}
