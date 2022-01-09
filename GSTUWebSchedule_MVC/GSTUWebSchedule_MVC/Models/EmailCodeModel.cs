using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class EmailCodeModel
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Не указано имя пользователя или использованы недопустимые символы")]
        [MinLength(4)]
        [MaxLength(15)]
        [RegularExpression("^[a-zA-Z0-9._\\-@#]{4,15}$")]
        public string Username { get; set; }

        public string EmailConfirmationCode { get; set; }
        public DateTime ConfirmationDate { get; set; }

        public string EmailChangePasswordCode { get; set; }
        public DateTime ChangePasswordDate { get; set; }

        public string Error { get; set; }
    }
}
