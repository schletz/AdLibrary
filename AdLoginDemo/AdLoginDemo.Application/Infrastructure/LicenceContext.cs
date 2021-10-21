using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdLoginDemo.Application.Infrastructure
{
    public class LicenceContext : DbContext
    {
        public LicenceContext(DbContextOptions opt) : base(opt)
        { }


    }
}
