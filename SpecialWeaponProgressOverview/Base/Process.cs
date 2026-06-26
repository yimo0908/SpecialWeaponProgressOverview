using System;
using System.Collections.Generic;
using System.Linq;
using SpecialWeaponProgressOverview.Models;

namespace SpecialWeaponProgressOverview.Base;

public class Process
{
    private readonly Func<uint, int> _getItemCountTotal;

    public Process(Func<uint, int> getItemCountDelegate)
    {
        _getItemCountTotal = getItemCountDelegate;
    }

    /// <summary>初始化武器进度字典，所有职业的所有阶段均置零。</summary>
    public static Dictionary<uint, List<int>> InitChart(WeaponSeriesInfo info)
    {
        var weaponProcess = new Dictionary<uint, List<int>>();

        for (var i = 0; i < info.JobIdList.Count; i++)
        {
            weaponProcess.Add(
                info.JobIdList[i],
                Enumerable.Repeat(0, info.WeaponIdStages.Count).ToList());
        }

        return weaponProcess;
    }

    /// <summary>获取武器进度数据，填充各职业各阶段的持有计数。</summary>
    public void GetProcessData(
        WeaponSeriesInfo                info,
        Dictionary<uint, List<int>>    weaponProcess)
    {
        var stageCount = info.WeaponIdStages.Count;

        for (var i = 0; i < info.JobIdList.Count; i++)
        {
            var curJobId = info.JobIdList[i];

            for (var j = 0; j < stageCount; j++)
            {
                var jobIndex = info.ResolveJobIndex(curJobId, j);

                if (jobIndex < 0 || jobIndex >= info.WeaponIdStages[j].Count)
                {
                    PluginService.PluginLog?.Verbose(
                        $"索引越界！系列={info.DisplayName}, 职业ID={curJobId}, 阶段={j}, jobIndex={jobIndex}");
                    continue;
                }

                var curWeaponId    = info.WeaponIdStages[j][jobIndex];
                var curWeaponCount = _getItemCountTotal(curWeaponId);
                weaponProcess[curJobId][j] = curWeaponCount;
            }
        }
    }
}
