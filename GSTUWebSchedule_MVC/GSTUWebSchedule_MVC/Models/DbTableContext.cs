﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class DbTableContext : DbContext
    {
        public DbSet<DbTableModel> DbTable { get; set; }
        public DbTableContext(DbContextOptions<DbTableContext> options) : base(options)
        {
            Database.EnsureCreated(); //Creating a new DB when using the first time
        }
    }
}
