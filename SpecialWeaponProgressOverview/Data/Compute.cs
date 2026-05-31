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

    // ---- 材料需求计算 ----

    /// <summary>计算义军武器所需材料。</summary>
    public static string ComputeNeedsBozja(
        List<List<uint>> weaponIdStages,
        List<uint>       jobIdList,
        Func<uint, int>  getItemCountTotal)
    {
        var jobCount   = jobIdList.Count;
        var stageCount = weaponIdStages.Count;

        // 初始化进度矩阵
        var weaponNeed = new Dictionary<uint, List<int>>();
        for (var i = 0; i < jobCount; i++)
            weaponNeed[jobIdList[i]] = Enumerable.Repeat(0, stageCount).ToList();

        // 扫描已持有武器，标记进度
        for (var i = 0; i < jobCount; i++)
        {
            var curJobId = jobIdList[i];
            for (var j = 0; j < stageCount; j++)
            {
                if (getItemCountTotal(weaponIdStages[j][i]) > 0)
                    AddOneToFollowing(weaponNeed[curJobId], j);
            }
        }

        // 汇总需求
        var needs    = Enumerable.Repeat(0, stageCount).ToList();
        foreach (var jobId in jobIdList)
        {
            for (var i = 0; i < stageCount; i++)
                needs[i] += weaponNeed[jobId][i];
        }

        // 义武材料倍数
        needs[0] *= 4;
        needs[1] *= 20;
        needs[2] *= 6;
        needs[3] *= 15;
        needs[4] *= 15;
        needs[5] *= 15;

        // 阶段 → 实际材料映射
        var expandedNeeds = new List<int>   { needs[0], needs[1], needs[1], needs[1], needs[2], needs[3], needs[4], needs[5] };
        var materialIds   = new List<uint>  { 30273u, 31573u, 31574u, 31575u, 31576u, 32956u, 32959u, 33767u };

        var have = materialIds.Select(id => getItemCountTotal(id)).ToList();

        return BuildMaterialString("需要", expandedNeeds, materialIds)
             + BuildMaterialString("仍需", SubtractLists(expandedNeeds, have), materialIds);
    }

    /// <summary>计算曼德维尔武器所需材料（陨石系列 + 诗学）。</summary>
    public static string ComputeNeedsMandervillous(
        List<List<uint>> weaponIdStages,
        List<uint>       jobIdList,
        Func<uint, int>  getItemCountTotal)
    {
        var jobCount   = jobIdList.Count;
        var stageCount = weaponIdStages.Count;

        var weaponNeed = new Dictionary<uint, List<int>>();
        for (var i = 0; i < jobCount; i++)
            weaponNeed[jobIdList[i]] = Enumerable.Repeat(0, stageCount).ToList();

        for (var i = 0; i < jobCount; i++)
        {
            var curJobId = jobIdList[i];
            for (var j = 0; j < stageCount; j++)
            {
                if (getItemCountTotal(weaponIdStages[j][i]) > 0)
                    AddOneToFollowing(weaponNeed[curJobId], j);
            }
        }

        var needs = Enumerable.Repeat(0, stageCount).ToList();
        foreach (var jobId in jobIdList)
        {
            for (var i = 0; i < stageCount; i++)
                needs[i] += 3 * weaponNeed[jobId][i];
        }

        var have = new List<int>
        {
            getItemCountTotal(38420u),  // 稀少陨石
            getItemCountTotal(38940u),  // 稀少球粒陨石
            getItemCountTotal(40322u),  // 稀少无球粒陨石
            getItemCountTotal(41032u),  // 雏晶
        };

        var missing = SubtractLists(needs, have);
        var total   = (needs[0] + needs[1] + needs[2] + needs[3]) * 500;

        return $"需要: {needs[0]}个稀少陨石, {needs[1]}个稀少球粒陨石, {needs[2]}个稀少无球粒陨石, {needs[3]}个雏晶\n"
             + $"仍需: {missing[0]}个稀少陨石, {missing[1]}个稀少球粒陨石, {missing[2]}个稀少无球粒陨石, {missing[3]}个雏晶\n"
             + $"共计: {total}诗学神典石";
    }

    /// <summary>计算幻境武器所需材料。</summary>
    public static string ComputeNeedsPhantom(
        Dictionary<uint, List<int>> phantomWeaponProcess,
        List<uint>                  jobIdList,
        Func<uint, int>             getItemCountTotal,
        List<uint>?                 materialItemIds    = null,
        List<int>?                  requiredPerPhase   = null)
    {
        materialItemIds  ??= [47750u, 46850u, 50058u];
        requiredPerPhase ??= [3, 3, 3];

        var n     = Math.Min(materialItemIds.Count, requiredPerPhase.Count);
        var needs = Enumerable.Repeat(0, n).ToList();
        var have  = materialItemIds.Take(n).Select(id => getItemCountTotal(id)).ToList();

        foreach (var jobId in jobIdList)
        {
            if (!phantomWeaponProcess.TryGetValue(jobId, out var process))
                process = [];

            for (var p = 0; p < n; p++)
            {
                var curVal = p < process.Count ? process[p] : 0;
                if (curVal > 0) continue;

                // 如果后面阶段已有，则无需此阶段材料
                var laterOwned = false;
                for (var q = p + 1; q < n; q++)
                {
                    var qVal = q < process.Count ? process[q] : 0;
                    if (qVal > 0) { laterOwned = true; break; }
                }

                if (!laterOwned)
                    needs[p] += requiredPerPhase[p];
            }
        }

        var needParts   = new List<string>();
        var remainParts = new List<string>();
        for (var i = 0; i < n; i++)
        {
            var name    = ItemSheet.GetRow(materialItemIds[i]).Name.ExtractText();
            needParts.Add($"{needs[i]}个{name}");
            var remaining = Math.Max(0, needs[i] - have[i]);
            remainParts.Add($"{remaining}个{name}");
        }

        var totalMissing = Enumerable.Range(0, n).Sum(i => Math.Max(0, needs[i] - have[i]));
        var totalCost    = totalMissing * 500;

        return $"需要: {string.Join(", ", needParts)}\n"
             + $"仍需: {string.Join(", ", remainParts)}\n"
             + $"共计: {totalCost}天道神典石";
    }

    // ---- 工具方法 ----

    private static void AddOneToFollowing(List<int> array, int currentIndex)
    {
        for (var i = currentIndex + 1; i < array.Count; i++)
            array[i] += 1;
    }

    private static List<int> SubtractLists(List<int> a, List<int> b)
    {
        var result = new List<int>(a.Count);
        for (var i = 0; i < a.Count; i++)
            result.Add(i < b.Count ? Math.Max(0, a[i] - b[i]) : a[i]);
        return result;
    }

    private static string BuildMaterialString(
        string prefix, List<int> counts, List<uint> itemIds)
    {
        var parts = new List<string>();
        for (var i = 0; i < counts.Count; i++)
        {
            if (counts[i] <= 0) continue;
            var name = ItemSheet.GetRow(itemIds[i]).Name.ExtractText();
            parts.Add($"{counts[i]}个{name}");
        }
        return parts.Count == 0 ? string.Empty : $"{prefix}: {string.Join(", ", parts)}\n";
    }
}
