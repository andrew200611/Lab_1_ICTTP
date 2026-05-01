using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Movies_domain.Model;
using Movies_infrastructure;

namespace Movies_infrastructure.Controllers
{
    public class MoviesController : Controller
    {
        private readonly Lab1dbContext _context;

        public MoviesController(Lab1dbContext context)
        {
            _context = context;
        }

        private async Task<User?> GetOrCreateDefaultUserAsync(CancellationToken ct = default)
        {
            var user = await _context.Users.Include(u => u.FavMovies).FirstOrDefaultAsync(u => u.Id == 1, ct);
            if (user != null) return user;

            user = await _context.Users.Include(u => u.FavMovies).FirstOrDefaultAsync(u => u.UsEmail == "default@local", ct);
            if (user != null) return user;

            var newUser = new User
            {
                UsEmail = "default@local",
                UsName = "Default",
                UsPassword = null,
                UsRole = null
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(ct);

            user = await _context.Users.Include(u => u.FavMovies).FirstOrDefaultAsync(u => u.Id == newUser.Id, ct);
            return user;
        }

        // GET: Movies
        public async Task<IActionResult> Index(int? id, string? name)
        {
            ViewBag.GenreId = id;
            ViewBag.GenreName = name;

            IQueryable<Movie> query = _context.Movies
                .Include(m => m.Acts)
                .Include(m => m.Grs);

            if (id != null)
            {
                query = query.Where(m => m.Grs.Any(g => g.Id == id));
            }

            var movies = await query.ToListAsync();

            var favIds = new HashSet<int>();
            var favUser = await GetOrCreateDefaultUserAsync();
            if (favUser != null && favUser.FavMovies != null)
            {
                favIds = new HashSet<int>(favUser.FavMovies.Select(m => m.Id));
            }

            ViewBag.FavMovieIds = favIds;

            return View(movies);
        }

        // GET: Movies/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var movie = await _context.Movies
                .Include(m => m.Acts)
                .Include(m => m.Grs)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();

            var favUser = await GetOrCreateDefaultUserAsync();
            ViewBag.IsFavorite = favUser != null && favUser.FavMovies.Any(m => m.Id == movie.Id);

            return View(movie);
        }

        // GET: Movies/Create
        public async Task<IActionResult> Create(int genreId, string genreName)
        {
            ViewBag.GenreId = genreId;
            ViewBag.GenreName = genreName;
            ViewBag.Actors = new MultiSelectList(await _context.Actors.ToListAsync(), "Id", "ActName");
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int genreId, string genreName, [Bind("MvName,MvDescription,MvYear")] Movie movie, int[] selectedActors)
        {
            if (ModelState.IsValid)
            {
                var genre = await _context.Genres.FindAsync(genreId);
                if (genre != null)
                {
                    _context.Add(movie);
                    movie.Grs.Add(genre);
                }
                else
                {
                    _context.Add(movie);
                }

                if (selectedActors != null)
                {
                    foreach (var actorId in selectedActors)
                    {
                        var actor = await _context.Actors.FindAsync(actorId);
                        if (actor != null)
                        {
                            movie.Acts.Add(actor);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { id = genreId, name = genreName });
            }
            ViewBag.GenreId = genreId;
            ViewBag.GenreName = genreName;
            ViewBag.Actors = new MultiSelectList(await _context.Actors.ToListAsync(), "Id", "ActName");
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Acts)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            ViewBag.Actors = new MultiSelectList(await _context.Actors.ToListAsync(), "Id", "ActName", movie.Acts.Select(a => a.Id));
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MvName,MvDescription,MvYear,Id")] Movie movie, int[] selectedActors)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var movieToUpdate = await _context.Movies
                        .Include(m => m.Acts)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (movieToUpdate == null)
                    {
                        return NotFound();
                    }

                    movieToUpdate.MvName = movie.MvName;
                    movieToUpdate.MvDescription = movie.MvDescription;
                    movieToUpdate.MvYear = movie.MvYear;

                    movieToUpdate.Acts.Clear();
                    if (selectedActors != null)
                    {
                        foreach (var actorId in selectedActors)
                        {
                            var actor = await _context.Actors.FindAsync(actorId);
                            if (actor != null)
                            {
                                movieToUpdate.Acts.Add(actor);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
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
            ViewBag.Actors = new MultiSelectList(await _context.Actors.ToListAsync(), "Id", "ActName", selectedActors);
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie != null)
            {
                try
                {
                    _context.Movies.Remove(movie);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index), "Genres"); 
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Не можна видалити цей фільм, оскільки він має пов'язані дані, що забороняють видалення.");
                    return View(movie);
                }
            }

            return RedirectToAction(nameof(Index), "Genres");
        }

        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var user = await GetOrCreateDefaultUserAsync();

            var movies = (await _context.Movies
                .Where(m => user != null && user.FavMovies.Select(f => f.Id).Contains(m.Id))
                .Include(m => m.Acts)
                .Include(m => m.Grs)
                .ToListAsync())
                .Where(m => m != null)
                .ToList();

            return View(movies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFavorite(int id, string? returnUrl)
        {
            var user = await GetOrCreateDefaultUserAsync();
            if (user != null)
            {
                var movie = await _context.Movies.FindAsync(id);
                if (movie != null && !user.FavMovies.Any(m => m.Id == id))
                {
                    user.FavMovies.Add(movie);
                    await _context.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFavorite(int id, string? returnUrl)
        {
            var user = await GetOrCreateDefaultUserAsync();
            if (user != null)
            {
                var movie = user.FavMovies.FirstOrDefault(m => m.Id == id);
                if (movie != null)
                {
                    user.FavMovies.Remove(movie);
                    await _context.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
