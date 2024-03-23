using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Models.DbContext
{
    public class KitchenHeroDbContextFactory : IDesignTimeDbContextFactory<KitchenHeroDbContext>
    {
        public KitchenHeroDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<KitchenHeroDbContext>();
            optionsBuilder.UseSqlServer("connection_string"); // Replace with your connection string
            return new KitchenHeroDbContext(optionsBuilder.Options);
        }
    }
}
