
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
    public class CompanyRepository : Repository<Company>, ICompany
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(Company entity)
        {
            _context.Update(entity);
        }
    }
}
