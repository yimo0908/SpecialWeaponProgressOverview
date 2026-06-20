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
        var stageCount = weaponIdStages.Count;
        var needs      = ComputeStageNeeds(weaponIdStages, jobIdList, getItemCountTotal);

        var recipes = DataBase.BozjaMaterialRecipes;
        var totalNeeded = new Dictionary<uint, int>();
        for (var stage = 0; stage < stageCount; stage++)
        {
            foreach (var (itemId, count) in recipes[stage])
            {
                totalNeeded.TryGetValue(itemId, out var existing);
                totalNeeded[itemId] = existing + needs[stage] * count;
            }
        }

        var materialIds = totalNeeded.Keys.ToList();
        var neededList  = materialIds.Select(id => totalNeeded[id]).ToList();
        var haveList    = materialIds.Select(id => getItemCountTotal(id)).ToList();

        return BuildMaterialString("需要", neededList, materialIds)
             + BuildMaterialString("仍需", SubtractLists(neededList, haveList), materialIds);
    }

    /// <summary>计算曼德维尔武器所需材料（陨石系列 + 诗学）。</summary>
    public static string ComputeNeedsMandervillous(
        List<List<uint>> weaponIdStages,
        List<uint>       jobIdList,
        Func<uint, int>  getItemCountTotal)
    {
        var stageCount = weaponIdStages.Count;
        var needs      = ComputeStageNeeds(weaponIdStages, jobIdList, getItemCountTotal);

        var recipes = DataBase.MandervillousMaterialRecipes;
        var totalNeeded = new Dictionary<uint, int>();
        for (var stage = 0; stage < stageCount; stage++)
        {
            foreach (var (itemId, count) in recipes[stage])
            {
                totalNeeded.TryGetValue(itemId, out var existing);
                totalNeeded[itemId] = existing + needs[stage] * count;
            }
        }

        var materialIds = totalNeeded.Keys.ToList();
        var neededList  = materialIds.Select(id => totalNeeded[id]).ToList();
        var haveList    = materialIds.Select(id => getItemCountTotal(id)).ToList();
        var missingList = SubtractLists(neededList, haveList);

        var totalPoetics = needs.Sum() * 1500;  // 每阶段 ×3 材料 ×500 诗学

        return BuildMaterialString("需要", neededList, materialIds)
             + BuildMaterialString("仍需", missingList, materialIds)
             + $"共计: {totalPoetics}诗学神典石";
    }

    /// <summary>计算幻境武器所需材料。</summary>
    public static string ComputeNeedsPhantom(
        List<List<uint>> weaponIdStages,
        List<uint>       jobIdList,
        Func<uint, int>  getItemCountTotal)
    {
        var stageCount = weaponIdStages.Count;
        var needs      = ComputeStageNeeds(weaponIdStages, jobIdList, getItemCountTotal);

        var recipes     = DataBase.PhantomMaterialRecipes;
        var totalNeeded = new Dictionary<uint, int>();
        for (var stage = 0; stage < stageCount; stage++)
        {
            foreach (var (itemId, count) in recipes[stage])
            {
                totalNeeded.TryGetValue(itemId, out var existing);
                totalNeeded[itemId] = existing + needs[stage] * count;
            }
        }

        var materialIds = totalNeeded.Keys.ToList();
        var neededList  = materialIds.Select(id => totalNeeded[id]).ToList();
        var haveList    = materialIds.Select(id => getItemCountTotal(id)).ToList();
        var missingList = SubtractLists(neededList, haveList);

        var totalPoetics = missingList.Sum() * 500;  // 每个材料 ×500 数理神典石

        return BuildMaterialString("需要", neededList, materialIds)
             + BuildMaterialString("仍需", missingList, materialIds)
             + $"共计: {totalPoetics}数理神典石";
    }

    /// <summary>计算优武所需材料。</summary>
    public static string ComputeNeedsEureka(
        List<List<uint>> weaponIdStages,
        List<uint>       jobIdList,
        Func<uint, int>  getItemCountTotal)
    {
        var stageCount = weaponIdStages.Count;
        var needs      = ComputeStageNeeds(weaponIdStages, jobIdList, getItemCountTotal);

        // 按材料种类聚合（同一种材料出现在多个阶段时合并）
        var recipes = DataBase.EurekaMaterialRecipes;
        var totalNeeded = new Dictionary<uint, int>();
        for (var stage = 0; stage < stageCount; stage++)
        {
            foreach (var (itemId, count) in recipes[stage])
            {
                totalNeeded.TryGetValue(itemId, out var existing);
                totalNeeded[itemId] = existing + needs[stage] * count;
            }
        }

        var materialIds = totalNeeded.Keys.ToList();
        var neededList  = materialIds.Select(id => totalNeeded[id]).ToList();
        var haveList    = materialIds.Select(id => getItemCountTotal(id)).ToList();

        return BuildMaterialString("需要", neededList, materialIds)
             + BuildMaterialString("仍需", SubtractLists(neededList, haveList), materialIds);
    }

    // ---- 工具方法 ----

    /// <summary>扫描武器进度，返回各阶段仍需攻克的职业数。
    /// 不持有任意阶段武器的职业视为需要从阶段 0 开始。</summary>
    private static List<int> ComputeStageNeeds(
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
            var hasAny   = false;

            for (var j = 0; j < stageCount; j++)
            {
                if (getItemCountTotal(weaponIdStages[j][i]) > 0)
                {
                    hasAny = true;
                    AddOneToFollowing(weaponNeed[curJobId], j);
                }
            }

            // 该职业没有任何阶段的武器 → 需要从阶段 0 开始全部推进
            if (!hasAny)
            {
                for (var j = 0; j < stageCount; j++)
                    weaponNeed[curJobId][j] = 1;
            }
        }

        var needs = Enumerable.Repeat(0, stageCount).ToList();
        foreach (var jobId in jobIdList)
        {
            for (var i = 0; i < stageCount; i++)
                needs[i] += weaponNeed[jobId][i];
        }

        return needs;
    }

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
