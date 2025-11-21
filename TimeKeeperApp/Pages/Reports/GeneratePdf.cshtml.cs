using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using System;
using System.Linq;
using System.Threading.Tasks;
using TimeKeeperApp.Data;
using TimeKeeperApp.Models;
using System.Collections.Generic;

namespace TimeKeeperApp.Pages.Reports
{
    public class GeneratePdfModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IRazorViewRenderer _renderer;

        public GeneratePdfModel(ApplicationDbContext db, IRazorViewRenderer renderer)
        {
            _db = db;
            _renderer = renderer;
        }

        // GET /Reports/GeneratePdf?week=2025-11-17
        public async Task<IActionResult> OnGetAsync(string week)
        {
            DateOnly parsed;
            DateOnly? parsedWeek = DateOnly.TryParse(week, out parsed) ? parsed : (DateOnly?)null;

            // Query joined data and project to DTOs (server-side join)
            var entries = await (from t in _db.TimeEntry
                                 join u in _db.Users on t.UserID equals u.Id into uj
                                 from u in uj.DefaultIfEmpty()
                                 where !parsedWeek.HasValue || t.Week == parsedWeek.Value 
                                    & t.ApprovalStatus == true
                                 orderby u.UserName, t.Week, t.TimeIn
                                 select new
                                 {
                                     UserName = u != null ? u.UserName : t.UserID,
                                     t.Week,
                                     t.TimeIn,
                                     t.TimeOut,
                                     t.HoursWorked
                                 }).ToListAsync();

            var dtoEntries = new List<TimeEntryReportDto>(entries.Count);
            foreach (var e in entries)
            {
                double hours = 0;
                if (e.TimeOut.HasValue)
                {
                    hours = (e.TimeOut.Value - e.TimeIn).TotalHours;
                }

                // per-entry "H:mm" formatted string
                var perHoursFormatted = FormatHours(hours);

                dtoEntries.Add(new TimeEntryReportDto
                {
                    UserName = e.UserName,
                    Week = e.Week,
                    TimeIn = e.TimeIn,
                    TimeOut = e.TimeOut,
                    HoursWorked = perHoursFormatted,
                    HoursDecimal = hours
                });
            }

            // Group totals by username
            var totals = dtoEntries
                .GroupBy(x => x.UserName ?? "")
                .Select(g =>
                {
                    var sum = g.Sum(x => x.HoursDecimal);
                    return new UserWeekTotalDto
                    {
                        UserName = g.Key,
                        TotalHours = sum,
                        TotalHoursFormatted = FormatHours(sum)
                    };
                })
                .OrderBy(t => t.UserName)
                .ToList();

            var model = new ReportViewModel
            {
                Entries = dtoEntries,
                Totals = totals
            };

            // Render the partial to HTML. pass the ReportViewModel (not the raw list)
            var html = await _renderer.RenderViewToStringAsync("/Pages/TimeEntryPages/ReportPartial.cshtml", model);

            // Convert HTML to PDF using Playwright
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            await page.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });
            var pdfBytes = await page.PdfAsync(new PagePdfOptions { Format = "A4", PrintBackground = true });

            var fileName = $"TimeSheet-{(parsedWeek?.ToString("yyyy-MM-dd") ?? "All")}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        private static string FormatHours(double hours)
        {
            // convert fractional hours to H:mm where H can exceed 24
            var totalMinutes = (int)Math.Round(hours * 60);
            var h = totalMinutes / 60;
            var m = Math.Abs(totalMinutes % 60);
            return $"{h}:{m:00}";
        }
    }
}