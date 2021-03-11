using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieSearch
{
    public class FilmUserRelationship
    {
        public int Id { get; set; }
        public int FilmId { get; set; }
        public string UserName { get; set; }
        public virtual Film Film { get; set; }
    }
}
