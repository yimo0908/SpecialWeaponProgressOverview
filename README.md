# SpecialWeaponProgressOverview

一个用于追踪FFXIV特殊武器进度的插件，支持多种武器系列的进度显示和材料统计。

---
> **注意：本插件为非官方开发，请遵守游戏使用条款。**
---

## 功能特点

- 🎯 **支持多种武器系列追踪**
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
  - 基于Allagan Tool的IPC实现
- 💡 **材料需求统计**
  - 精确计算下一阶段所需材料（仅支持义武、曼武、幻武）
- 🎨 **直观的UI界面**
  - 清晰的进度条和材料列表展示

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
│   └── Process.cs                        # 进度处理  
├── Data/                                 # 数据处理和计算  
│   ├── Compute.cs                        # 材料需求计算  
│   ├── DataBase.cs                       # 武器数据存储  
│   └── Inventory.cs                      # 背包数据管理  
├── Drawer/                               # UI绘制组件  
│   ├── DrawMethod.cs                     # 通用绘制方法  
│   ├── AnimaDrawer.cs                    # 魂武界面  
│   ├── BozjaDrawer.cs                    # 义武界面  
│   ├── EurekaDrawer.cs                   # 优武界面  
│   ├── MandervillousDrawer.cs            # 曼武界面  
│   ├── PhantomDrawer.cs                  # 幻武界面  
│   ├── SkysteelDrawer.cs                 # 天钢工具界面  
│   ├── SplendorousDrawer.cs              # 莫雯工具界面  
│   ├── UltimateDrawer.cs                 # 绝本武器界面  
│   └── ZodiacDrawer.cs                   # 古武界面  
├── Plugin.cs                             # 插件主入口  
├── MainWindow.cs                         # 主窗口实现  
└── Configuration.cs                      # 配置文件

### 更新指南

当游戏版本更新时，需要检查以下内容：

1. **武器数据更新**
    - 更新 `Data/DataBase.cs` 中的武器ID列表、武器与职业对应关系
    - 更新 `Base/Process.cs` 中的进度获取逻辑
    - 检查新增职业是否需要添加到对应的武器系列

2. **更新 `Data/Compute.cs` 中的计算方法** (可选)
    - 确保货币需求计算逻辑与游戏版本同步

3. **UI显示更新**
    - 更新 `Drawer` 目录下的绘制方法
    - 检查 `InventoryWindow.cs` 的调用

## 贡献指南

欢迎提交Issue和Pull Request来帮助改进这个项目。

## 许可证 License

见 [LICENSE.md](LICENSE.md) 文件。
