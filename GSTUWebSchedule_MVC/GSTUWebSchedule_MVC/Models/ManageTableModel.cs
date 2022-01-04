using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class ManageTableModel
    {
        [Key]
        public int id { get; set; }

        [Range(1, 2)]
        public int Case { get; set; }

        public string Error { get; set; }
        public string Error2 { get; set; }

        [Range(0, Subjects.size - 1)]
        public int Subject { get; set; }

        [MinLength(4)]
        public string Group { get; set; }

        [MinLength(9)]
        [MaxLength(9)]
        [RegularExpression("\\d{4}\\/\\d{4}")]
        public string Year { get; set; }

        [Range(1, 3)]
        public int Semester { get; set; }

        [Range(1, 540)]
        public int NumberOfLabs { get; set; }

        [MinLength(1)]
        public string Students { get; set; }

        public IEnumerable<GSTUWebSchedule_MVC.Models.DbTableModel> DbTable;
    }
}
