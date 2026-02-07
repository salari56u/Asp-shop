using DigiStore.Data;
using DigiStore.Models;
using DigiStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DigiStore.Areas.Admin.Controllers
{
    public class UsersController : BaseAdminController
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {

            var currentAdminRole = User.FindFirstValue(ClaimTypes.Role);
            var isSuperAdmin = currentAdminRole == "SuperAdmin";

            var usersQuery = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!isSuperAdmin)
            {
                usersQuery = usersQuery.Where(u => !u.UserRoles.Any(ur => ur.Role.Name == "Admin" || ur.Role.Name == "SuperAdmin"));
            }

            var users = await usersQuery.ToListAsync();
            return View(users);
        }


        public async Task<IActionResult> Create()
        {
            await PrepareRolesViewBag();
            return View(new UserCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareRolesViewBag();
                return View(model); 
            }

            if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "این نام کاربری تکراری است.");
                await PrepareRolesViewBag();
                return View(model);
            }

            var newUser = new User
            {
                UserName = model.UserName,
                Email = model.Email ?? model.UserName + "@moboland.ir",
                PasswordHash = model.Password, 
                UserRoles = new List<UserRole> { new UserRole { RoleId = model.RoleId } }
            };

            _context.Users.Add(newUser);
            _context.Wallets.Add(new Wallet { User = newUser, Balance = 0 });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var model = new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                RoleId = user.UserRoles.FirstOrDefault()?.RoleId ?? 0
            };

            var currentAdminRole = User.FindFirstValue(ClaimTypes.Role);
            var targetUserRole = user.UserRoles.FirstOrDefault()?.RoleId;

            if (targetUserRole != null)
            {
                var roleName = await _context.Roles
                    .Where(r => r.Id == targetUserRole)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync();

                if (currentAdminRole != "SuperAdmin" && (roleName == "Admin" || roleName == "SuperAdmin"))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }

            await PrepareRolesViewBag(model.RoleId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareRolesViewBag(model.RoleId);
                return View(model);
            }

            var user = await _context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == model.Id);
            if (user == null) return NotFound();

            user.UserName = model.UserName;
            user.Email = model.Email;

            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = model.Password;
            }

            // آپدیت نقش
            var currentRoles = _context.UserRoles.Where(ur => ur.UserId == model.Id);
            _context.UserRoles.RemoveRange(currentRoles);
            _context.UserRoles.Add(new UserRole { UserId = model.Id, RoleId = model.RoleId });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();



            var isSuperAdmin = User.IsInRole("SuperAdmin");
            var targetRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == id)
                .ToListAsync();

            var isTargetAdmin = targetRoles.Any(tr => tr.Role.Name == "Admin" || tr.Role.Name == "SuperAdmin");

            if (isTargetAdmin && !isSuperAdmin)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task PrepareRolesViewBag(int? selectedId = null)
        {
            var currentAdminRole = User.FindFirstValue(ClaimTypes.Role);

            IQueryable<Role> rolesQuery = _context.Roles;

            if (currentAdminRole != "SuperAdmin")
            {
                rolesQuery = rolesQuery.Where(r => r.Name == "Customer");
            }

            var roles = await rolesQuery.ToListAsync();

            ViewBag.Roles = new SelectList(roles, "Id", "Name", selectedId);
        }
    }
}