using System;
using System.Linq;
using System.Collections.Generic;
using SpecialWeaponProgressOverview.Data;

namespace SpecialWeaponProgressOverview.Base
{
    public class Process
    {
        private static readonly Dictionary<int, List<List<uint>>> WeaponIdsMap = new()
        {
            { 1, DataBase.ZodiacWeaponId },
            { 2, DataBase.AnimaWeaponId },
            { 3, DataBase.EurekaWeaponId },
            { 4, DataBase.BozjaWeaponId },
            { 5, DataBase.MandervillousWeaponId },
            { 6, DataBase.PhantomWeaponId },
            { 7, DataBase.SkysteelWeaponId },
            { 8, DataBase.SplendorousWeaponId },
            { 9, DataBase.UltimateWeaponId }
        };

        private static readonly Dictionary<int, List<uint>> JobIdListsMap = new()
        {
            { 1, DataBase.ZodiacWeaponJobIdList },
            { 2, DataBase.AnimaWeaponJobIdList },
            { 3, DataBase.EurekaWeaponJobIdList },
            { 4, DataBase.BozjaWeaponJobIdList },
            { 5, DataBase.MandervillousWeaponJobIdList },
            { 6, DataBase.PhantomWeaponJobIdList },
            { 7, DataBase.SkysteelWeaponJobIdList },
            { 8, DataBase.SplendorousWeaponJobIdList },
            { 9, DataBase.UltimateWeaponJobIdList }
        };

        private static readonly Dictionary<uint, int> JobIndex = DataBase.JobIndex;
        private static readonly Dictionary<uint, int> NewJobIndex = DataBase.NewJobIndex;
        private static readonly Dictionary<uint, int> LifeJobIndex = DataBase.LifeJobIndex;

        public static readonly Dictionary<int, int> JobsOfSpecialWeapon = new()
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

        private readonly MainWindow.ItemCountDelegate getItemCountTotal;

        public Process(MainWindow.ItemCountDelegate getItemCountDelegate)
        {
            getItemCountTotal = getItemCountDelegate;
        }

        public static Dictionary<uint, List<int>> InitChart(int weaponIndex)
        {
            var weaponProcess = new Dictionary<uint, List<int>>();
            var jobIdList = JobIdListsMap[weaponIndex];
            var weaponIdList = WeaponIdsMap[weaponIndex];
            var jobCount = JobsOfSpecialWeapon[weaponIndex];

            for (var i = 0; i < jobCount; i++)
            {
                weaponProcess.Add(jobIdList[i], [..Enumerable.Repeat(0, weaponIdList.Count)]);
            }

            return weaponProcess;
        }


        public void GetProcessData(
            int weaponIndex, List<List<uint>> weaponIdList, List<uint> jobIdList,
            ref Dictionary<uint, List<int>> weaponProcess)
        {
            var jobCount = JobsOfSpecialWeapon[weaponIndex];

            for (var i = 0; i < jobCount; i++) //Job Index
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
                        // 如果索引越界，则输出日志
                        Console.WriteLine($"索引越界！当前武器索引为：{jobIndex}，当前武器数量为：{weaponIdList[j].Count}");
                    }
                }
            }
        }
    }
}
