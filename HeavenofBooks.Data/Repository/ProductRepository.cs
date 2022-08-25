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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var updateAnObject = _context.Products.FirstOrDefault(u => u.Id == product.Id);
            if (updateAnObject !=null)
            {
                updateAnObject.ISBN = product.ISBN;
                updateAnObject.ListPrice = product.ListPrice;
                updateAnObject.Price = product.Price;
                updateAnObject.Price100 = product.Price100;
                updateAnObject.Price50 = product.Price50;
                updateAnObject.Description = product.Description;
                updateAnObject.CategoryId = product.CategoryId;
                updateAnObject.Author = product.Author;
                updateAnObject.CoverTypeId = product.CoverTypeId;
                if (product.ImageUrl != null)
                {
                    updateAnObject.ImageUrl = product.ImageUrl;
                }
            }
        }
    }
}
