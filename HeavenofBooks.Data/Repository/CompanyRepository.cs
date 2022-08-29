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
    public class CompanyRepository : Repository<AppCompany>, ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(AppCompany company)
        {
            _context.Companies.Update(company);
        }
    }
}
