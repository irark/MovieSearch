using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace MovieSearch
{
    public partial class Category
    {
        public Category()
        {
            Films = new HashSet<Film>();
        }

        public int Id { get; set; }
        [Required(ErrorMessage = "Поле не повинне бути порожнім")]
        [Display(Name = "Категорія")]
        public string Name { get; set; }

        public virtual ICollection<Film> Films { get; set; }
    }
}
