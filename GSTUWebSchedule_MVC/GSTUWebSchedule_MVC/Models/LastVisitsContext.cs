using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class LastVisitsContext : DbContext
    {
        public DbSet<LastVisitsModel> DbTable { get; set; }
        public LastVisitsContext(DbContextOptions<LastVisitsContext> options) : base(options)
        {
            Database.EnsureCreated(); //Creating a new DB when using the first time
        }
    }
}
