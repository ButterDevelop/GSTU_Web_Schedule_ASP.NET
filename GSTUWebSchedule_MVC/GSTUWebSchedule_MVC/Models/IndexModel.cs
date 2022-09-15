﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class Subjects
    {
        public const int size = 7;
        public static string[] constArray = new string[size] { "ОАиП", "ОВП", "МА", "МЛ", "АГиЛА", "Программирование", "Python" };
    }
    public class IndexModel
    {
        [Key]
        public int id { get; set; }

        public bool Case { get; set; }

        public IEnumerable<GSTUWebSchedule_MVC.Models.DbTableModel> DbTable;

        public string dataLabs { get; set; }
        public string dataReports { get; set; }
        public string dataDefences { get; set; }
        public int dataSubject { get; set; }
        public string dataGroup { get; set; }
    }
}
