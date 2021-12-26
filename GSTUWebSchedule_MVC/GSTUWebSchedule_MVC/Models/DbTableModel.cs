using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class DbTableModel
    {
        [Key]
        public int id { get; set; }

        public string Username { get; set; }

        [Range(1, Subjects.size - 1)]
        public int Subject { get; set; }

        [MinLength(4)]
        public string Group { get; set; }

        [MinLength(1)]
        public string Students { get; set; }

        [Range(1, 540)]
        public int NumberOfLabs { get; set; }
        public string Labs { get; set; }
        public string Reports { get; set; }
        public string Defences { get; set; }
    }
}
