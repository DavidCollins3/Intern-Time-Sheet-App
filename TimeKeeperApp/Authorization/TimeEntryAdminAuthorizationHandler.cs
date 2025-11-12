using TimeKeeperApp.Models;
using TimeKeeperApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace TimeKeeperApp.Authorization
{
    public class TimeEntryAdminAuthorizationHandler
                    : AuthorizationHandler<OperationAuthorizationRequirement, TimeEntry>
    {
        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                   OperationAuthorizationRequirement requirement,
                                   TimeEntry resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.CompletedTask;
            }
            // If the user is in the Admin role, allow all operations.
            if (context.User.IsInRole(Constants.AdminRole))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
