using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class DbEmailCodesContext : DbContext
    {
        public DbSet<EmailCodeModel> DbCodes { get; set; }
        public DbEmailCodesContext(DbContextOptions<DbEmailCodesContext> options) : base(options)
        {
            Database.EnsureCreated(); //Creating a new DB when using the first time
        }
    }
}
