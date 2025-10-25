## 项目介绍

SpecialWeaponProgressOverview 是一个 FFXIV (最终幻想14) 插件，用于追踪和显示玩家在游戏中各种特殊武器的获取进度。该插件支持多种武器系列，包括但不限于：

- 古武 (Zodiac)
- 魂武 (Anima)
- 优武 (Eureka)
- 义武 (Bozja)
- 曼武 (Mandervillous)
- 幻武 (Phantom)
- 天钢工具 (Skysteel)
- 莫雯工具 (Splendorous)
- 绝本武器 (Ultimate)

插件会自动使用Allagan Tool插件扫描玩家的背包，计算每种武器的获取进度，并提供所需材料的统计信息。

## 更新内容

### 每次更新需要检查的内容

1. **更新新特殊武器相关内容**
    - 更新 `Data/DataBase.cs` 中的武器ID列表、武器与职业对应关系
    - 更新 `Base/Process.cs` 中的进度获取逻辑
    - 检查新增职业是否需要添加到对应的武器系列
   
2. **更新 `Data/Compute.cs` 中的计算方法** (可选)
    - 确保货币需求计算逻辑与游戏版本同步
   
3. **UI显示更新**
    - 检查 `Drawer` 目录下的绘制方法
    - 检查 `InventoryWindow.cs` 的调用
