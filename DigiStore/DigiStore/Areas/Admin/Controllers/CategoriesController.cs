using DigiStore.Areas.Admin.Dtos;
using DigiStore.Areas.Admin.Models;
using DigiStore.Data;
using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DigiStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : BaseAdminController
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext Context)
        {
            _context = Context;
        }


        public IActionResult Index()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.IsSuccess = TempData["IsSuccess"];

            var category =  _context.Categories.ToList();
            return View(category);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CategoryModel category, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "wp-content", "uploads" , "Categories");
                        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        category.ImageName = uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ProfileImageUrl", "خطا در آپلود تصویر: " + ex.Message);
                        return View(category);
                    }
                }
                else
                {
                   
                    // teamMember.ProfileImageUrl = "/images/TeamMember/default-avatar.png";
                }

                Category category1 = new Category();
                category1.Title=category.Title;
                category1.ImageName = category.ImageName;
                _context.Categories.Add(category1);
                await _context.SaveChangesAsync();
                TempData["Message"] = "دسته بندی جدید با موفقیت افزوده شد.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index");
            }
            ViewBag.ErrorMessage = "لطفا تمام فیلدهای الزامی را به درستی پر کنید.";
            return View(category);
        }



        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            CategoryModel vm=new CategoryModel();
            vm.Title=category.Title;
            vm.ImageName= category.ImageName;
            return View(vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category, IFormFile? imageFile )
        {
            if (ModelState.IsValid)
            {
                string oldImagePathServer = null;
                string newImageRelativePath = null;

                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "wp-content", "uploads", "Categories");
                        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        newImageRelativePath =uniqueFileName;
                        category.ImageName = uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ProfileImageUrl", "خطا در آپلود تصویر: " + ex.Message);
                        return View(category);
                    }
                }

                var existingCategory = _context.Categories.Find(category.Id);
                if (existingCategory == null)
                {
                    return NotFound();
                }
                existingCategory.Title = category.Title;
                if (!string.IsNullOrEmpty(category.ImageName))
                {
                    oldImagePathServer = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/wp-content/uploads/Categories/", existingCategory.ImageName);
                }
                if (newImageRelativePath != null)
                {
                    existingCategory.ImageName = newImageRelativePath;
                }
                _context.Entry(existingCategory).State = EntityState.Modified;
                await _context.SaveChangesAsync();


                if (oldImagePathServer != null && System.IO.File.Exists(oldImagePathServer))
                {
                    try { System.IO.File.Delete(oldImagePathServer); }
                    catch (IOException ioEx) { System.Diagnostics.Debug.WriteLine("Error deleting image: " + ioEx.Message); }
                }
                TempData["Message"] = "اطلاعات دسته بندی با موفقیت ویرایش شد.";
                TempData["IsSuccess"] = true;
                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "لطفا تمام فیلدهای الزامی را به درستی پر کنید.";
            return View(category);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var member = _context.Categories.Find(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                TempData["Message"] = "دسته بندی مورد نظر یافت نشد.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            string imagePathToDelete = null;
            if (!string.IsNullOrEmpty(category.ImageName))
            {
                imagePathToDelete = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/wp-content/uploads/Categories/" , category.ImageName);
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            if (imagePathToDelete != null && System.IO.File.Exists(imagePathToDelete))
            {
                try { System.IO.File.Delete(imagePathToDelete); }
                catch (IOException ioEx) { System.Diagnostics.Debug.WriteLine("Error deleting image: " + ioEx.Message); }
            }


            TempData["Message"] = "دسته بندی با موفقیت حذف شد.";
            TempData["IsSuccess"] = true;
            return RedirectToAction("Index");
        }
    }
}
