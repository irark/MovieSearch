using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace MovieSearch
{
    public partial class Ganre
    {
        public Ganre()
        {
            FilmGanreRelationships = new HashSet<FilmGanreRelationship>();
        }

        public int Id { get; set; }
        [Required(ErrorMessage = "Поле не повинне бути порожнім")]
        [Display(Name = "Назва")]
        public string Name { get; set; }

        public virtual ICollection<FilmGanreRelationship> FilmGanreRelationships { get; set; }
    }
}
