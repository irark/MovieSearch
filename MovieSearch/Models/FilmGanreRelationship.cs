using System;
using System.Collections.Generic;

#nullable disable

namespace MovieSearch
{
    public partial class FilmGanreRelationship
    {
        public int Id { get; set; }
        public int FilmId { get; set; }
        public int GanreId { get; set; }

        public virtual Film Film { get; set; }
        public virtual Ganre Ganre { get; set; }
    }
}
