﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Models
{
    public class ResetPasswordExactModel
    {
        [Required(ErrorMessage = "Не указан новый пароль")]
        [DataType(DataType.Password)]
        [MinLength(6)]
        [MaxLength(20)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Не указано подтверждение нового пароля")]
        [DataType(DataType.Password)]
        [MinLength(6)]
        [MaxLength(20)]
        public string NewPasswordConfirm { get; set; }

        [MinLength(4)]
        public string Code { get; set; }

        public string Error { get; set; }
    }
}
