﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.Repository
{
    public interface IDbContext : IDisposable
    {
        IDbConnectionContext Open();
        IDbConnectionContext BeginTransaction();
    }
}
