using System;
using System.Collections.Generic;
using System.Linq;
using SpecialWeaponProgressOverview.Base;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Data;

public static class Compute
{
    // ---- 结构化材料需求结果 ----

    /// <summary>材料需求结果：包含多个分段和可选的页脚文本（如神典石）。</summary>
    public sealed class MaterialNeedsResult
    {
        public List<MaterialNeedsSection> Sections { get; } = new();
        public string? Footer { get; set; }
    }

    /// <summary>单个分段（如"需要"/"仍需"），包含材料 ItemId 与数量列表。</summary>
    public sealed class MaterialNeedsSection
    {
        public required string Prefix { get; init; }
        public required List<(uint ItemId, int Count)> Entries { get; init; }
    }

    // ---- 材料需求计算 ----

    /// <summary>计算义军武器所需材料。</summary>
    public static MaterialNeedsResult? ComputeNeedsBozja(
        WeaponSeriesInfo info,
        Func<uint, int>  getItemCountTotal)
    {
        var (materialIds, neededList, missingList) =
            ComputeMaterialNeedsCore(info, DataBase.BozjaMaterialRecipes, getItemCountTotal);

        return BuildResult("需要", neededList, "仍需", missingList, materialIds);
    }

    /// <summary>计算曼德维尔武器所需材料（陨石系列 + 诗学）。</summary>
    public static MaterialNeedsResult? ComputeNeedsMandervillous(
        WeaponSeriesInfo info,
        Func<uint, int>  getItemCountTotal)
    {
        var (materialIds, neededList, missingList) =
            ComputeMaterialNeedsCore(info, DataBase.MandervillousMaterialRecipes, getItemCountTotal);

        var totalPoetics = missingList.Sum() * 500;
        var result = BuildResult("需要", neededList, "仍需", missingList, materialIds);
        if (result != null)
            result.Footer = $"共计: {totalPoetics}诗学神典石";
        return result;
    }

    /// <summary>计算幻境武器所需材料。</summary>
    public static MaterialNeedsResult? ComputeNeedsPhantom(
        WeaponSeriesInfo info,
        Func<uint, int>  getItemCountTotal)
    {
        var (materialIds, neededList, missingList) =
            ComputeMaterialNeedsCore(info, DataBase.PhantomMaterialRecipes, getItemCountTotal);

        var totalPoetics = missingList.Sum() * 500;
        var result = BuildResult("需要", neededList, "仍需", missingList, materialIds);
        if (result != null)
            result.Footer = $"共计: {totalPoetics}数理神典石";
        return result;
    }

    /// <summary>计算优武所需材料。</summary>
    public static MaterialNeedsResult? ComputeNeedsEureka(
        WeaponSeriesInfo info,
        Func<uint, int>  getItemCountTotal)
    {
        var (materialIds, neededList, missingList) =
            ComputeMaterialNeedsCore(info, DataBase.EurekaMaterialRecipes, getItemCountTotal);

        return BuildResult("需要", neededList, "仍需", missingList, materialIds);
    }

    // ---- 核心计算逻辑 ----

    /// <summary>核心材料需求计算：聚合配方、对比持有量，返回材料 ID、需要量、仍需量列表。</summary>
    internal static (List<uint> materialIds, List<int> neededList, List<int> missingList)
        ComputeMaterialNeedsCore(
            WeaponSeriesInfo                     info,
            List<List<(uint ItemId, int Count)>> recipes,
            Func<uint, int>                      getItemCountTotal)
    {
        var stageCount  = info.WeaponIdStages.Count;
        var needs       = ComputeStageNeeds(info, getItemCountTotal);

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

        return (materialIds, neededList, missingList);
    }

    // ---- 工具方法 ----

    /// <summary>扫描武器进度，返回各阶段仍需攻克的职业数。
    /// 不持有任意阶段武器的职业视为需要从阶段 0 开始。</summary>
    internal static List<int> ComputeStageNeeds(
        WeaponSeriesInfo info,
        Func<uint, int>  getItemCountTotal)
    {
        var jobIdList   = info.JobIdList;
        var jobCount    = jobIdList.Count;
        var stageCount  = info.WeaponIdStages.Count;

        // 直接按阶段累计，无需中间 Dictionary<uint, List<int>>
        var needs = new int[stageCount];

        for (var i = 0; i < jobCount; i++)
        {
            var curJobId = jobIdList[i];
            var hasAny   = false;

            for (var j = 0; j < stageCount; j++)
            {
                var jobIndex = info.ResolveJobIndex(curJobId, j);
                if (jobIndex < 0 || jobIndex >= info.WeaponIdStages[j].Count)
                    continue;

                if (getItemCountTotal(info.WeaponIdStages[j][jobIndex]) > 0)
                {
                    hasAny = true;
                    // 拥有阶段 j 的武器 → 后续阶段 j+1..end 各需 +1
                    for (var k = j + 1; k < stageCount; k++)
                        needs[k]++;
                }
            }

            // 该职业没有任何阶段的武器 → 需要从阶段 0 开始全部推进
            if (!hasAny)
            {
                for (var j = 0; j < stageCount; j++)
                    needs[j]++;
            }
        }

        return needs.ToList();
    }

    private static List<int> SubtractLists(List<int> a, List<int> b)
    {
        var result = new List<int>(a.Count);
        for (var i = 0; i < a.Count; i++)
            result.Add(i < b.Count ? Math.Max(0, a[i] - b[i]) : a[i]);
        return result;
    }

    /// <summary>根据需要量/仍需量构建结构化结果。若两组均无内容则返回 null。</summary>
    private static MaterialNeedsResult? BuildResult(
        string neededPrefix, List<int> neededList,
        string missingPrefix, List<int> missingList,
        List<uint> itemIds)
    {
        var result = new MaterialNeedsResult();

        var neededEntries = BuildEntries(neededList, itemIds);
        if (neededEntries.Count > 0)
            result.Sections.Add(new MaterialNeedsSection
            {
                Prefix = neededPrefix,
                Entries = neededEntries
            });

        var missingEntries = BuildEntries(missingList, itemIds);
        if (missingEntries.Count > 0)
            result.Sections.Add(new MaterialNeedsSection
            {
                Prefix = missingPrefix,
                Entries = missingEntries
            });

        return result.Sections.Count > 0 ? result : null;
    }

    private static List<(uint ItemId, int Count)> BuildEntries(
        List<int> counts, List<uint> itemIds)
    {
        var entries = new List<(uint ItemId, int Count)>();
        for (var i = 0; i < counts.Count; i++)
        {
            if (counts[i] <= 0) continue;
            entries.Add((itemIds[i], counts[i]));
        }
        return entries;
    }
}
