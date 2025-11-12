using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TimeKeeperApp.Data;
using TimeKeeperApp.Models;
using TimeKeeperApp.Authorization;

namespace TimeKeeperApp.Pages.TimeEntryPages
{
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IList<TimeEntry> TimeEntry { get;set; }

        public async Task OnGetAsync()
        {
            var timeEntries = from t in Context.TimeEntry
                              select t;

            var isAuthorized = User.IsInRole(Constants.AdminRole) ||
                               User.IsInRole(Constants.SuperRole);

            var currentUserId = UserManager.GetUserId(User);

            // Only approved Time Entries are shown
            // UNLESS you're the owner, a supervisor, or an admin
            if (!isAuthorized)
            {
                timeEntries = timeEntries.Where(t => t.ApprovalStatus == true
                                                    || t.UserID == currentUserId);
            }

            TimeEntry = await timeEntries.ToListAsync();
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
