﻿using Bulky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.Interface
{
    public interface IApplicationUser : IRepository<ApplicationUser>
    {
        void updated(ApplicationUser applicationUser);
    }
}
