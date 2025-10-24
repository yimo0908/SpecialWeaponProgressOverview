using System.Collections.Generic;
using System.Linq;
using SpecialWeaponProgressOverview.Data;

namespace SpecialWeaponProgressOverview.Base
{
    public class Process
    {
        private static readonly List<List<uint>> ZodiacWeaponId = DataBase.ZodiacWeaponId;
        private static readonly List<List<uint>> AnimaWeaponId = DataBase.AnimaWeaponId;
        private static readonly List<List<uint>> EurekaWeaponId = DataBase.EurekaWeaponId;
        private static readonly List<List<uint>> BozjaWeaponId = DataBase.BozjaWeaponId;
        private static readonly List<List<uint>> MandervillousWeaponId = DataBase.MandervillousWeaponId;
        private static readonly List<List<uint>> PhantomWeaponId = DataBase.PhantomWeaponId;
        private static readonly List<List<uint>> SkysteelWeaponId = DataBase.SkysteelWeaponId;
        private static readonly List<List<uint>> SplendorousWeaponId = DataBase.SplendorousWeaponId;
        private static readonly List<List<uint>> UltimateWeaponId = DataBase.UltimateWeaponId;

        private static readonly List<uint> AnimaWeaponJobIdList = DataBase.AnimaWeaponJobIdList;
        private static readonly List<uint> ZodiacWeaponJobIdList = DataBase.ZodiacWeaponJobIdList;
        private static readonly List<uint> EurekaWeaponJobIdList = DataBase.EurekaWeaponJobIdList;
        private static readonly List<uint> BozjaWeaponJobIdList = DataBase.BozjaWeaponJobIdList;
        private static readonly List<uint> MandervillousWeaponJobIdList = DataBase.MandervillousWeaponJobIdList;
        private static readonly List<uint> PhantomWeaponJobIdList = DataBase.PhantomWeaponJobIdList;
        private static readonly List<uint> SkysteelWeaponJobIdList = DataBase.SkysteelWeaponJobIdList;
        private static readonly List<uint> SplendorousWeaponJobIdList = DataBase.SplendorousWeaponJobIdList;
        private static readonly List<uint> UltimateWeaponJobIdList = DataBase.UltimateWeaponJobIdList;

        private static readonly Dictionary<uint, int> JobIndex = DataBase.JobIndex;
        private static readonly Dictionary<uint, int> NewJobIndex = DataBase.NewJobIndex;
        private static readonly Dictionary<uint, int> LifeJobIndex = DataBase.LifeJobIndex;

        private static readonly Dictionary<int, int> JobsOfSpecialWeapon = new()
        {
            { 1, 10 }, //古武
            { 2, 13 }, //魂武
            { 3, 15 }, //优武
            { 4, 17 }, //义武
            { 5, 19 }, //曼武
            { 6, 21 }, //幻武
            { 7, 11 }, //天钢
            { 8, 11 }, //莫雯
            { 9, 21 }, //绝境战
        };

        private readonly InventoryWindow.ItemCountDelegate getItemCountTotal;

        public Process(InventoryWindow.ItemCountDelegate getItemCountDelegate)
        {
            getItemCountTotal = getItemCountDelegate;
        }

        public Dictionary<uint, List<int>> InitChart(int weaponIndex)
        {
            var weaponProcess = new Dictionary<uint, List<int>>();
            
            switch (weaponIndex)
            {
                case 1: // 古武
                    for (var i = 0; i < JobsOfSpecialWeapon[1]; i++)
                    {
                        weaponProcess.Add(ZodiacWeaponJobIdList[i], [.. Enumerable.Repeat(0, ZodiacWeaponId.Count)]);
                    }
                    break;
                case 2: // 魂武
                    for (var i = 0; i < JobsOfSpecialWeapon[2]; i++)
                    {
                        weaponProcess.Add(AnimaWeaponJobIdList[i], [.. Enumerable.Repeat(0, AnimaWeaponId.Count)]);
                    }
                    break;
                case 3: // 优武
                    for (var i = 0; i < JobsOfSpecialWeapon[3]; i++)
                    {
                        weaponProcess.Add(EurekaWeaponJobIdList[i], [.. Enumerable.Repeat(0, EurekaWeaponId.Count)]);
                    }
                    break;
                case 4: // 义武
                    for (var i = 0; i < JobsOfSpecialWeapon[4]; i++)
                    {
                        weaponProcess.Add(BozjaWeaponJobIdList[i], [.. Enumerable.Repeat(0, BozjaWeaponId.Count)]);
                    }
                    break;
                case 5: // 曼武
                    for (var i = 0; i < JobsOfSpecialWeapon[5]; i++)
                    {
                        weaponProcess.Add(MandervillousWeaponJobIdList[i], [.. Enumerable.Repeat(0, MandervillousWeaponId.Count)]);
                    }
                    break;
                case 6: // 幻武
                    for (var i = 0; i < JobsOfSpecialWeapon[6]; i++)
                    {
                        weaponProcess.Add(PhantomWeaponJobIdList[i], [.. Enumerable.Repeat(0, PhantomWeaponId.Count)]);
                    }
                    break;
                case 7: // 天钢
                    for (var i = 0; i < JobsOfSpecialWeapon[7]; i++)
                    {
                        weaponProcess.Add(SkysteelWeaponJobIdList[i], [.. Enumerable.Repeat(0, SkysteelWeaponId.Count)]);
                    }
                    break;
                case 8: // 莫雯
                    for (var i = 0; i < JobsOfSpecialWeapon[8]; i++)
                    {
                        weaponProcess.Add(SplendorousWeaponJobIdList[i], [.. Enumerable.Repeat(0, SplendorousWeaponId.Count)]);
                    }
                    break;
                case 9: // 绝境战
                    for (var i = 0; i < JobsOfSpecialWeapon[9]; i++)
                    {
                        weaponProcess.Add(UltimateWeaponJobIdList[i], [.. Enumerable.Repeat(0, UltimateWeaponId.Count)]);
                    }
                    break;
            }
            
            return weaponProcess;
        }

        public void GetProcessData(
            int weaponIndex, List<List<uint>> weaponIdList, List<uint> jobIdList,
            ref Dictionary<uint, List<int>> weaponProcess)
        {
            for (var i = 0; i < JobsOfSpecialWeapon[weaponIndex]; i++) //Job Index
            {
                for (var j = 0; j < weaponIdList.Count; j++) //阶段
                {
                    var curJobId = jobIdList[i];
                    // 针对不同武器类型使用专用索引
                    var jobIndex = weaponIndex switch
                    {
                        1 or 6 => NewJobIndex[curJobId],                         // 古武 幻武使用NewJobIndex
                        7 or 8 => LifeJobIndex[curJobId],                        // 天钢和莫雯使用LifeJobIndex
                        9 => j < 5 ? JobIndex[curJobId] : NewJobIndex[curJobId], // 绝境战特殊处理
                        _ => JobIndex[curJobId]                                  // 其他武器使用JobIndex
                    };
                    // 添加边界检查
                    if (jobIndex < weaponIdList[j].Count)
                    {
                        var curWeaponId = weaponIdList[j][jobIndex];
                        var curWeaponCount = getItemCountTotal(curWeaponId);
                        weaponProcess[curJobId][j] = curWeaponCount;
                    }
                    else
                    {
                        // 如果索引越界，则设置为0
                        weaponProcess[curJobId][j] = 0;
                    }
                }
            }
        }
    }
}
