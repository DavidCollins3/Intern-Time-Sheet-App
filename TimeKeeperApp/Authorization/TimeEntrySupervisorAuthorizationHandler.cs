using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System.Threading.Tasks;
using TimeKeeperApp.Data;
using TimeKeeperApp.Models;

namespace TimeKeeperApp.Authorization
{
    public class TimeEntrySupervisorAuthorizationHandler :
        AuthorizationHandler<OperationAuthorizationRequirement, TimeEntry>
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

            // If the operation is "Approve", allow supervisors to approve time entries.
            // If not "Approve", return.
            // Allow supervisors to read time entries, as well.
            if (requirement.Name != Constants.ApproveOperationName &&
                requirement.Name != Constants.ReadOperationName)
            {
                return Task.CompletedTask;
            }

            {
                if (context.User.IsInRole(Constants.SuperRole))
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}
