using Example.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Data
{
    public class OrdersContext: DbContext
    {
        public OrdersContext(DbContextOptions<OrdersContext> options) : base(options)
        {
        }

        public DbSet<CommandDto> Command { get; set; }

        public DbSet<ProductDto> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductDto>().ToTable("Products").HasKey(s => s.ProductId);
            modelBuilder.Entity<CommandDto>().ToTable("Command").HasKey(s => s.CommandId);
        }
    }
}
