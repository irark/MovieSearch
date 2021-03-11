using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace MovieSearch
{
    public partial class Actor
    {
        public Actor()
        {
            FilmActorRelationships = new HashSet<FilmActorRelationship>();
        }

        public int Id { get; set; }
        [Required(ErrorMessage = "Поле не повинне бути порожнім")]
        [Display(Name = "Назва")]
        public string Name { get; set; }

        public virtual ICollection<FilmActorRelationship> FilmActorRelationships { get; set; }
    }
}
