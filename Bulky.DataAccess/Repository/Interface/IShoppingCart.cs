using Bulky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.Interface
{
    public interface IShoppingCart : IRepository<ShoppingCart>
    {
        void Update(ShoppingCartRepository shoppingcart);
    }
}
