namespace PPlanner.Migrations
{
    using PPlanner.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Security;
    using WebMatrix.WebData;

    internal sealed class Configuration : DbMigrationsConfiguration<PPlanner.Models.ProjectDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(PPlanner.Models.ProjectDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            context.ItemStatuses.AddOrUpdate(
              ists => ists.Status,
              new ItemStatus { Status = "To Do" },
              new ItemStatus { Status = "In Progress" },
              new ItemStatus { Status = "Done" },
              new ItemStatus { Status = "Deleted" }
            );

            SeedMemberShip();
        }

        private void SeedMemberShip()
        {
            if (!WebSecurity.Initialized)
            {
                WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            }

            var roles = (SimpleRoleProvider)Roles.Provider;
            var membership = (SimpleMembershipProvider)Membership.Provider;
            if (!roles.RoleExists("Admin"))
            {
                roles.CreateRole("Admin");
            }
            if (!roles.RoleExists("Product owner"))
            {
                roles.CreateRole("Product owner");
            }
            if (!roles.RoleExists("Scrum master"))
            {
                roles.CreateRole("Scrum master");
            }
            if (!roles.RoleExists("Developer"))
            {
                roles.CreateRole("Developer");
            }
            //if (membership.GetUser("admin", false) == null)
            if (!WebSecurity.UserExists("admin"))
            {
                //membership.CreateUserAndAccount("admin", "admin", new { isApproved = "as"});

                WebSecurity.CreateUserAndAccount(
                        "admin",
                        "admin",
                        new { isApproved = true });
            }

            if (!roles.GetRolesForUser("admin").Contains("Admin"))
            {
                roles.AddUsersToRoles(new[] { "admin" }, new[] { "Admin" });
            }

        }
    }
}
