using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TimeKeeperApp.Authorization;
using TimeKeeperApp.Data;
using TimeKeeperApp.Models;

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
        public List<DateOnly> Weeks { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedWeek { get; set; }

        public async Task OnGetAsync()
        {
            var timeEntries = from t in Context.TimeEntry
                              select t;

            var isAuthorized = User.IsInRole(Constants.AdminRole) ||
                               User.IsInRole(Constants.SuperRole);

            var currentUserId = UserManager.GetUserId(User);

            Weeks = timeEntries.Select(t => t.Week).Distinct().ToList();


            // Only your Time Entries are shown
            // UNLESS you're a supervisor or admin
            if (!isAuthorized)
            {
                    timeEntries = timeEntries.Where(t => t.UserID == currentUserId
                                                    //&& t.Week.ToString() == SelectedWeek
                                                    );
            }
            timeEntries = timeEntries.OrderBy(t => t.TimeIn);
            TimeEntry = await timeEntries.ToListAsync();
        }

        public void OnPostWeeks()
        {
            var timeEntries = from t in Context.TimeEntry
                              select t;

            var isAuthorized = User.IsInRole(Constants.AdminRole) ||
                               User.IsInRole(Constants.SuperRole);

            var currentUserId = UserManager.GetUserId(User);

            Weeks = timeEntries.Select(t => t.Week).Distinct().ToList();

            // Only your Time Entries are shown
            // UNLESS you're a supervisor or admin
            // Parse the selected week using the same ISO format the select emits
            if (!string.IsNullOrEmpty(SelectedWeek)
                && DateOnly.TryParse(SelectedWeek, out var parsedWeek))
            {
                timeEntries = timeEntries.Where(t => t.Week == parsedWeek);
            }

            // Only your Time Entries are shown UNLESS you're a supervisor or admin
            if (!isAuthorized)
            {
                timeEntries = timeEntries.Where(t => t.UserID == currentUserId);
            }
            timeEntries = timeEntries.OrderBy(t => t.TimeIn);
            TimeEntry = timeEntries.ToList();
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
