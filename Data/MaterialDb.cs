public class MaterialDb : DbContext
{
    public MaterialDb(DbContextOptions<MaterialDb> options) : base(options) {}
    public DbSet<Material> Materials => Set<Material>();
}
