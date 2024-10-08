﻿using Bulky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.Interface
{
    public interface IUnitOfWork
    {
        ICompany CompanyRepo { get; }
        ICategory CategoryRepo { get; }
        IProduct ProductRepo { get; }
        IProductImage ProductImageRepo { get; }
        IApplicationUser UserRepo { get; }
        IShoppingCart ShoppingCartRepo { get; }
        IOrderheader OrderheaderRepo { get; }
        IOrderdetails OrderdetailsRepo { get; }
        void save();
    }
}
