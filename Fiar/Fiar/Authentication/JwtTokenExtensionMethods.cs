using Ixs.DNA;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fiar
{
    /// <summary>
    /// Extension methods for working with Jwt bearer tokens
    /// </summary>
    public static class JwtTokenExtensionMethods
    {
        /// <summary>
        /// Generates a Jwt bearer token containing the users username
        /// </summary>
        /// <param name="user">The users details</param>
        /// <param name="user">The users roles</param>
        /// <returns></returns>
        public static string GenerateJwtToken(this ApplicationUser user, IList<string> userRoles)
        {
            // Set our token claims (key/value pair)
            var claims = new[]
            {
                // Unique ID for this token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                
                // The username using the Identity name so it fills out the HttpContext.User.Identity.Name value
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),

                // Add user Id so that UserManager.GetUserAsync can find the user based on Id
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };
            // Add user roles into the claims
            foreach (var role in userRoles)
                claims = claims.Append(new Claim(ClaimTypes.Role, role));

            // Create the credentials used to generate the token.
            var credentials = new SigningCredentials(
                // Get the secret key from the configuration.
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DI.ConfigBox.Jwt_SecretKey)),
                // Use HS256 algorithm
                SecurityAlgorithms.HmacSha256
                );

            // Generate the Jwt Token
            var token = new JwtSecurityToken(
                issuer: DI.ConfigBox.Jwt_Issuer,
                audience: DI.ConfigBox.Jwt_Audience,
                claims: claims,
                // Expire if not used for X seconds
                expires: DateTime.Now.AddSeconds(DI.ConfigBox.Jwt_Expires),
                signingCredentials: credentials
                );

            // Return the generated token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
