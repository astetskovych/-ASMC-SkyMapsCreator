namespace AstronomyStellarMapsCreator.Server.Common
{
    public interface IRepository
    {
        Task<IEnumerable<StarDTO>> GetCatAsync(int? catId);
        Task<V50Catalog> GetCelestialObjectAsync(int celestialObjectId);
        Task<IEnumerable<CatDTO>> GetCatsAsync();
        Task<IEnumerable<CategoryDTO>> GetCategoriesAsync();
        Task<IEnumerable<CatDTO>> GetCatsFilteredAsync(string? name, int? categoryId, string? key);
    }
}
