using System.Collections.Generic;
using SpecialWeaponProgressOverview.Data;

namespace SpecialWeaponProgressOverview.Models;

/// <summary>职业索引类型。</summary>
public enum JobIndexKind { Standard, New, Life }

/// <summary>武器系列完整元数据，消除分散在各处的魔法数字和字典。</summary>
public sealed class WeaponSeriesInfo
{
    public WeaponSeries Series            { get; init; }
    public string       DisplayName       { get; init; } = string.Empty;
    public string       ChineseName       { get; init; } = string.Empty;
    public List<uint>   JobIdList         { get; init; } = [];
    public List<List<uint>> WeaponIdStages  { get; init; } = [];
    public List<string>     PhaseNames      { get; init; } = [];
    public JobIndexKind     JobIndexKind    { get; init; } = JobIndexKind.Standard;

    /// <summary>是否需要材料需求计算</summary>
    public bool HasMaterialComputation  { get; init; }

    /// <summary>最终阶段索引列表。为空时默认仅最后阶段为最终阶段；
    /// 优武特例：优雷卡(index 13) 与 优雷卡·改(index 14) 均视为最终阶段。</summary>
    public List<int> FinalStageIndices { get; init; } = [];

    /// <summary>判断指定阶段是否为最终阶段。
    /// 若 <see cref="FinalStageIndices"/> 非空则按其判断，否则取最后阶段。</summary>
    public bool IsFinalStage(int stageIndex)
        => FinalStageIndices.Count > 0
            ? FinalStageIndices.Contains(stageIndex)
            : stageIndex == WeaponIdStages.Count - 1;

    /// <summary>根据职业ID和阶段索引解析该职业在武器阶段列表中的索引。</summary>
    public int ResolveJobIndex(uint jobId, int stageIndex = 0)
    {
        // 绝境战特殊处理：前5个绝本用旧索引，后续用新索引
        if (Series == WeaponSeries.Ultimate)
            return stageIndex < 5
                ? (DataBase.JobIndex.TryGetValue(jobId, out var jidx) ? jidx : -1)
                : (DataBase.NewJobIndex.TryGetValue(jobId, out var nidx) ? nidx : -1);

        return JobIndexKind switch
        {
            JobIndexKind.New  => DataBase.NewJobIndex.TryGetValue(jobId, out var nidx) ? nidx : -1,
            JobIndexKind.Life => DataBase.LifeJobIndex.TryGetValue(jobId, out var lidx) ? lidx : -1,
            _ => DataBase.JobIndex.TryGetValue(jobId, out var idx) ? idx : -1,
        };
    }

    // ---- 9 系列的全局注册表 ----
    public static readonly WeaponSeriesInfo Zodiac = new()
    {
        Series       = WeaponSeries.Zodiac,
        DisplayName  = "Zodiac",
        ChineseName  = "古武",
        JobIdList    = DataBase.ZodiacWeaponJobIdList,
        WeaponIdStages = DataBase.ZodiacWeaponId,
        PhaseNames   = ["半成品", "本体", "天极", "魂晶", "魂灵", "新星", "镇魂", "黄道武器", "黄道武器·本我"],
        JobIndexKind = JobIndexKind.New,
    };

    public static readonly WeaponSeriesInfo Anima = new()
    {
        Series       = WeaponSeries.Anima,
        DisplayName  = "Anima",
        ChineseName  = "魂武",
        JobIdList    = DataBase.AnimaWeaponJobIdList,
        WeaponIdStages = DataBase.AnimaWeaponId,
        PhaseNames   = ["元灵武器·元灵", "元灵武器·觉醒", "新元灵武器", "元灵武器·超导",
                        "百炼成钢的元灵武器", "元灵武器·灵慧", "真元灵武器", "真元灵武器·灵光"],
    };

    public static readonly WeaponSeriesInfo Eureka = new()
    {
        Series       = WeaponSeries.Eureka,
        DisplayName  = "Eureka",
        ChineseName  = "优武",
        JobIdList    = DataBase.EurekaWeaponJobIdList,
        WeaponIdStages = DataBase.EurekaWeaponId,
        PhaseNames   = ["禁地兵装", "禁地兵装+1", "禁地兵装+2", "常风", "恒冰", "恒冰+1",
                        "元素", "元素+1", "元素+2", "涌火", "丰水", "丰水+1",
                        "新兵装", "优雷卡", "优雷卡·改"],
        HasMaterialComputation = true,
        // 优雷卡(13) 与 优雷卡·改(14) 均视为最终阶段
        FinalStageIndices = [13, 14],
    };

    public static readonly WeaponSeriesInfo Bozja = new()
    {
        Series       = WeaponSeries.Bozja,
        DisplayName  = "Bozja",
        ChineseName  = "义武",
        JobIdList    = DataBase.BozjaWeaponJobIdList,
        WeaponIdStages = DataBase.BozjaWeaponId,
        PhaseNames   = ["义军武器", "改良型义军武器", "回忆", "裁决", "改良型裁决", "女王武器"],
        HasMaterialComputation = true,
    };

    public static readonly WeaponSeriesInfo Mandervillous = new()
    {
        Series       = WeaponSeries.Mandervillous,
        DisplayName  = "Mandervillous",
        ChineseName  = "曼武",
        JobIdList    = DataBase.MandervillousWeaponJobIdList,
        WeaponIdStages = DataBase.MandervillousWeaponId,
        PhaseNames   = ["曼德维尔武器", "曼德维尔武器·惊异", "曼德维尔武器·威严", "曼德维尔武器·盈满"],
        HasMaterialComputation = true,
    };

    public static readonly WeaponSeriesInfo Phantom = new()
    {
        Series       = WeaponSeries.Phantom,
        DisplayName  = "Phantom",
        ChineseName  = "幻武",
        JobIdList    = DataBase.PhantomWeaponJobIdList,
        WeaponIdStages = DataBase.PhantomWeaponId,
        PhaseNames   = ["幻境武器·半影", "幻境武器·本影", "幻境武器·黯影"],
        JobIndexKind = JobIndexKind.New,
        HasMaterialComputation = true,
    };

    public static readonly WeaponSeriesInfo Skysteel = new()
    {
        Series       = WeaponSeries.Skysteel,
        DisplayName  = "Skysteel",
        ChineseName  = "天钢",
        JobIdList    = DataBase.SkysteelWeaponJobIdList,
        WeaponIdStages = DataBase.SkysteelWeaponId,
        PhaseNames   = ["天钢工具", "天钢工具+1", "龙诗工具", "改良型龙诗工具", "天诗工具", "天工工具"],
        JobIndexKind = JobIndexKind.Life,
    };

    public static readonly WeaponSeriesInfo Splendorous = new()
    {
        Series       = WeaponSeries.Splendorous,
        DisplayName  = "Splendorous",
        ChineseName  = "莫雯",
        JobIdList    = DataBase.SplendorousWeaponJobIdList,
        WeaponIdStages = DataBase.SplendorousWeaponId,
        PhaseNames   = ["卓越工具", "改良型卓越工具", "水晶工具", "乔菈水晶工具",
                        "乔菈卓绝工具", "诺弗兰特远见工具", "领航星工具"],
        JobIndexKind = JobIndexKind.Life,
    };

    public static readonly WeaponSeriesInfo Ultimate = new()
    {
        Series       = WeaponSeries.Ultimate,
        DisplayName  = "Ultimate",
        ChineseName  = "绝境战",
        JobIdList    = DataBase.UltimateWeaponJobIdList,
        WeaponIdStages = DataBase.UltimateWeaponId,
        PhaseNames   = ["绝巴哈", "绝神兵", "绝亚", "绝龙诗", "绝欧米茄", "绝伊甸", "绝妖星"],
    };

    /// <summary>按枚举索引的快速查找表。</summary>
    public static readonly IReadOnlyDictionary<WeaponSeries, WeaponSeriesInfo> All =
        new Dictionary<WeaponSeries, WeaponSeriesInfo>
        {
            [WeaponSeries.Zodiac]        = Zodiac,
            [WeaponSeries.Anima]         = Anima,
            [WeaponSeries.Eureka]        = Eureka,
            [WeaponSeries.Bozja]         = Bozja,
            [WeaponSeries.Mandervillous] = Mandervillous,
            [WeaponSeries.Phantom]       = Phantom,
            [WeaponSeries.Skysteel]      = Skysteel,
            [WeaponSeries.Splendorous]   = Splendorous,
            [WeaponSeries.Ultimate]      = Ultimate,
        };

    /// <summary>横向 Tab 显示名称（含"总览"）。</summary>
    public static readonly string[] TabNames =
        ["总览", "古武", "魂武", "优武", "义武", "曼武", "幻武", "天钢", "莫雯", "绝境战"];

    /// <summary>Tab 索引 → 枚举。</summary>
    public static WeaponSeries FromComboIndex(int index) => index switch
    {
        1 => WeaponSeries.Zodiac,
        2 => WeaponSeries.Anima,
        3 => WeaponSeries.Eureka,
        4 => WeaponSeries.Bozja,
        5 => WeaponSeries.Mandervillous,
        6 => WeaponSeries.Phantom,
        7 => WeaponSeries.Skysteel,
        8 => WeaponSeries.Splendorous,
        9 => WeaponSeries.Ultimate,
        _ => WeaponSeries.None,
    };
}
