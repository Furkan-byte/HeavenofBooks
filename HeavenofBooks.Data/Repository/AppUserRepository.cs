using HeavenofBooks.DataAccess.Data;
using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenofBooks.DataAccess.Repository
{
    public class AppUserRepository : Repository<AppUser> , IAppUserRepository
    {
        private readonly ApplicationDbContext _context;
        public AppUserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        


    }
}
