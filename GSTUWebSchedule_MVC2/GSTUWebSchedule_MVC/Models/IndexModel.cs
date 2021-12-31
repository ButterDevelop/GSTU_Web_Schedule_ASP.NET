using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class Subjects
    {
        public const int size = 5;
        public static string[] constArray = new string[size] { "ОАиП", "ОВП", "МА", "МЛ", "АГиЛА" };
    }
    public class IndexModel
    {
        [Key]
        public int id { get; set; }

        public bool Case = false;

        public IEnumerable<GSTUWebSchedule_MVC.Models.DbTableModel> DbTable;

        /*public string dataLabs;
        public string dataReports;
        public string dataDefences;
        public int dataSubject;
        public string dataGroup;*/
    }
}
