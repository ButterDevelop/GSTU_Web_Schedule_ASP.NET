using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class RegisterModel
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Не указано имя пользователя или использованы недопустимые символы")]
        [MinLength(4)]
        [MaxLength(15)]
        [RegularExpression("^[a-zA-Z0-9._\\-@#]{4,15}$")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        [MinLength(6)]
        [MaxLength(20)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Не указан Email")]
        [DataType(DataType.EmailAddress)]
        [MinLength(2)]
        [MaxLength(40)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Не указано имя")]
        [MinLength(2)]
        [MaxLength(15)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указана фамилия")]
        [MinLength(2)]
        [MaxLength(15)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Не указано отчество")]
        [MinLength(2)]
        [MaxLength(15)]
        public string Middlename { get; set; }
    }
}
