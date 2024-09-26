using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProduct
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;

        }

        public IEnumerable<Product> SearchProduct(string searchQuery)
        {
            return _context.products.Where(p => p.Title.Contains(searchQuery));
        }

        public void Update(Product product)
        {

            _context.products.Update(product);
        }
    }
}
