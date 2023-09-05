using GameEngine.Catalog.Service.Dtos;
using GameEngine.Catalog.Service.Entities;

namespace GameEngine.Catalog.Service;

    public static class Extensions
    {
        public static ItemDto AsDto(this Item item)
        {
            return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
        }
    }
 