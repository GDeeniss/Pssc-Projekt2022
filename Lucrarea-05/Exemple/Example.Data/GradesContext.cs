﻿using Example.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Data
{
    public class GradesContext: DbContext
    {
        public GradesContext(DbContextOptions<GradesContext> options) : base(options)
        {
        }

        public DbSet<GradeDto> Grades { get; set; }

        public DbSet<StudentDto> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudentDto>().ToTable("Product").HasKey(s => s.ProductId);
            modelBuilder.Entity<GradeDto>().ToTable("Command").HasKey(s => s.CommandId);
        }
    }
}
