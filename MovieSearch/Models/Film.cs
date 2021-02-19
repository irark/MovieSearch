using System;
using System.Collections.Generic;

#nullable disable

namespace MovieSearch
{
    public partial class Film
    {
        public Film()
        {
            FilmActorRelationships = new HashSet<FilmActorRelationship>();
            FilmGanreRelationships = new HashSet<FilmGanreRelationship>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public int Year { get; set; }
        public string Info { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<FilmActorRelationship> FilmActorRelationships { get; set; }
        public virtual ICollection<FilmGanreRelationship> FilmGanreRelationships { get; set; }
    }
}
