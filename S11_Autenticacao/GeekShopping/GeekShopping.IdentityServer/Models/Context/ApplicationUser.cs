using Microsoft.AspNetCore.Identity;

namespace GeekShopping.IdentityServer.Models.Context
{
    public class ApplicationUser : IdentityUser
    {
        private string FirstName { get; set; }
        private string LastName { get; set; }
    }
}