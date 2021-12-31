using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class SaveTableModel
    {
        public string dataLabs { get; set; }
        public string dataReports { get; set; }
        public string dataDefences { get; set; }

        [Range(0, Subjects.size - 1)]
        public int dataSubject { get; set; }
        [MinLength(4)]
        public string dataGroup { get; set; }
    }
}
