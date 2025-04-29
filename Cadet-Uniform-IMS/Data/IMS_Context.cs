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
    public class IMS_Context: IdentityDbContext<IMS_User>
    {
        public IMS_Context(DbContextOptions<IMS_Context> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IMS_User>().Ignore(e => e.Name);
            modelBuilder.Entity<IMS_Staff>().ToTable("Staff");
            modelBuilder.Entity<IMS_Cadet>().ToTable("Cadet");
            modelBuilder.Entity<StockSize>().HasKey(t => new { t.StockID, t.AttributeID });
            modelBuilder.Entity<BasketStock>().HasKey(t => new { t.StockID, t.UID });
            modelBuilder.Entity<PendingOrderItem>().HasKey(t => new { t.PendingOrderID, t.StockID });
            modelBuilder.Entity<OrderItem>().HasKey(t => new { t.OrderID, t.StockID });
        }

        public DbSet<IMS_User> User { get; set; }
        public DbSet<IMS_Cadet> Cadet { get; set; }
        public DbSet<IMS_Staff> Staff { get; set; }
        public DbSet<Stock> Stock { get; set; } = default!;
        public DbSet<StockSize> StockSize { get; set; } = default!;
        public DbSet<SizeAttribute> SizeAttribute { get; set; } = default!;
        public DbSet<Uniform> Uniform { get; set; } = default!;
        public DbSet<UniformType> UniformType { get; set; } = default!;
        public DbSet<BasketStock> BasketStock { get; set; } = default!;
        public DbSet<OrderHistory> OrderHistory { get; set; } = default!;
        public DbSet<PendingOrder> PendingOrder { get; set; } = default!;
        public DbSet<PendingOrderItem> PendingOrderItems { get; set; } = default!;
        public DbSet<OrderItem> OrderItems { get; set; } = default!;


    }
}
