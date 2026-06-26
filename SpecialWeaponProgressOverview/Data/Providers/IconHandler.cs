using Dalamud.Interface.Textures;
using SpecialWeaponProgressOverview.Base;

namespace SpecialWeaponProgressOverview.Data.Providers;

/// <summary>
/// 图标纹理加载器：缓存 <see cref="ISharedImmediateTexture"/> 引用，
/// 避免每帧重复调用 <see cref="ITextureProvider.GetFromGameIcon"/>。
/// 镜像 Collections 项目的 IconHandler 模式，并增加纹理引用缓存。
/// </summary>
public sealed class IconHandler
{
    private readonly uint _iconId;
    private ISharedImmediateTexture? _texture;

    public IconHandler(uint iconId)
    {
        _iconId = iconId;
    }

    /// <summary>
    /// 获取缓存的共享纹理引用。首次调用时从游戏数据加载，
    /// 后续直接返回缓存的 <see cref="ISharedImmediateTexture"/>。
    /// </summary>
    /// <remarks>
    /// Dalamud 文档指出 ISharedImmediateTexture 可安全缓存，
    /// 虽然内部已有字典级缓存，但跳过每帧的 GetFromGameIcon 调用
    /// （含 GameIconLookup 哈希 + 两层 ConcurrentDictionary 查找）
    /// 仍可减少表格密集绘制时的开销。
    /// </remarks>
    public ISharedImmediateTexture GetIcon()
    {
        return _texture ??= PluginService.TextureProvider
            .GetFromGameIcon(new GameIconLookup(_iconId));
    }

    /// <summary>静态快捷方法：按 iconId 获取共享纹理（不缓存，适用于一次性使用场景）。</summary>
    public static ISharedImmediateTexture GetIcon(uint iconId, bool hq = false)
    {
        return PluginService.TextureProvider
            .GetFromGameIcon(new GameIconLookup(iconId, hq));
    }
}
