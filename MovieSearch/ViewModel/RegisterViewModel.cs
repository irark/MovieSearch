using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MovieSearch.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Поле не повинне бути порожнім")]
        [Display(Name = "Ім'я")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле не повинне бути порожнім")]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Поле не повинне бути порожнім")]
        [Display(Name = "Рік народження")]
        public int Year { get; set; }
        [Required(ErrorMessage = "Поле не повинне бути порожнім")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Поле не повинне бути порожнім")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        [Display(Name = "Підтвердження паролю")]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }
    }
}
