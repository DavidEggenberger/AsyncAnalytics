﻿using Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Types.Overrides
{
    public class ApplicationUserClaimsPrincipalFactory<User> : IUserClaimsPrincipalFactory<User> where User : ApplicationUser
    {
        private ApplicationUserManager applicationUserManager;
        public ApplicationUserClaimsPrincipalFactory(ApplicationUserManager TeamApplicationUserManager)
        {
            this.applicationUserManager = TeamApplicationUserManager;
        }
        public async Task<ClaimsPrincipal> CreateAsync(User user)
        {
            ApplicationUser applicationUser = await applicationUserManager.FindByIdAsync(user.Id.ToString());
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, applicationUser.UserName),
                new Claim(ClaimTypes.Email, applicationUser.Email),
                new Claim("picture", applicationUser.PictureUri),
                new Claim(ClaimTypes.Sid, applicationUser.Id.ToString())
            };
            var result = await applicationUserManager.GetMembershipClaimsForApplicationUser(user);
            if (result.Successful)
            {
                claims.AddRange(result.Value);
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme, nameType: ClaimTypes.NameIdentifier, ClaimTypes.Role);
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            
            return claimsPrincipal;
        }
    }
}
