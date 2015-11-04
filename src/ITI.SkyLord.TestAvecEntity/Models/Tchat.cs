using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.SkyLord.TestAvecEntity.Models
{
    public class Tchat
    {
        [Key]
        public int ID { get; set; }
        public string Personne { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public int age { get; set; }
    }
}
