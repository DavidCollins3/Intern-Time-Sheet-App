using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TimeKeeperApp.Data;
using TimeKeeperApp.Models;
using TimeKeeperApp.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TimeKeeperApp.Pages.TimeEntryPages
{
    public class DetailsModel : DI_BasePageModel
    {
        public DetailsModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public TimeEntry TimeEntry { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            TimeEntry? _timeEntry = await Context.TimeEntry
                .FirstOrDefaultAsync(m => m.TimeEntryId == id);

            if (_timeEntry == null)
            {
                return NotFound();
            }

            TimeEntry = _timeEntry;

            var isAuthorized = User.IsInRole(Constants.AdminRole) ||
                               User.IsInRole(Constants.SuperRole);

            var currentUserId = UserManager.GetUserId(User);

            if (!isAuthorized
                && currentUserId != TimeEntry.UserID
                && TimeEntry.ApprovalStatus != true)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, bool approvalStatus)
        {
            var timeEntry = await Context.TimeEntry.FirstOrDefaultAsync(
                                                    m => m.TimeEntryId == id);

            if (timeEntry == null)
            {
                return NotFound();
            }

            var timeEntryOperation = (approvalStatus == timeEntry.ApprovalStatus)
                                        ? TimeEntryOperations.Approve
                                         : null;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, timeEntry,
                                                     timeEntryOperation);
            
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            timeEntry.ApprovalStatus = !approvalStatus;
            Context.TimeEntry.Update(timeEntry);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
