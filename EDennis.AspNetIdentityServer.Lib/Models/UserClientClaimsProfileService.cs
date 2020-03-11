﻿using EDennis.AspNetIdentityServer.Data;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNetIdentityServer.Lib.Models {

    /// <summary>
    /// Gets all roles and role claims prefixed by client_id, as well as
    /// all requested user claims.
    /// </summary>
    public class UserClientClaimsProfileService : IProfileService {

        private readonly AspNetIdentityDbContext _dbContext;

        public UserClientClaimsProfileService(AspNetIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context) {

            var clientId = context.Client.ClientId;
            var userId = context.Subject.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.EndsWith("NameIdentifier"))?.Value;

            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(userId)) {

                var roles = (from r in _dbContext.Roles
                             join ur in _dbContext.UserRoles on r.Id equals ur.RoleId
                             join u in _dbContext.Users on ur.UserId equals u.Id
                             where u.Id == userId && r.Name.StartsWith(clientId + ".")
                             select new Claim("role", r.Name)).ToList();

                context.IssuedClaims.AddRange(roles);

                var roleClaims = (from r in _dbContext.Roles
                             join ur in _dbContext.UserRoles on r.Id equals ur.RoleId
                             join u in _dbContext.Users on ur.UserId equals u.Id
                             join rc in _dbContext.RoleClaims on r.Id equals rc.RoleId
                             where u.Id == userId && r.Name.StartsWith(clientId + ".")
                             select new Claim(rc.ClaimType, rc.ClaimValue)).ToList();

                context.IssuedClaims.AddRange(roleClaims);

                var userClaims = (from uc in _dbContext.UserClaims
                                  join u in _dbContext.Users on uc.UserId equals u.Id
                                  where u.Id == userId 
                                  select new Claim(uc.ClaimType, uc.ClaimValue)).ToList();

                var requestedClaims = userClaims.Where(x => context.RequestedClaimTypes.Contains(x.Type,StringComparer.OrdinalIgnoreCase)).ToList();
                

                context.IssuedClaims.AddRange(requestedClaims);

            }
            return Task.CompletedTask;

        }

        public Task IsActiveAsync(IsActiveContext context) {
            context.IsActive = true;
            return Task.FromResult(true);
        }
    }
}
