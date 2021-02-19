using System;
using System.Collections.Generic;

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
        public string Name { get; set; }

        public virtual ICollection<FilmActorRelationship> FilmActorRelationships { get; set; }
    }
}
