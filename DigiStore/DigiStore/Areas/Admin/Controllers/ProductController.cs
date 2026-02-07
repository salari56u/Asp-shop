using DigiStore.Data;
using DigiStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigiStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : BaseAdminController
    {
        private readonly AppDbContext _context;

        public ProductController (AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            int pageSize = 12;

            var p1 = _context.Products
                .Include(p => p.Images)
                .Include(p => p.ProductCategories)
                .ThenInclude(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                p1 = p1.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
            }


            var totalItems = await p1.CountAsync();
            var products = await p1
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // 5. پر کردن ویومدل
            var model = new ShopViewModel
            {
                Products = products,
                Categories = await _context.Categories.ToListAsync(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Search = search,
            };


            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Partials/_ProductGrid", model);
            }


            return View(model);

        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductCreateViewModel
            {
                Categories = await _context.Categories.ToListAsync()
            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }

            var product = new Product
            {
                Title = model.Title,
                Slug = model.Slug,
                Description = model.Description,
                Price = model.Price,
                OldPrice = model.OldPrice,
                Stock = model.Stock,
                CreatedAt = DateTime.Now,
                Images = new List<ProductImage>()
            };

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/wp-content/uploads/2025/01");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // تصویر اصلی
            if (model.MainImage != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.MainImage.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.MainImage.CopyToAsync(stream);

                product.MainImageName = fileName;
                //product.Images.Add(new ProductImage
                //{
                //    Url = fileName,
                //    IsPrimary = true
                //});
            }

            // تصاویر گالری
            if (model.GalleryImages != null && model.GalleryImages.Any())
            {
                foreach (var image in model.GalleryImages)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    product.Images.Add(new ProductImage
                    {
                        Url = fileName,
                        IsPrimary = false
                    });
                }
            }

            // دسته‌بندی‌ها
            foreach (var catId in model.SelectedCategories)
            {
                product.ProductCategories.Add(new ProductCategory
                {
                    CategoryId = catId
                });
            }


            _context.Products.Add(product);
            await _context.SaveChangesAsync();



            if (model.SpecKeys != null && model.SpecValues != null)
            {
                for (int i = 0; i < model.SpecKeys.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(model.SpecKeys[i]) &&
                        !string.IsNullOrWhiteSpace(model.SpecValues[i]))
                    {
                        _context.ProductSpecifications.Add(new ProductSpecification
                        {
                            ProductId = product.Id,
                            Key = model.SpecKeys[i],
                            Value = model.SpecValues[i]
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }



            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategories)
                .Include(p => p.Images)
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);


            if (product == null)
                return NotFound();

            var model = new ProductCreateViewModel
            {
                Title = product.Title,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                OldPrice = product.OldPrice,
                Stock = product.Stock,
                SelectedCategories = product.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                Categories = await _context.Categories.ToListAsync(),
                ExistingImages = product.Images.ToList(),
                SpecKeys = product.Specifications.Select(x => x.Key).ToList(),
                SpecValues = product.Specifications.Select(x => x.Value).ToList()

            };


            ViewBag.ProductId = product.Id;
            ViewBag.CurrentImage = product.MainImageName;

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCreateViewModel model)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategories)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.ToListAsync();
                ViewBag.ProductId = id;
                ViewBag.CurrentImage = product.MainImageName;
                return View(model);
            }

            product.Title = model.Title;
            product.Slug = model.Slug;
            product.Description = model.Description;
            product.Price = model.Price;
            product.OldPrice = model.OldPrice;
            product.Stock = model.Stock;


            string imagePathToDelete = null;
            if (model.MainImage != null)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/wp-content/uploads/2025/01");

                var fileName = Guid.NewGuid() + Path.GetExtension(model.MainImage.FileName);
                var filePath = Path.Combine(uploadsPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.MainImage.CopyToAsync(stream);
                if (!string.IsNullOrEmpty(product.MainImageName))
                {
                    imagePathToDelete = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/wp-content/uploads/2025/01", product.MainImageName);
                }
                product.MainImageName = fileName;
            }

            // ویرایش دسته‌بندی‌ها
            product.ProductCategories.Clear();
            foreach (var catId in model.SelectedCategories)
            {
                product.ProductCategories.Add(new ProductCategory
                {
                    CategoryId = catId
                });
            }


            // افزودن تصاویر جدید به گالری
            if (model.GalleryImages != null && model.GalleryImages.Any())
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/wp-content/uploads/2025/01");

                foreach (var image in model.GalleryImages)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    product.Images.Add(new ProductImage
                    {
                        Url = fileName,
                        IsPrimary = false
                    });
                }
            }

            var oldSpecs = _context.ProductSpecifications.Where(x => x.ProductId == product.Id);
            _context.ProductSpecifications.RemoveRange(oldSpecs);

            if (model.SpecKeys != null && model.SpecValues != null)
            {
                for (int i = 0; i < model.SpecKeys.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(model.SpecKeys[i]) &&
                        !string.IsNullOrWhiteSpace(model.SpecValues[i]))
                    {
                        _context.ProductSpecifications.Add(new ProductSpecification
                        {
                            ProductId = product.Id,
                            Key = model.SpecKeys[i],
                            Value = model.SpecValues[i]
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();




            if (imagePathToDelete != null && System.IO.File.Exists(imagePathToDelete))
            {
                try { System.IO.File.Delete(imagePathToDelete); }
                catch (IOException ioEx) { System.Diagnostics.Debug.WriteLine("Error deleting image: " + ioEx.Message); }
            }

            return RedirectToAction(nameof(Index));
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var product = _context.Products.Find(id);
            if (product== null)
            {
                return NotFound();
            }
            return View(product);
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                 .Include(p => p.ProductCategories).
                 Include(p => p.Specifications)
                 .Include(p=>p.Images)
                 .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                TempData["Message"] = "محصول مورد نظر یافت نشد.";
                TempData["IsSuccess"] = false;
                return RedirectToAction("Index");
            }

            string imagePathToDelete = null;
            
            if (!string.IsNullOrEmpty(product.MainImageName))
            {
                imagePathToDelete = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/wp-content/uploads/2025/01", product.MainImageName);
            }

            List<string> pathtoDelete = new List<string>();
            var productImages = await _context.ProductImages.Where(p=>p.ProductId==product.Id).ToListAsync();


            foreach(var item in productImages)
            {
                string path= Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/wp-content/uploads/2025/01", item.Url);
                if (!string.IsNullOrEmpty(path))
                {
                    pathtoDelete.Add(path);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            if (imagePathToDelete != null && System.IO.File.Exists(imagePathToDelete))
            {
                try { System.IO.File.Delete(imagePathToDelete); }
                catch (IOException ioEx) { System.Diagnostics.Debug.WriteLine("Error deleting image: " + ioEx.Message); }
            }
            foreach(var item in pathtoDelete)
            {
                try { System.IO.File.Delete(item); }
                catch (IOException ioEx) { System.Diagnostics.Debug.WriteLine("Error deleting image: " + ioEx.Message); }
            }

            TempData["Message"] = "محصول با موفقیت حذف شد.";
            TempData["IsSuccess"] = true;
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null)
                return NotFound();

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/wp-content/uploads/2025/01", image.Url);

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> SetMainImage(int id)
        {
            var image = await _context.ProductImages
                .Include(i => i.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (image == null)
                return NotFound();

            foreach (var img in image.Product.Images)
                img.IsPrimary = false;

            image.IsPrimary = true;

            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
