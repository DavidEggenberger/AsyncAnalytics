﻿using Infrastructure.Identity;
using Infrastructure.Identity.Types;
using Infrastructure.Identity.Types.Constants;
using Infrastructure.Identity.Types.Enums;
using Infrastructure.Identity.Types.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Services
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        private IdentificationDbContext identificationDbContext;
        public ApplicationUserManager(IdentificationDbContext identificationDbContext, IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<ApplicationUserManager> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.identificationDbContext = identificationDbContext;
        }

        public async Task<ApplicationUser> FindAsync(HttpContext httpContext)
        {
            ApplicationUser user;
            if ((user = identificationDbContext.Users.SingleOrDefault(u => u.Id.ToString() == httpContext.User.FindFirst(ClaimTypes.Sid).Value)) != null)
            {
                return user;
            }
            throw new IdentityOperationException("No user is found for the provided HttpContext");
        }
        public async Task<ApplicationUser> FindByStripeCustomerId(string stripeCustomerId)
        {
            ApplicationUser user;
            if((user = identificationDbContext.Users.SingleOrDefault(u => u.StripeCustomerId == stripeCustomerId)) != null)
            {
                return user;
            }
            throw new IdentityOperationException("No user is found for the provided HttpContext");
        }
        public async Task<Guid> GetSelectedTeamId(ApplicationUser applicationUser)
        {
            await identificationDbContext.Entry(applicationUser).Collection(x => x.Memberships).Query().Include(x => x.Team).LoadAsync();
            Team team;
            if ((team = applicationUser.Memberships.SingleOrDefault(x => x.Status == UserSelectionStatus.Selected)?.Team) != null)
            {
                return team.Id;
            }
            throw new IdentityOperationException("No user is found for the provided HttpContext");
        }
        public async Task UnSelectAllTeams(ApplicationUser applicationUser)
        {
            await identificationDbContext.Entry(applicationUser).Collection(x => x.Memberships).LoadAsync();
            applicationUser.Memberships?.ToList().ForEach(x => x.Status = UserSelectionStatus.NotSelected);
            await identificationDbContext.SaveChangesAsync();
        }
        public async Task SelectTeamForUser(ApplicationUser applicationUser, Team team)
        {
            applicationUser.Memberships.ToList().ForEach(x => x.Status = UserSelectionStatus.NotSelected);
            applicationUser.Memberships.Where(x => x.TeamId == team.Id).First().Status = UserSelectionStatus.Selected;
            await identificationDbContext.SaveChangesAsync();
        }
        public async Task<List<ApplicationUserTeam>> GetAllTeamMemberships(ApplicationUser applicationUser)
        {
            return identificationDbContext.ApplicationUserTeams.Include(x => x.Team).Where(x => x.UserId == applicationUser.Id).ToList();
        }
        public async Task<List<Team>> GetTeamsWhereApplicationUserIsMember(ApplicationUser applicationUser)
        {
            throw new Exception();
        }
        public async Task<List<Team>> GetTeamsWhereApplicationUserIsAdmin(ApplicationUser applicationUser)
        {
            throw new Exception();
        }
        public async Task<IdentityOperationResult<List<Claim>>> GetMembershipClaimsForApplicationUser(ApplicationUser applicationUser)
        {
            ApplicationUser _applicationUser = await identificationDbContext.Users.Include(x => x.Memberships).FirstAsync(x => x.Id == applicationUser.Id);
            ApplicationUserTeam applicationUserTeam = _applicationUser.Memberships.Where(x => x.Status == UserSelectionStatus.Selected).FirstOrDefault();
            if(applicationUserTeam == null)
            {
                return IdentityOperationResult<List<Claim>>.Fail("");
            }
            List<Claim> claims = new List<Claim>
            {
                new Claim(IdentityStringConstants.IdentityTeamIdClaimType, applicationUserTeam.TeamId.ToString()),
                new Claim(IdentityStringConstants.IdentityTeamRoleClaimType, applicationUserTeam.Role.ToString())
            };
            return IdentityOperationResult<List<Claim>>.Success(claims);
        }
    }
}
