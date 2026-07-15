using System.Security.Claims;
using System.Collections.Generic;
using System.Security.Claims;
using Domain.Entities;

namespace SICPAI.Helpers
{
    public static class ClaimsHelper
    {
        public static IEnumerable<Claim> GetUserClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            if (user.UserRoles != null)
            {
                foreach (var ur in user.UserRoles)
                {
                    if (ur.RoleCatalog != null && !string.IsNullOrEmpty(ur.RoleCatalog.Name))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, ur.RoleCatalog.Name));
                    }
                }
            }

            return claims;
        }
    }
}
