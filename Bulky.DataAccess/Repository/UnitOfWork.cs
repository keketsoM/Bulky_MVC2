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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            CategoryRepo = new CategoryRepository(_context);
            ProductRepo = new ProductRepository(_context);
            CompanyRepo = new CompanyRepository(_context);
            ShoppingCartRepo = new ShoppingCartRepository(_context);
            UserRepo = new ApplicationUserRepository(_context);
            OrderheaderRepo = new OrderheaderRepository(_context);
            OrderdetailsRepo = new OrderdetailRepository(_context);
            ProductImageRepo = new ProductImageRepository(_context);
        }

        public ICategory CategoryRepo
        {
            get;
            private set;
        }
        public IProductImage ProductImageRepo
        {
            get;
            private set;
        }
        public IProduct ProductRepo
        {
            get;
            private set;
        }

        public ICompany CompanyRepo
        {
            get;
            private set;
        }

        public IShoppingCart ShoppingCartRepo
        {
            get;
            private set;
        }

        public IApplicationUser UserRepo
        {
            get;
            private set;
        }

        public IOrderheader OrderheaderRepo
        {
            get;
            private set;
        }

        public IOrderdetails OrderdetailsRepo
        {
            get;
            private set;
        }

        public void save()
        {
            _context.SaveChanges();
        }
    }
}
