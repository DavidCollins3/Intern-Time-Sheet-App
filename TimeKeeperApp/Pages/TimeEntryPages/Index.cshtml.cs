using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public IList<TimeEntry> TimeEntry { get; set; } = new List<TimeEntry>();
        public List<DateOnly> Weeks { get; set; } = new List<DateOnly>();

        // Use SelectListItem so the view can render username text and user id value
        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();

        [BindProperty(SupportsGet = true)]
        public string? SelectedWeek { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedUser { get; set; }

        public async Task OnGetAsync()
        {
            var timeEntries = from t in Context.TimeEntry
                              select t;

            var isAuthorized = User.IsInRole(Constants.AdminRole) ||
                               User.IsInRole(Constants.SuperRole);

            var currentUserId = UserManager.GetUserId(User);

            Weeks = timeEntries.Select(t => t.Week).Distinct().ToList();

            // Build user select list from users who have time entries
            Users = await Context.Users
                       .Where(u => Context.TimeEntry.Any(te => te.UserID == u.Id))
                       .OrderBy(u => u.UserName)
                       .Select(u => new SelectListItem { Value = u.Id, Text = u.UserName })
                       .ToListAsync();

            // Apply week filter (ISO format yyyy-MM-dd expected from the select)
            if (!string.IsNullOrEmpty(SelectedWeek)
                && DateOnly.TryParseExact(SelectedWeek, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedWeek))
            {
                timeEntries = timeEntries.Where(t => t.Week == parsedWeek);
            }

            // Apply user filter (admins/supervisors only)
            if (!string.IsNullOrEmpty(SelectedUser) && isAuthorized)
            {
                timeEntries = timeEntries.Where(t => t.UserID == SelectedUser);
            }

            // Only show your entries for non-authorized users
            if (!isAuthorized)
            {
                timeEntries = timeEntries.Where(t => t.UserID == currentUserId);
            }

            timeEntries = timeEntries.OrderBy(t => t.TimeIn);
            TimeEntry = await timeEntries.ToListAsync();
        }

        // handle the filter form
        public IActionResult OnPostWeeks()
        {
            // Redirect to GET so query string preserves filters and Generate Report link will include them
            return RedirectToPage("./Index", new { SelectedWeek, SelectedUser });
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

            approveHelper(timeEntry);

            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostApproveAll()
        {
            var timeEntries = from t in Context.TimeEntry
                              select t;

            if (!string.IsNullOrEmpty(SelectedWeek)
                && DateOnly.TryParse(SelectedWeek, out var parsedWeek))
            {
                timeEntries = timeEntries.Where(t => t.Week == parsedWeek);
            }

            if (timeEntries == null)
            {
                return NotFound();
            }

            foreach (var timeEntry in timeEntries)
            {

                var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, timeEntry,
                                                     TimeEntryOperations.Approve);
                if (!isAuthorized.Succeeded)
                {
                    return Forbid();
                }
                approveHelper(timeEntry);
            }
            await Context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        public void approveHelper(TimeEntry timeEntry)
        {
            timeEntry.ApprovalStatus = true;
            Context.TimeEntry.Update(timeEntry);
        }
    }
}
