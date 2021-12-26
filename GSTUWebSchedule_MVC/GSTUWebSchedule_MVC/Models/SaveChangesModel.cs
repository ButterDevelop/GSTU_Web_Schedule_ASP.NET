using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class SaveChangesModel
    {
        [Range(1, Subjects.size - 1)]
        public int Subject { get; set; }

        public string dataLabs;
        public string dataReports;
        public string dataDefences;
        public int dataSubject;
        public string dataGroup;
    }
}
