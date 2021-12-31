using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class DeleteTableModel
    {
        [Range(0, Subjects.size - 1)]
        public int Subject { get; set; }

        [MinLength(4)]
        public string Group { get; set; }

        [Range(0, 1)]
        public int Confirm1 { get; set; }
        [Range(0, 1)]
        public int Confirm2 { get; set; }
    }
}
