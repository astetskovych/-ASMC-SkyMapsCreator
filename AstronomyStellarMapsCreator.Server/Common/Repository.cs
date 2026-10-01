using Microsoft.EntityFrameworkCore.Metadata; // keep
using System.Reflection;

namespace AstronomyStellarMapsCreator.Server.Common
{
    public class Repository : IRepository
    {
        public readonly Astro280326Context _context;
        public Repository(Astro280326Context context)
        {
            _context = context;
        }
    
        public async Task<IEnumerable<StarDTO>> GetCatAsync(int? catId)
        {
            if(catId is null)
                return await GetDefaultCatAsync();

            return await GetCatByIdAsync(catId);
        }

        private async Task<IEnumerable<StarDTO>> GetCatByIdAsync(int? catId)
        {
            var cat = await _context.Cats
                    .Include(c => c.UniqueIdetifier)
                    .ThenInclude(u => u.Category)
                    .FirstOrDefaultAsync(c => c.Id == catId);       

            var identifier = cat.UniqueIdetifier;
            var category = identifier.Category?.Name;

            var catalogueEntityType = GetCatalogueEntities(category, identifier.No).FirstOrDefault();

            if (catalogueEntityType == null)
                throw new InvalidOperationException(
                    $"Catalogue '{category}/{identifier.No}' was not found.");

            var method = GetType().GetMethod(nameof(GetCatalogueData),
                                             BindingFlags.Instance | BindingFlags.NonPublic)!
                                            .MakeGenericMethod(catalogueEntityType.ClrType);

            var task = (Task<IEnumerable<StarDTO>>)method.Invoke(this, null)!;

            return await task;
        }

        public async Task<V50Catalog> GetCelestialObjectAsync(int id)   
        {
            return await _context.V50Catalogs.Where(s => s.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CatDTO>> GetCatsAsync()
        {
            return await _context.Cats.Select(c => new CatDTO
            {
                Id = c.Id,
                UniqueIdetifierId = c.UniqueIdetifierId,
                Name = c.Name,
                Records = c.Records
            }).ToListAsync();
        }

        public async Task<IEnumerable<CategoryDTO>> GetCategoriesAsync()
        {
            return await _context.Categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToListAsync();
        }

        public async Task<IEnumerable<CatDTO>> GetCatsFilteredAsync(string? name, int? categoryId, string? key)
        {
            var query = _context.Cats.AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }
            if (categoryId.HasValue)
            {
                query = query.Where(c => c.UniqueIdetifier.CategoryId == categoryId.Value);
            }
            if (!string.IsNullOrEmpty(key))
            {
                query = query.Where(c => c.Name.Contains(key) || c.Title.Contains(key));
            }
            return await query.Select(c => new CatDTO
            {
                Id = c.Id,
                UniqueIdetifierId = c.UniqueIdetifierId,
                Name = c.Name,
                Records = c.Records
            }).ToListAsync();
        }

        private async Task<IEnumerable<StarDTO>> GetDefaultCatAsync()
        {
            var stars = await _context.V50Catalogs.Where(s =>
                s.RahH != null &&
                s.RamMin != null &&
                s.RasS != null &&
                s.DedDeg != null &&
                s.DemArcmin != null &&
                s.DesArcsec != null &&
                s.De != null
                ).Select(s => new StarDTO
                {
                    Id = s.Id,
                    RA = (double)(s.RahH + s.RamMin / 60.0 + s.RasS / 3600.0),
                    Dec = s.De.Trim() == "-"
                    ? (-1) * (double)(s.DedDeg + s.DemArcmin / 60.0 + s.DesArcsec / 3600.0)
                    : (double)(s.DedDeg + s.DemArcmin / 60.0 + s.DesArcsec / 3600.0),
                    Mag = s.VmagMag ?? 100,
                }).ToListAsync();

            return stars;
        }

        private List<IEntityType> GetCatalogueEntities(string category, int? catalogueNumber)
        {
            var catalogue = $"{category}/{catalogueNumber}";

            return _context.Model
                .GetEntityTypes()
                .Where(e =>
                    e.GetSchema() == "Cats" &&
                    e.GetTableName() != null &&
                    e.GetTableName()!.StartsWith(
                        catalogue,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        //TODO: check if all catalogues has the same names of properties
        private async Task<IEnumerable<StarDTO>> GetCatalogueData<T>() where T : class
        {
            var entityType = _context.Model.FindEntityType(typeof(T));

            string? magProperty = null;
            double magFactor = 1.0;

            if (entityType?.FindProperty("VmagMag") != null)
            {
                magProperty = "VmagMag";
            }
            else if (entityType?.FindProperty("MagMag") != null)
            {
                magProperty = "MagMag";
            }
            else if (entityType?.FindProperty("Vmag01mag") != null)
            {
                magProperty = "Vmag01mag";
                magFactor = 0.1;
            }

            var stars = await _context.Set<T>()
                .Where(s =>
                    EF.Property<int?>(s, "RahH") != null &&
                    EF.Property<int?>(s, "RamMin") != null &&
                    EF.Property<double?>(s, "RasS") != null &&
                    EF.Property<int?>(s, "DedDeg") != null &&
                    EF.Property<int?>(s, "DemArcmin") != null &&
                    EF.Property<int?>(s, "DesArcsec") != null &&
                    EF.Property<string>(s, "De") != null)
                .Select(s => new StarDTO
                {
                    Id = EF.Property<int>(s, "Id"),

                    RA =
                        EF.Property<int?>(s, "RahH").Value +
                        EF.Property<int?>(s, "RamMin").Value / 60.0 +
                        EF.Property<double?>(s, "RasS").Value / 3600.0,

                    Dec =
                        EF.Property<string>(s, "De").Trim() == "-"
                            ? -1 * (
                                EF.Property<int?>(s, "DedDeg").Value +
                                EF.Property<int?>(s, "DemArcmin").Value / 60.0 +
                                EF.Property<int?>(s, "DesArcsec").Value / 3600.0)
                            : (
                                EF.Property<int?>(s, "DedDeg").Value +
                                EF.Property<int?>(s, "DemArcmin").Value / 60.0 +
                                EF.Property<int?>(s, "DesArcsec").Value / 3600.0),

                    Mag = EF.Property<double?>(s, magProperty) * magFactor ?? 100
                })
                .ToListAsync();

            return stars;
        }
    }
}
