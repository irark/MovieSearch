using Microsoft.AspNetCore.Identity;

namespace MovieSearch.Models
{
    public class User : IdentityUser
    {
        public int Year { get; set; }
    }
}


