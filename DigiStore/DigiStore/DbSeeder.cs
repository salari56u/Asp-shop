using DigiStore.Data;
using DigiStore.Models;

namespace DigiStore.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { Name = "SuperAdmin" },
                    new Role { Name = "Admin" },
                    new Role { Name = "Customer" }
                };
                context.Roles.AddRange(roles);
                context.SaveChanges();
            }
            if (!context.Users.Any(u => u.UserName == "09000000000")) 
            {
                var superRole = context.Roles.First(r => r.Name == "SuperAdmin");

                var superAdmin = new User
                {
                    UserName = "09000000000",
                    Email = "admin@moboland.ir",
                    PasswordHash = "123456",
                    UserRoles = new List<UserRole>
                    {
                        new UserRole { RoleId = superRole.Id }
                    }
                };

                context.Users.Add(superAdmin);
                context.Wallets.Add(new Wallet { User = superAdmin, Balance = 0 });

                context.SaveChanges();
            }
        }
    }
}