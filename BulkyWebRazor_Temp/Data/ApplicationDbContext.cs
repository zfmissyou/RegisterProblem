﻿using BulkyWebRazor_Temp.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyWebRazor_Temp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 10, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 11, Name = "Scifi", DisplayOrder = 2 },
                new Category { Id = 12, Name = "History", DisplayOrder = 3 }
                );
        }
    }
}
