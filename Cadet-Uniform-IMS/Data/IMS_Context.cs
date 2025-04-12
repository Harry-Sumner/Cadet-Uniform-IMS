using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Cadet_Uniform_IMS.Data
{
    public class IMS_Context: IdentityDbContext<User>
    {
        public IMS_Context(DbContextOptions<IMS_Context> options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Cadet> Cadet { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Stock> Stock { get; set; } = default!;
        public DbSet<StockSize> StockSize { get; set; } = default!;
        public DbSet<SizeAttribute> SizeAttribute { get; set; } = default!;
        public DbSet<Uniform> Uniform { get; set; } = default!;
        public DbSet<UniformType> UniformType { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().Ignore(e => e.Name);
            modelBuilder.Entity<Staff>().ToTable("Staff");
            modelBuilder.Entity<Cadet>().ToTable("Cadet");
            modelBuilder.Entity<StockSize>().HasKey(t => new { t.StockID, t.AttributeID });
        }
    }
}
