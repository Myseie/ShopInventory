using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShopInventory.Models;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using X.PagedList;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;


namespace ShopInventory.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Index (Listar Produkterna med Paginering och Sökfunktion)
        public async Task<IActionResult> Index(string searchString, int? page)
        {

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            // Skapa en sökbar lista över produkter
            var products = _context.Products.AsQueryable();

            // Filtrera produkterna om söksträngen inte är tom
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) || p.Category.Contains(searchString));
            }

            // Paginera de filtrerade produkterna
            var pagedProducts = products.ToPagedList(pageNumber, pageSize);

            // Spara nuvarande filter i ViewData
            ViewData["CurrentFilter"] = searchString;

            // Returnera produkterna till vyn
            return View(pagedProducts);

        }

        // GET: Create (Formulär för att skapa produkt)
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create (Sparar ny produkt inklusive bild)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Category")] Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Generera en unik filnamn
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    var filePath = Path.Combine(uploadDir, uniqueFileName);

                    // Spara bilden i wwwroot/images
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Sätt sökvägen till produktens bild
                    product.ImagePath = "/images/" + uniqueFileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(product);
        }


        // GET: Edit (Visa formulär för att redigera en produkt)
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();

            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Edit (Uppdaterar en produkt inklusive eventuell ny bild)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Category,ImagePath")] Product product, IFormFile? imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Hantera ny bilduppladdning
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(imageFile.FileName);
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // Uppdatera bildsökvägen
                        product.ImagePath = "/images/" + uniqueFileName;
                    }
                    else
                    {
                        // Behåll den gamla bilden om ingen ny laddas upp
                        var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                        if (existingProduct != null)
                        {
                            product.ImagePath = existingProduct.ImagePath;
                        }
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }



        // GET: Delete (Visa sida för att ta bort en produkt)
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Delete (Tar bort produkten)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}