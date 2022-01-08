using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Не указано имя пользователя или использованы недопустимые символы")]
        [MaxLength(15)]
        [RegularExpression("^[a-zA-Z0-9._\\-@#]{4,15}$")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Не указан Email")]
        [DataType(DataType.EmailAddress)]
        [MaxLength(40)]
        public string Email { get; set; }

        public string Error { get; set; }

        public int Case { get; set; }

        public string NewPassword { get; set; }
        public string NewPasswordConfirm { get; set; }

        public string Code { get; set; }
    }
}
