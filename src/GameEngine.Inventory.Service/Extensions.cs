using GameEngine.Inventory.Service.Dtos;
using GameEngine.Inventory.Service.Entities;

namespace GameEngine.Inventory.Service
{
    public static class Extensions
    {
        public static InventoryItemDto AsDto(this InventoryItem item, string name, string description )
        {
            return new InventoryItemDto(item.CatalogItemId, name, description, item.Quantity, item.AcquiredDate);
        }
    }
}
