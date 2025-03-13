using Microsoft.EntityFrameworkCore;


namespace Cadet_Uniform_IMS.Data
{
    public class IMS_Context(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Stock> Stock { get; set; } = default!;
    }
}
