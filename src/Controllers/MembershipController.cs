﻿using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Areas.Identity.Pages.Account.Manage;
using AdvantagePlatform.Data;
using LtiAdvantage.IdentityServer4;
using LtiAdvantage.NamesRoleProvisioningService;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Sample membership controller that implements the Membership service.
    /// See https://www.imsglobal.org/spec/lti-nrps/v2p0.
    /// </summary>
    public class MembershipController : MembershipControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MembershipController(
            ILogger<MembershipControllerBase> logger,
            ApplicationDbContext context) : base(logger)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns members of the course. Ignores filters.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The members of the sample course.</returns>
        protected override async Task<ActionResult<MembershipContainer>> OnGetMembershipAsync(GetMembershipRequest request)
        {
            // In this sample app, each registered app user has an associated platform,
            // course, and membership. So look up the user that owns the requested course.
            var user = await _context.GetUserByContextIdAsync(request.ContextId);
            if (user == null)
            {
                return NotFound(new ProblemDetails {Title = $"{nameof(request.ContextId)} not found."});
            }

            var membership = new MembershipContainer
            {
                Id = Request.GetDisplayUrl(),
                Context = new Context
                {
                    Id = user.Course.Id.ToString(),
                    Title = user.Course.Name
                }
            };

            if (user.People.Any())
            {
                var people = user.People
                    .Select(p => new Member
                    {
                        FamilyName = p.LastName,
                        GivenName = p.FirstName,
                        Roles = PeopleModel.ParsePersonRoles(p.Roles),
                        Status = MemberStatus.Active,
                        LisPersonSourcedId = p.SisId,
                        UserId = p.Id.ToString()
                    });

                if (request.Rlid.IsPresent())
                {
                    if (!int.TryParse(request.Rlid, out var id))
                    {
                        return NotFound(new ProblemDetails {Title = $"{nameof(request.Rlid)} not found."});
                    }

                    people = people.Where(p => user.Course.ResourceLinks.Any(l => l.Id == id));
                }

                if (request.Role.HasValue)
                {
                    people = people.Where(p => p.Roles.Any(r => r == request.Role.Value));
                }

                membership.Members = people.ToList();
            }

            return membership;
        }
    }
}
