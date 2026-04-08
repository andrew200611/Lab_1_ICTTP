using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Movies_domain.Model;

namespace Movies_infrastructure.Services
{
    public class GenreImportService : IImportService<Genre>
    {
        private readonly Lab1dbContext _context;

        public GenreImportService(Lab1dbContext context)
        {
            _context = context;
        }

        public async Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Дані не можуть бути прочитані", nameof(stream));
            }

            using (XLWorkbook workBook = new XLWorkbook(stream))
            {
                foreach (IXLWorksheet worksheet in workBook.Worksheets)
                {
                    
                    var genreName = worksheet.Name;
                    var genre = await _context.Genres
                        .FirstOrDefaultAsync(g => g.GrName == genreName, cancellationToken);

                    if (genre == null)
                    {
                        genre = new Genre { GrName = genreName };
                        _context.Genres.Add(genre);
                    }

                    
                    foreach (var row in worksheet.RowsUsed().Skip(1))
                    {
                        await AddMovieAsync(row, cancellationToken, genre);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task AddMovieAsync(IXLRow row, CancellationToken cancellationToken, Genre genre)
        {
            var movieName = GetMovieName(row);
            if (string.IsNullOrWhiteSpace(movieName)) return;

            var movie = await _context.Movies
                .Include(m => m.Grs)
                .Include(m => m.Acts)
                .FirstOrDefaultAsync(m => m.MvName == movieName, cancellationToken);

            if (movie == null)
            {
                movie = new Movie
                {
                    MvName = movieName,
                    MvYear = GetMovieYear(row),
                    MvDescription = GetMovieDescription(row),
                };
                _context.Movies.Add(movie);
            }
            else
            {
                movie.MvYear = GetMovieYear(row);
                movie.MvDescription = GetMovieDescription(row);
            }

            if (!movie.Grs.Any(g => g.Id == genre.Id || g.GrName == genre.GrName))
            {
                movie.Grs.Add(genre);
            }

            await GetActorsAsync(row, movie, cancellationToken);
        }

        private static string GetMovieName(IXLRow row)
            => row.Cell(1).Value.ToString();

        private static int? GetMovieYear(IXLRow row)
        {
            var raw = row.Cell(2).Value.ToString();
            return int.TryParse(raw, out var year) ? year : null;
        }

        private static string? GetMovieDescription(IXLRow row)
        {
            var val = row.Cell(3).Value.ToString();
            return string.IsNullOrWhiteSpace(val) ? null : val;
        }

        private async Task GetActorsAsync(IXLRow row, Movie movie, CancellationToken cancellationToken)
        {

            for (int i = 4; i <= 13; i++)
            {
                var actorName = row.Cell(i).Value.ToString().Trim();
                if (string.IsNullOrWhiteSpace(actorName)) continue;

                var actor = await _context.Actors
                    .FirstOrDefaultAsync(a => a.ActName == actorName, cancellationToken);

                if (actor == null)
                {
                    actor = new Actor { ActName = actorName };
                    _context.Actors.Add(actor);
                }

                if (!movie.Acts.Any(a => a.Id == actor.Id || a.ActName == actorName))
                {
                    movie.Acts.Add(actor);
                }
            }
        }
    }
}
