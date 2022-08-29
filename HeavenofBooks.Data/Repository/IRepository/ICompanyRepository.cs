using HeavenofBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenofBooks.DataAccess.Repository.IRepository
{
    public interface ICompanyRepository : IRepository<AppCompany>
    {
        void Update(AppCompany company);
    }
}
