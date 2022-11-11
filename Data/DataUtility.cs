using BlogMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BlogMVC.Data
{
    // sets up roles and users
    // static ensures class is instantiated only once
    // utilized during execution of applicaton
    public static class DataUtility
    {
        // roles
        private const string _adminRole = "Administrator";
        private const string _modRole = "Moderator";
        // TODO: change to real email when deploying to Railway
        private const string _adminEmail = "tnguyen@mailinator.com";
        private const string _modEmail = "ttbmoderator@mailinator.com";

        // begin setting up connection to db
        public static string GetConnectionString(IConfiguration config)
        {
            string? connectionString = config.GetConnectionString("DefaultConnection");

            string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
        }

        // sets db properties
        public static string BuildConnectionString(string databaseUrl)
        {
            // provides an object representation of a uniform resource identifier (uri) and easy access to parts of the uri
            var databaseUri = new Uri(databaseUrl);

            var userInfo = databaseUri.UserInfo.Split(':');

            // create and manage the contents of connection strings used by the NpgsqlConnection class
            var builder = new NpgsqlConnectionStringBuilder()
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Require,
                TrustServerCertificate = true
            };

            return builder.ToString();
        }

        // establishes connection to db
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            // obtaining necessary services based on the IServiceProvider param
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
            var configSvc = svcProvider.GetRequiredService<IConfiguration>();
            var userManagerSvc = svcProvider.GetRequiredService<UserManager<BlogUser>>();
            var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // align db by checking Migrations
            await dbContextSvc.Database.MigrateAsync();

            // seed Default Roles
            await SeedRolesAsync(roleManagerSvc);

            // seed Users
            await SeedUsersAsync(dbContextSvc, configSvc, userManagerSvc);
        }

        // creates a role for users
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(_adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(_adminRole));
            }

            if (!await roleManager.RoleExistsAsync(_modRole))
            {
                await roleManager.CreateAsync(new IdentityRole(_modRole));
            }
        }
        
        // creates demo login users
        private static async Task SeedUsersAsync(ApplicationDbContext context, IConfiguration config, UserManager<BlogUser> userManager)
        {
            // creates an admin user
            if (!context.Users.Any(u => u.Email == _adminEmail))
            {
                BlogUser adminUser = new()
                {
                    UserName = _adminEmail,
                    Email = _adminEmail,
                    FirstName = "Tom",
                    LastName = "Nguyen",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, config["AdminPassword"] ?? Environment.GetEnvironmentVariable("AdminPassword"));
                await userManager.AddToRoleAsync(adminUser, _adminRole);
            }

            // creates a mod user
            if (!context.Users.Any(u => u.Email == _modEmail))
            {
                BlogUser modUser = new()
                {
                    UserName = _modEmail,
                    Email = _modEmail,
                    FirstName = "Mod",
                    LastName = "User",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(modUser, config["ModeratorPassword"] ?? Environment.GetEnvironmentVariable("ModeratorPassword"));
                await userManager.AddToRoleAsync(modUser, _modRole);
            }
        }
    }
}
