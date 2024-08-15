using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Model.ViewModel
{
    public class ShoppingCartVM
    {
        public double OrderTotal { get; set; }
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
    }
}
