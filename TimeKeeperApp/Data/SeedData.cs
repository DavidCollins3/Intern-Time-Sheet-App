using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.VisualBasic;
using System.Net;
using TimeKeeperApp.Authorization;
using TimeKeeperApp.Models;

namespace TimeKeeperApp.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // For testing purposes, all users share the same password "Sw0rdfish!"
                // password set using "dotnet user-secrets set SeedUserPW Sw0rdfish!"

                // admin user admin@email.com
                // can do anything & everything
                var adminID = await EnsureUser(serviceProvider, testUserPw, "admin@email.com");
                await EnsureRole(serviceProvider, adminID, Authorization.Constants.AdminRole);

                // supervisor user super@email.com
                // can create intern accounts & approve time entries
                var superID = await EnsureUser(serviceProvider, testUserPw, "super@email.com");
                await EnsureRole(serviceProvider, superID, Authorization.Constants.SuperRole);

                // intern users intern@email.com & intern2@email.com
                // can create & edit their own time entries
                var internID = await EnsureUser(serviceProvider, testUserPw, "intern@email.com");
                await EnsureRole(serviceProvider, internID, Authorization.Constants.InternRole);

                var intern2ID = await EnsureUser(serviceProvider, testUserPw, "intern2@email.com");
                await EnsureRole(serviceProvider, intern2ID, Authorization.Constants.InternRole);

                SeedDB(context, internID);
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceprovider,
                                                    string testUserPw, string UserName)
        {
            var userManager = serviceprovider.GetService<UserManager<IdentityUser>>();

            var user = await userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = UserName,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, testUserPw);
            }
            if (user == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }
            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
                                                            string uid, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (roleManager == null)
            {
                throw new Exception("roleManager is null");
            }

            IdentityResult IR;
            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            // if (userManager == null)
            // {
            //     throw new Exception("userManager is null");
            // }

            var user = await userManager.FindByIdAsync(uid);

            if (user == null)
            {
                throw new Exception("The user was not found. The testUserPW was probably not strong enough!");
            }

            IR = await userManager.AddToRoleAsync(user, role);

            return IR;
        }

        public static void SeedDB(ApplicationDbContext context, string internID)
        {
            if (context.TimeEntry.Any())
            {
                return;      // DB has been seeded
            }

            context.TimeEntry.AddRange(
                new TimeEntry
                {
                    UserID = internID,
                    Week = new DateOnly(2025, 10, 26),
                    TimeIn = new DateTime(2025, 10, 27, 9, 0, 0),
                    TimeOut = new DateTime(2025, 10, 27, 17, 0, 0),
                    //HoursWorked = TimeOut.Subtract(TimeIn).ToString(),
                    ApprovalStatus = true
                },
                new TimeEntry
                {
                    UserID = internID,
                    Week = new DateOnly(2025, 10, 26),
                    TimeIn = new DateTime(2025, 10, 28, 9, 0, 0),
                    TimeOut = new DateTime(2025, 10, 28, 17, 0, 0),
                    //HoursWorked = TimeOut.Subtract(TimeIn).ToString(),
                    ApprovalStatus = true
                },
                new TimeEntry
                {
                    UserID = internID,
                    Week = new DateOnly(2025, 10, 26),
                    TimeIn = new DateTime(2025, 10, 29, 9, 0, 0),
                    TimeOut = new DateTime(2025, 10, 29, 17, 0, 0),
                    //HoursWorked = TimeOut.Subtract(TimeIn).ToString(),
                    ApprovalStatus = true
                },
                new TimeEntry
                {
                    UserID = internID,
                    Week = new DateOnly(2025, 10, 26),
                    TimeIn = new DateTime(2025, 10, 30, 9, 0, 0),
                    TimeOut = new DateTime(2025, 10, 30, 17, 0, 0),
                    //HoursWorked = TimeOut.Subtract(TimeIn).ToString(),
                    ApprovalStatus = true
                },
                new TimeEntry
                {
                    UserID = internID,
                    Week = new DateOnly(2025, 10, 26),
                    TimeIn = new DateTime(2025, 10, 31, 9, 0, 0),
                    TimeOut = new DateTime(2025, 10, 31, 17, 0, 0),
                    //HoursWorked = TimeOut.Subtract(TimeIn).ToString(),
                    ApprovalStatus = true
                },
                new TimeEntry
                {
                    UserID = internID,
                    Week = new DateOnly(2025, 11, 2),
                    TimeIn = new DateTime(2025, 11, 3, 9, 0, 0),
                    TimeOut = new DateTime(2025, 11, 3, 17, 0, 0),
                    //HoursWorked = TimeOut.Subtract(TimeIn).ToString(),
                    ApprovalStatus = false
                },
                new TimeEntry
                {
                    UserID = internID,
                    Week = new DateOnly(2025, 11, 2),
                    TimeIn = new DateTime(2025, 11, 4, 9, 0, 0),
                    TimeOut = new DateTime(2025, 11, 4, 17, 0, 0),
                    //HoursWorked = TimeOut.Subtract(TimeIn).ToString(),
                    ApprovalStatus = false
                },
                new TimeEntry
                {
                    UserID = internID,
                    Week = new DateOnly(2025, 11, 2),
                    TimeIn = new DateTime(2025, 11, 5, 9, 0, 0),
                    TimeOut = new DateTime(2025, 11, 5, 17, 0, 0),
                    //HoursWorked = TimeOut.Subtract(TimeIn).ToString(),
                    ApprovalStatus = false
                }
             );
            context.SaveChanges();
        }
    }
}
