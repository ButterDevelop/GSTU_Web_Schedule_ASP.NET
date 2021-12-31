using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class DbUsersContext : DbContext
    {
        public DbSet<DbUsersModel> DbUsers { get; set; }
        public DbUsersContext(DbContextOptions<DbUsersContext> options) : base(options)
        {
            Database.EnsureCreated(); //Creating a new DB when using the first time
        }
    }
}
