using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.SkyLord.TestAvecEntity.Models
{
    public class Tchat
    {
        public int ID { get; set; }
        public string Personne { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }
}
