using System;
using System.Collections.Generic;

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
        public string Name { get; set; }

        public virtual ICollection<FilmGanreRelationship> FilmGanreRelationships { get; set; }
    }
}
