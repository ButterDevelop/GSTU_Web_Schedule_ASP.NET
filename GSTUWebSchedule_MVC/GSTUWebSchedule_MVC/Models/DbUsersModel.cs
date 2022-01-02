using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class DbUsersModel
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Не указано имя пользователя")]
        [MinLength(4)]
        [MaxLength(15)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Salt { get; set; }

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

        public DateTime RegisterTime { get; set; }

        public string Approved { get; set; }

        public string Role { get; set; }
    }
}
