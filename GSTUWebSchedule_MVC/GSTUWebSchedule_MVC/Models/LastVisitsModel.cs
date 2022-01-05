using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class LastVisitsModel
    {
        [Key]
        public int id { get; set; }

        public string Username { get; set; }

        public DateTime Date { get; set; }

        public string OS { get; set; }

        public string IP { get; set; }

        public string Browser { get; set; }

        public string FullUserAgent { get; set; }

        public bool isMobile { get; set; }
    }
}
