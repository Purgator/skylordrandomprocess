using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;

namespace ITI.SkyLord.TestAvecEntity.Models
{
    public class BddContext : DbContext
    {
        public DbSet<Tchat> Tchat { get; set; }

    }
}
