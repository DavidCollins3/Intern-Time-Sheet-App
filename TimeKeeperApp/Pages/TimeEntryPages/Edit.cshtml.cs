using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeKeeperApp.Data;
using TimeKeeperApp.Models;
using TimeKeeperApp.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace TimeKeeperApp.Pages.TimeEntryPages
{
    public class EditModel : DI_BasePageModel
    {
        public EditModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        [BindProperty]
        public TimeEntry TimeEntry { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            TimeEntry? timeEntry = await Context.TimeEntry
                .FirstOrDefaultAsync(m => m.TimeEntryId == id);

            if (timeEntry == null)
            {
                return NotFound();
            }

            TimeEntry = timeEntry;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                      User, TimeEntry,
                                                      TimeEntryOperations.Update);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Fetch Time Entry from DB to get UserID
            var timeEntry = await Context
                .TimeEntry.AsNoTracking()
                .FirstOrDefaultAsync(m => m.TimeEntryId == id);

            if(timeEntry == null )
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, timeEntry,
                                                     TimeEntryOperations.Update);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            TimeEntry.UserID = timeEntry.UserID;

            Context.Attach(TimeEntry).State = EntityState.Modified;

            if (TimeEntry.ApprovalStatus == true)
            {
                // If the time entry is updated after approval,
                // and since the user cannot approve,
                // reset ApprovalStatus to false
                // so the update can be verified and approved again.
                var canApprove = await AuthorizationService.AuthorizeAsync(
                                                     User, TimeEntry,
                                                     TimeEntryOperations.Approve);

                if (!canApprove.Succeeded)
                {
                    TimeEntry.ApprovalStatus = false;
                }
            }

            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
