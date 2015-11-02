using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Dnx.Runtime.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Dnx.Runtime;

namespace ITI.SkyLord.TestAvecEntity.Models
{
    /*
    public class BddContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var appEnv = CallContextServiceLocator.Locator.ServiceProvider
                            .GetRequiredService<IApplicationEnvironment>();
            optionsBuilder.UseNpgsql($"Data Source={ appEnv.ApplicationBasePath }/blog.db");
        }

        public DbSet<Tchat> Tchat { get; set; }

    }
    */
}
