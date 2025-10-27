using System;
using System.Numerics;
using KamiToolKit.Nodes;
using KamiToolKit.Classes;
using Dalamud.Interface.Text;
using Dalamud.Interface;

namespace SpecialWeaponProgressOverview.UiHelpers
{
    // 一组小型工厂/适配方法，封装常用节点的创建与绑定。
    // Drawer 重构时会依赖这些工具以保持风格一致、减少重复代码。
    public static class KamiUiHelpers
    {
        // 创建一个居左的文本节点（可配置字体/颜色将来扩充）
        public static TextNode CreateText(string text, AlignmentType align = AlignmentType.Left)
        {
            var node = new TextNode {
                String = text,
                AlignmentType = align,
                IsVisible = true
            };
            return node;
        }

        // 创建一个带文字的按钮（可绑定点击回调）
        public static TextButtonNode CreateButton(string label, Action? onClick = null)
        {
            var btn = new TextButtonNode {
                String = label,
                IsVisible = true,
            };

            // 简单事件绑定：ButtonBase 在 KamiToolKit 中通常提供事件回调点
            // 这里演示性的绑定方式，实际绑定将在 Drawer 里按具体 API 调整
            if (onClick != null) {
                btn.InitializeComponentEvents(); // ensure events ready
                // btn.Clicked += (s, e) => onClick.Invoke(); // 示例：根据实际 event 名称绑定
            }

            return btn;
        }

        // 创建一个图标按钮（用游戏内图标）
        public static IconButtonNode CreateIconButton(uint iconId, Action? onClick = null)
        {
            var iconBtn = new IconButtonNode {
                IconId = iconId,
                IsVisible = true,
                // 默认大小可由调用方调整：iconBtn.Size = new Vector2(32,32);
            };

            if (onClick != null) {
                iconBtn.InitializeComponentEvents();
                // iconBtn.Clicked += (s, e) => onClick.Invoke(); // 根据实际事件签名调整
            }

            return iconBtn;
        }

        // 创建一个用于显示数量的计数节点（封装格式化）
        public static TextNode CreateCountNode(int count)
        {
            var node = new TextNode {
                String = count.ToString(),
                AlignmentType = AlignmentType.Right,
                IsVisible = true
            };
            return node;
        }

        // 将字符串快速转换为 SeString（如果需要 SeString 支持）
        public static Dalamud.Game.Text.SeStringHandling.SeString ToSeString(string s)
        {
            return new SeString(new[]{ new Dalamud.Game.Text.SeStringHandling.Payloads.TextPayload(s) });
        }
    }
}