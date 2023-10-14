namespace GameEngine.Inventory.Service.Dtos
{
    public record CatalogItemDto(Guid Id, string Name, string Description);
    public record GrantItemsDto(Guid UserId, Guid CatalogItemId, int Quantity);
    public record InventoryItemDto(Guid CatalogItemId, string Name, string Description, int Quantity, DateTimeOffset AcquiredDate);
}