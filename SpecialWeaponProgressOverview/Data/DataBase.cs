using System.Collections.Generic;
using System.Linq;

namespace SpecialWeaponProgressOverview.Data;

public static class DataBase
{
    public static readonly List<List<uint>> ZodiacWeaponId =
    [
        [1665, 1735, 1805, 1874, 1944, 2046, 2135, 2191, 2192, 7887],           //半成品，50
        [1675, 1746, 1816, 1885, 1955, 2052, 2140, 2213, 2214, 7888],           //本体，80
        [6257, 6258, 6259, 6260, 6261, 6262, 6263, 6264, 6265, 9250],           //天极，90
        [7824, 7825, 7826, 7827, 7828, 7829, 7830, 7831, 7832, 9251],           //魂晶，100
        [7834, 7835, 7836, 7837, 7838, 7839, 7840, 7841, 7842, 9252],           //魂灵，100
        [7863, 7864, 7865, 7866, 7867, 7868, 7869, 7870, 7871, 9253],           //新星，110
        [8649, 8650, 8651, 8652, 8653, 8654, 8655, 8656, 8657, 9254],           //镇魂，115
        [9491, 9492, 9493, 9494, 9495, 9496, 9497, 9498, 9499, 9501],           //黄道武器
        [10054, 10055, 10056, 10057, 10058, 10059, 10060, 10061, 10062, 10064], //黄道武器·本我
    ];
    
    public static readonly List<List<uint>> AnimaWeaponId =
    [
        Enumerable.Range(13611, 13).Select(i => (uint)i).ToList(), //魂武一阶段，元灵武器·元灵
        Enumerable.Range(13597, 13).Select(i => (uint)i).ToList(), //魂武二阶段，元灵武器·觉醒
        Enumerable.Range(13223, 13).Select(i => (uint)i).ToList(), //魂武三阶段，新元灵武器
        Enumerable.Range(14870, 13).Select(i => (uint)i).ToList(), //魂武四阶段，元灵武器·超导
        Enumerable.Range(15223, 13).Select(i => (uint)i).ToList(), //魂武五阶段，百炼成钢的元灵武器
        Enumerable.Range(15237, 13).Select(i => (uint)i).ToList(), //魂武六阶段，元灵武器·灵慧
        Enumerable.Range(15251, 13).Select(i => (uint)i).ToList(), //魂武七阶段，真元灵武器
        Enumerable.Range(16050, 13).Select(i => (uint)i).ToList(), //魂武八阶段，真元灵武器·灵光
    ];

    public static readonly List<List<uint>> EurekaWeaponId =
    [
        Enumerable.Range(21942, 15).Select(i => (uint)i).ToList(), //禁地兵装
        Enumerable.Range(21958, 15).Select(i => (uint)i).ToList(), //禁地兵装+1
        Enumerable.Range(21974, 15).Select(i => (uint)i).ToList(), //禁地兵装+2
        Enumerable.Range(21990, 15).Select(i => (uint)i).ToList(), //常风
        Enumerable.Range(22925, 15).Select(i => (uint)i).ToList(), //恒冰
        Enumerable.Range(22941, 15).Select(i => (uint)i).ToList(), //恒冰+1
        Enumerable.Range(22957, 15).Select(i => (uint)i).ToList(), //元素
        Enumerable.Range(24039, 15).Select(i => (uint)i).ToList(), //元素+1
        Enumerable.Range(24055, 15).Select(i => (uint)i).ToList(), //元素+2
        Enumerable.Range(24071, 15).Select(i => (uint)i).ToList(), //涌火
        Enumerable.Range(24643, 15).Select(i => (uint)i).ToList(), //丰水
        Enumerable.Range(24659, 15).Select(i => (uint)i).ToList(), //丰水+1
        Enumerable.Range(24675, 15).Select(i => (uint)i).ToList(), //新兵装
        Enumerable.Range(24691, 15).Select(i => (uint)i).ToList(), //优雷卡
        Enumerable.Range(24707, 15).Select(i => (uint)i).ToList(), //优雷卡·改
    ];
    
    public static readonly List<List<uint>> BozjaWeaponId =
    [
        Enumerable.Range(30228, 17).Select(i => (uint)i).ToList(), //义军武器
        Enumerable.Range(30767, 17).Select(i => (uint)i).ToList(), //改良型义军武器
        Enumerable.Range(30785, 17).Select(i => (uint)i).ToList(), //回忆
        Enumerable.Range(32651, 17).Select(i => (uint)i).ToList(), //裁决
        Enumerable.Range(32669, 17).Select(i => (uint)i).ToList(), //改良型裁决
        Enumerable.Range(33462, 17).Select(i => (uint)i).ToList(), //女王武器
    ];

    public static readonly List<List<uint>> MandervillousWeaponId =
    [
        Enumerable.Range(38400, 19).Select(i => (uint)i).ToList(), //曼德维尔武器
        Enumerable.Range(39144, 19).Select(i => (uint)i).ToList(), //曼德维尔武器·惊异
        Enumerable.Range(39920, 19).Select(i => (uint)i).ToList(), //曼德维尔武器·威严
        Enumerable.Range(40932, 19).Select(i => (uint)i).ToList(), //曼德维尔武器·盈满
    ];

    public static readonly List<List<uint>> PhantomWeaponId =
    [
        Enumerable.Range(47869, 21).Select(i => (uint)i).ToList(), //幻境武器·半影
        Enumerable.Range(47006, 21).Select(i => (uint)i).ToList(), //幻境武器·本影
        Enumerable.Range(50032, 21).Select(i => (uint)i).ToList(), //幻境武器·黯影
    ];

    public static readonly List<List<uint>> SkysteelWeaponId =
    [
        Enumerable.Range(29612, 11).Select(i => (uint)i).ToList(), //天钢工具
        Enumerable.Range(29623, 11).Select(i => (uint)i).ToList(), //天钢工具+1
        Enumerable.Range(29634, 13).Select(i => (uint)i).ToList(), //龙诗工具
        Enumerable.Range(30282, 13).Select(i => (uint)i).ToList(), //改良型龙诗工具
        Enumerable.Range(30293, 13).Select(i => (uint)i).ToList(), //天诗工具
        Enumerable.Range(31714, 13).Select(i => (uint)i).ToList(), //天工工具
    ];

    public static readonly List<List<uint>> SplendorousWeaponId =
    [
        Enumerable.Range(38715, 11).Select(i => (uint)i).ToList(), //卓越
        Enumerable.Range(38726, 11).Select(i => (uint)i).ToList(), //改良型卓越
        Enumerable.Range(38737, 11).Select(i => (uint)i).ToList(), //水晶
        Enumerable.Range(39732, 11).Select(i => (uint)i).ToList(), //乔菈水晶
        Enumerable.Range(39743, 11).Select(i => (uint)i).ToList(), //乔菈卓绝
        Enumerable.Range(41180, 11).Select(i => (uint)i).ToList(), //诺弗兰特远见
        Enumerable.Range(41191, 11).Select(i => (uint)i).ToList(), //领航星
    ];

    public static readonly List<List<uint>> UltimateWeaponId =
    [
        Enumerable.Range(20959, 15).Select(i => (uint)i).ToList(), //巴哈
        Enumerable.Range(22868, 15).Select(i => (uint)i).ToList(), //神兵
        Enumerable.Range(28289, 17).Select(i => (uint)i).ToList(), //亚历山大
        Enumerable.Range(36943, 19).Select(i => (uint)i).ToList(), //龙诗
        [
            39164, 39165, 39166, 39167, 39168, 39169, 39170,
            39171, 39172, 39173, 39174, 39175, 39176, 39177,
            39178, 39179, 39180, 39181, 39182, 43642, 43663
        ],                                                         //欧米茄
        Enumerable.Range(44721, 21).Select(i => (uint)i).ToList(), //伊甸
    ];

    // Job ID

    // 19骑士 21战士 32黑骑 37绝枪
    // 24白魔 28学者 33占星 40贤者
    // 20武僧 22龙骑 30忍者 34武士 39镰刀 41蝰蛇
    // 23诗人 31机工 38舞者
    // 25黑魔 27召唤 35赤魔 42画家
    // 8刻木 9锻铁 10铸甲 11雕金
    // 12制革 13裁缝 14炼金 15烹调
    // 16采矿 17园艺 18捕鱼

    public static readonly List<uint> AnimaWeaponJobIdList =
    [
        19, 21, 32,
        24, 28, 33,
        20, 22, 30,
        23, 31,
        25, 27
    ];

    public static readonly List<uint> ZodiacWeaponJobIdList =
    [
        19, 21,
        24, 28,
        20, 22, 30,
        23,
        25, 27
    ];

    public static readonly List<uint> EurekaWeaponJobIdList =
    [
        19, 21, 32,
        24, 28, 33,
        20, 22, 34, 30,
        23, 31,
        25, 27, 35
    ];

    public static readonly List<uint> BozjaWeaponJobIdList =
    [
        19, 21, 32, 37,
        24, 28, 33,
        20, 22, 34, 30,
        23, 31, 38,
        25, 27, 35
    ];

    public static readonly List<uint> MandervillousWeaponJobIdList =
    [
        19, 21, 32, 37,
        24, 28, 33, 40,
        20, 22, 39, 34, 30,
        23, 31, 38,
        25, 27, 35,
    ];

    public static readonly List<uint> PhantomWeaponJobIdList =
    [
        19, 21, 32, 37,
        24, 28, 33, 40,
        20, 22, 39, 34, 30, 41,
        23, 31, 38,
        25, 27, 35, 42,
    ];

    public static readonly List<uint> SkysteelWeaponJobIdList =
    [
        8, 9, 10, 11,
        12, 13, 14, 15,
        16, 17, 18,
    ];

    public static readonly List<uint> SplendorousWeaponJobIdList =
    [
        8, 9, 10, 11,
        12, 13, 14, 15,
        16, 17, 18,
    ];

    public static readonly List<uint> UltimateWeaponJobIdList = // 部分职业暂无老绝本武器，故重新排序
    [
        19, 21, 32, 24, 28, 33, 20,
        22, 30, 34, 23, 31, 25, 27,
        35, 37, 38, 40, 39, 41, 42,
    ];

    public static readonly Dictionary<uint, int> JobIndex = new()
    {
        { 19, 0 }, { 21, 2 }, { 32, 6 }, { 37, 15 },
        { 24, 8 }, { 28, 11 }, { 33, 12 }, { 40, 17 },
        { 20, 1 }, { 22, 3 }, { 34, 13 }, { 39, 18 }, { 30, 5 }, { 41, 19 },
        { 23, 4 }, { 31, 7 }, { 38, 16 },
        { 25, 9 }, { 27, 10 }, { 35, 14 }, { 42, 20 }
    };

    public static readonly Dictionary<uint, int> NewJobIndex = new()
    {
        { 19, 0 }, { 21, 2 }, { 32, 10 }, { 37, 15 },
        { 24, 5 }, { 28, 8 }, { 33, 12 }, { 40, 18 },
        { 20, 1 }, { 22, 3 }, { 34, 13 }, { 39, 17 }, { 30, 9 }, { 41, 19 },
        { 23, 4 }, { 31, 11 }, { 38, 16 },
        { 25, 6 }, { 27, 7 }, { 35, 14 }, { 42, 20 }
    };

    public static readonly Dictionary<uint, int> LifeJobIndex = new()
    {
        { 8, 0 }, { 9, 1 }, { 10, 2 }, { 11, 3 },
        { 12, 4 }, { 13, 5 }, { 14, 6 }, { 15, 7 },
        { 16, 8 }, { 17, 9 }, { 18, 10 },
    };
}
