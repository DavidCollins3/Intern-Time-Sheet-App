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

        
        [BindProperty(SupportsGet = true)]
        public IList<int> Weeks { get; set; }


        public async Task OnGetAsync()
        {
            var timeEntries = from t in Context.TimeEntry
                              select t;

            var isAuthorized = User.IsInRole(Constants.AdminRole) ||
                               User.IsInRole(Constants.SuperRole);

            var currentUserId = UserManager.GetUserId(User);

            var weeks = timeEntries.Select(t => t.Week).Distinct().ToList();
            ViewData["Weeks"] = weeks;

            //var selectedWeek = Request.Form["SelectWeek"];

            // Only your Time Entries are shown
            // UNLESS you're a supervisor or admin
            if (!isAuthorized)
            {
                timeEntries = timeEntries.Where(t => t.UserID == currentUserId
                                                //&& t.Week.ToString() == selectedWeek
                                                );
            }

            timeEntries = timeEntries.OrderBy(t => t.TimeIn);

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

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, timeEntry,
                                                     TimeEntryOperations.Approve);

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
