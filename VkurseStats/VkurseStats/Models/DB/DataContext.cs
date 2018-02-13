using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace VkurseStats.Models.DB
{
    public class DataContext : DbContext
    {
        public DataContext() : base("name=DefaultConnectionString")
        {
        }

        public DbSet<VkurseRate> VkurseRates { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("vkurse");
        }
    }
}