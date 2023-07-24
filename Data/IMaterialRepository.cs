public interface IMaterialRepository : IDisposable
{
    Task<List<Material>> GetMaterialsAsync();
    Task<List<Material>> GetMaterialsAsync(string name);
    Task<Material> GetMaterialAsync(int materialId);
    Task InsertMaterialAync(Material material);
    Task UpdateMaterialAsync(Material material);
    Task DeleteMaterialAsync(int materialId);
    Task SaveAsync();

}