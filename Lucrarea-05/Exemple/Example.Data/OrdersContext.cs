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

        public DbSet<OrderDto> Orders { get; set; }

        public DbSet<PersonDto> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonDto>().ToTable("Person").HasKey(s => s.PersonId);
            modelBuilder.Entity<OrderDto>().ToTable("Order").HasKey(s => s.OrderId);
        }
    }
}
