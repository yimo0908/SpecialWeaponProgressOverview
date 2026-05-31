namespace SpecialWeaponProgressOverview.Models;

/// <summary>物品信息，用于雇员背包缓存。</summary>
public sealed class ItemInfo(uint itemId, uint quantity)
{
    public uint ItemId   { get; set; } = itemId;
    public uint Quantity { get; set; } = quantity;
}
