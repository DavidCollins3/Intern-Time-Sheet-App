using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimeKeeperApp.Data;
using TimeKeeperApp.Models;
using TimeKeeperApp.Authorization;

namespace TimeKeeperApp.Pages.TimeEntryPages
{
    public class CreateModel : DI_BasePageModel
    {
        public CreateModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public TimeEntry TimeEntry { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            TimeEntry.UserID = UserManager.GetUserId(User);

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                        User, TimeEntry,
                                                        TimeEntryOperations.Create);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Context.TimeEntry.Add(TimeEntry);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
