# SpecialWeaponProgressOverview

一个用于追踪FFXIV特殊武器进度的 Dalamud 插件，支持多种武器系列的进度显示和材料统计。

---
> **注意：本插件为非官方开发，请遵守游戏使用条款。**
---

## 功能特点

- 🎯 **支持 9 种武器系列追踪**
  - 古武 (Zodiac Weapons)
  - 魂武 (Anima Weapons)
  - 优武 (Eureka Weapons)
  - 义武 (Bozja Weapons)
  - 曼武 (Mandervillous Weapons)
  - 幻武 (Phantom Weapons)
  - 天钢工具 (Skysteel Tools)
  - 莫雯工具 (Splendorous Tools)
  - 绝本武器 (Ultimate Weapons)
- 📊 **实时进度显示**
  - 自动获取当前武器阶段和进度
- 📦 **自动背包扫描**
  - 基于 Allagan Tools IPC 实现，支持角色背包、陆行鸟鞍囊及最多 10 个雇员的所有栏位
- 💡 **材料需求统计**
  - 精确计算各阶段材料需求（优武、义武、曼武、幻武）
- 🔄 **IPC 数据缓存**
  - 一键刷新并缓存所有武器和材料数据，避免重复 IPC 调用
- 🎨 **直观的 UI 界面**
  - 标签页导航，总览页显示全系列进度条与绝本圆形进度
  - 物品图标内嵌显示，支持点击复制物品名称

## 前置条件

- FFXIV客户端
- Dalamud  
- Allagan Tool

## 使用说明

1. 安装Dalamud和Allagan Tool插件。
2. 添加仓库链接  
   ```https://raw.githubusercontent.com/yimo0908/DalamudPlugin/refs/heads/main/pluginmaster.json```.
3. 搜索并安装SpecialWeaponProgressOverview插件。
4. 使用 `/pover` 命令打开主界面  

## 开发指南

### 项目结构

SpecialWeaponProgressOverview/  
├── Base/                                 # 核心功能实现  
│   ├── PluginService.cs                  # 插件服务管理  
│   └── Process.cs                        # 进度数据处理逻辑  
├── Data/                                 # 数据处理和计算  
│   ├── Compute.cs                        # 材料需求计算引擎  
│   ├── DataBase.cs                       # 武器 ID、职业列表、材料配方  
│   └── Inventory.cs                      # Allagan Tools IPC 调用与缓存  
├── Drawer/                               # UI 绘制组件  
│   ├── DrawMethod.cs                     # 通用绘制方法（纹理渲染、剪贴板）  
│   ├── WeaponSeriesDrawer.cs             # 统一基类，处理表格布局与材料分发  
│   ├── AnimaDrawer.cs                    # 魂武界面  
│   ├── BozjaDrawer.cs                    # 义武界面  
│   ├── EurekaDrawer.cs                   # 优武界面  
│   ├── MandervillousDrawer.cs            # 曼武界面  
│   ├── PhantomDrawer.cs                  # 幻武界面  
│   ├── SkysteelDrawer.cs                 # 天钢工具界面  
│   ├── SplendorousDrawer.cs              # 莫雯工具界面  
│   ├── UltimateDrawer.cs                 # 绝本武器界面  
│   └── ZodiacDrawer.cs                   # 古武界面  
├── Models/                               # 数据模型  
│   ├── WeaponSeries.cs                   # 武器系列枚举  
│   ├── WeaponSeriesInfo.cs               # 系列元数据（阶段、名称、职业索引）  
│   └── ItemInfo.cs                       # 物品 ID + 数量 DTO  
├── Plugin.cs                             # 插件主入口（/pover）  
├── MainWindow.cs                         # 主窗口（标签页导航、总览、进度绘制）  
└── Configuration.cs                      # 可序列化配置

### 更新指南

当游戏版本更新时，需要检查以下内容：

1. **武器数据更新**
    - 更新 `Data/DataBase.cs` 中的武器 ID 列表、武器与职业对应关系
    - 更新 `Base/Process.cs` 中的进度获取逻辑
    - 检查新增职业是否需要添加到对应的武器系列

2. **材料配方更新**
    - 更新 `Data/DataBase.cs` 中的材料配方表（优武/义武/曼武/幻武）
    - 更新 `Data/Compute.cs` 中的计算方法，确保货币需求与版本同步

3. **UI 显示更新**
    - 更新 `Drawer/` 下的绘制类
    - 如有新增武器系列，在 `Models/WeaponSeries.cs` 枚举和 `Models/WeaponSeriesInfo.cs` 元数据中注册

## 贡献指南

欢迎提交Issue和Pull Request来帮助改进这个项目。

## 许可证 License

见 [LICENSE.md](LICENSE.md) 文件。
