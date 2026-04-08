using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movies_domain.Model;
using Movies_infrastructure;
using Movies_infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movies_infrastructure.Controllers
{
    public class GenresController : Controller
    {
        private readonly Lab1dbContext _context;

        private readonly IDataPortServiceFactory<Genre> _genreDataPortServiceFactory;

        public GenresController(Lab1dbContext context, IDataPortServiceFactory<Genre> genreDataPortServiceFactory)
        {
            _context = context;
            _genreDataPortServiceFactory = genreDataPortServiceFactory;
        }

        [HttpGet]
        public IActionResult Import() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel, CancellationToken cancellationToken = default)
        {
            if (fileExcel == null || fileExcel.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Будь ласка, оберіть файл для завантаження.");
                return View();
            }

            var importService = _genreDataPortServiceFactory.GetImportService(fileExcel.ContentType);
            using var stream = fileExcel.OpenReadStream();
            await importService.ImportFromStreamAsync(stream, cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearDatabase(CancellationToken cancellationToken = default)
        {
         
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Movies\", \"Genres\", \"Actors\", \"Reviews\", \"Users\" CASCADE;", cancellationToken);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken cancellationToken = default,
            [FromQuery] string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            var exportService = _genreDataPortServiceFactory.GetExportService(contentType);
            var memoryStream = new MemoryStream();
            await exportService.WriteToAsync(memoryStream, cancellationToken);
            await memoryStream.FlushAsync(cancellationToken);
            memoryStream.Position = 0;
            return new FileStreamResult(memoryStream, contentType)
            {
                FileDownloadName = $"genres_{DateTime.UtcNow:yyyy-MM-dd}.xlsx"
            };
        }



        // GET: Genres
        public async Task<IActionResult> Index()
        {
            return View(await _context.Genres.ToListAsync());
        }

        // GET: Genres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var genre = await _context.Genres.FirstOrDefaultAsync(m => m.Id == id);
            if (genre == null) return NotFound();
            return RedirectToAction("Index", "Movies", new { id = genre.Id, name = genre.GrName });
        }

        // GET: Genres/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Genres/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GrName,Id")] Genre genre)
        {
            if (ModelState.IsValid)
            {
                _context.Add(genre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(genre);
        }

        // GET: Genres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }
            return View(genre);
        }

        // POST: Genres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GrName,Id")] Genre genre)
        {
            if (id != genre.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(genre);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenreExists(genre.Id))
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
            return View(genre);
        }

        // GET: Genres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genre = await _context.Genres
                .FirstOrDefaultAsync(m => m.Id == id);
            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }

        // POST: Genres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre != null)
            {
                try
                {
                    _context.Genres.Remove(genre);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Не можна видалити цей жанр, оскільки він використовується у фільмах. Спочатку відв'яжіть його від відповідних фільмів.");
                    return View(genre);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool GenreExists(int id)
        {
            return _context.Genres.Any(e => e.Id == id);
        }
    }
}
