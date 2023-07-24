public class MaterialRepository : IMaterialRepository
{
    private readonly MaterialDb _context;
    public MaterialRepository(MaterialDb context)
    {
        _context = context;
    }
    public Task<List<Material>> GetMaterialsAsync() => _context.Materials.ToListAsync();

    public Task<List<Material>> GetMaterialsAsync(string name) =>
        _context.Materials.Where(material => material.Partnumber.Contains(name)).ToListAsync();

    public async Task<Material> GetMaterialAsync(int materialId) =>
        await _context.Materials.FindAsync(new object[] { materialId });

    public async Task InsertMaterialAync(Material material) => await _context.Materials.AddRangeAsync(material);

    public async Task UpdateMaterialAsync(Material material)
    {
        var materialFromDb = await _context.Materials.FindAsync(new object[] { material.Id });
        if (materialFromDb == null) return;
        materialFromDb.Partnumber = material.Partnumber;
        materialFromDb.ManufacturerCode = material.ManufacturerCode;
        materialFromDb.Price = material.Price;
        materialFromDb.UnitOfIssue = material.UnitOfIssue;
    }


    public async Task DeleteMaterialAsync(int materialId)
    {
        var materialFromDb = await _context.Materials.FindAsync(new object[] { materialId  });
        if (materialFromDb == null) return;
        _context.Materials.Remove(materialFromDb);
    }

    public async Task SaveAsync() => await _context.SaveChangesAsync();


    private bool _disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
        _disposed = true;
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}