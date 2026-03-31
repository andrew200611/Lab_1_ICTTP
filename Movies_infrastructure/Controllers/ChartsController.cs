using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movies_domain.Model;
using Movies_infrastructure;

namespace Movies_infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        private readonly Lab1dbContext _context;

        public ChartsController(Lab1dbContext context)
        {
            _context = context;
        }

       
        private record CountByYearItem(string Year, int Count);

        [HttpGet("countByYear")]
        public async Task<JsonResult> GetCountByYear([FromQuery] int? genreId, CancellationToken ct)
        {
            var query = _context.Movies.AsQueryable();

            if (genreId.HasValue)
            {
                query = query.Where(m => m.Grs.Any(g => g.Id == genreId.Value));
            }

            var data = await query
                .Where(m => m.MvYear != null)
                .GroupBy(m => m.MvYear)
                .OrderBy(g => g.Key)
                .Select(g => new CountByYearItem(g.Key.ToString()!, g.Count()))
                .ToListAsync(ct);

            return new JsonResult(data);
        }

        
        private record CountByGenreItem(string Genre, int Count);

        [HttpGet("countByGenre")]
        public async Task<JsonResult> GetCountByGenre(CancellationToken ct)
        {
            var data = await _context.Genres
                .OrderByDescending(g => g.Mvs.Count)
                .Select(g => new CountByGenreItem(g.GrName, g.Mvs.Count))
                .ToListAsync(ct);

            return new JsonResult(data);
        }
    }
}