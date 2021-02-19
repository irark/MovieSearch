using System;
using System.Collections.Generic;

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
        public string Name { get; set; }

        public virtual ICollection<Film> Films { get; set; }
    }
}
