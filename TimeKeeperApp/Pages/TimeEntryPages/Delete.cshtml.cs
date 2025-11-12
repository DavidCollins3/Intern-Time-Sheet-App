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
    public class DeleteModel : DI_BasePageModel
    {
        public DeleteModel(
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
            TimeEntry? _timeEntry = await Context.TimeEntry
                .FirstOrDefaultAsync(m => m.TimeEntryId == id);

            if (_timeEntry == null)
            {
                return NotFound();
            }
            TimeEntry = _timeEntry;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                     User, TimeEntry,
                                                     TimeEntryOperations.Delete);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var timeEntry = await Context
                .TimeEntry.AsNoTracking()
                .FirstOrDefaultAsync(m => m.TimeEntryId == id);

            if (timeEntry == null)
            {
                return Forbid();
            }

            Context.TimeEntry.Remove(timeEntry);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
