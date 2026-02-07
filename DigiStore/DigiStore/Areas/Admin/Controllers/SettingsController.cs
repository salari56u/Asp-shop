using DigiStore.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : BaseAdminController
    {
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings.OrderByDescending(s => s.CreatedAt).ToListAsync();
            return View(settings);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var setting = await _context.SiteSettings.FindAsync(id);
            if (setting == null) return NotFound();
            ViewBag.setting = setting.Id;
            return View(setting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SiteSetting model, IFormFile? uploadFile)
        {
            ModelState.Remove("Key");   
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return View(model);
            }

            var setting = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.Id);
            if (setting == null) return NotFound();
            model.Key = setting.Key;
            model.CreatedAt = setting.CreatedAt;

            if (setting.Key == "Site_Logo" && uploadFile != null)
            {
                string fileName = "logo" + Path.GetExtension(uploadFile.FileName);
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadFile.CopyToAsync(stream);
                }

                setting.Value = "/uploads/" + fileName;
            }
            else
            {
                setting.Value = model.Value;
            }

            setting.Description = model.Description;
            setting.UpdatedAt = DateTime.Now;

            _context.Update(setting);
            await _context.SaveChangesAsync();
            SiteSettingService.ClearCache();
            return RedirectToAction(nameof(Index));
        }
    }
}
