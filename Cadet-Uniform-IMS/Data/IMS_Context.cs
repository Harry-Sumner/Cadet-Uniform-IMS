using Microsoft.EntityFrameworkCore;


namespace Cadet_Uniform_IMS.Data
{
    public class IMS_Context: DbContext
    {
        public IMS_Context(DbContextOptions<IMS_Context> options) : base(options)
        {
        }

        public DbSet<Stock> Stock { get; set; } = default!;
        public DbSet<StockSize> StockSize { get; set; } = default!;
        public DbSet<SizeAttribute> SizeAttribute { get; set; } = default!;
        public DbSet<Uniform> Uniform { get; set; } = default!;
        public DbSet<UniformType> UniformType { get; set; } = default!;
    }
}
