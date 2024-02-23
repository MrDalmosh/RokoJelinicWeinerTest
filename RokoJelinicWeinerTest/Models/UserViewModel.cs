using Microsoft.AspNetCore.Identity;

namespace RokoJelinicWeinerTest.Models
{
    public class UserViewModel
    {
        public IdentityUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}
