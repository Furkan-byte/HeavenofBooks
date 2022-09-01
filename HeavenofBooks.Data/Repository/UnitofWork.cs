using HeavenofBooks.DataAccess.Data;
using HeavenofBooks.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenofBooks.DataAccess.Repository
{
    public class UnitofWork : IUnitofWork
    {
        private readonly ApplicationDbContext _context;
        public UnitofWork(ApplicationDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
            CoverType = new CoverTypeRepository(_context);
            Product = new ProductRepository(_context);
            Company = new CompanyRepository(_context);
            AppUser = new AppUserRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
        }
        public ICategoryRepository Category { get; private set; }
        public ICoverTypeRepository CoverType { get; private set; }
        public IProductRepository Product { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IAppUserRepository AppUser { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
