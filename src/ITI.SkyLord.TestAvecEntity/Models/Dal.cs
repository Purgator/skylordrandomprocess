using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.SkyLord.TestAvecEntity.Models
{
    public class Dal : IDisposable
    {
        
        ApplicationDbContext _bdd;

        public Dal()
        {
            _bdd = new ApplicationDbContext();
        }


        public List<Tchat> GetAllMessage()
        {
            return _bdd.Tchat.ToList<Tchat>();
        }

        public void AddMessage(string personne, string message)
        {
            _bdd.Tchat.Add(new Tchat { Message = message, Personne = personne });
            _bdd.SaveChanges();
        }


        public void Dispose()
        {
            _bdd.Dispose();
        }


    }
}
