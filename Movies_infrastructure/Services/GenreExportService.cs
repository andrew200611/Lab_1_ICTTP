using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Movies_domain.Model;

namespace Movies_infrastructure.Services
{
    public class GenreExportService : IExportService<Genre>
    {
        private static readonly IReadOnlyList<string> HeaderNames = new string[]
        {
            "Назва фільму",
            "Рік",
            "Опис",
            "Актор 1",
            "Актор 2",
            "Актор 3",
            "Актор 4",
            "Актор 5",
            "Актор 6",
            "Актор 7",
            "Актор 8",
            "Актор 9",
            "Актор 10",
        };

        private readonly Lab1dbContext _context;

        public GenreExportService(Lab1dbContext context)
        {
            _context = context;
        }

        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (!stream.CanWrite)
                throw new ArgumentException("Потік не підтримує запис", nameof(stream));

            var genres = await _context.Genres
                .Include(g => g.Mvs)
                    .ThenInclude(m => m.Acts)
                .ToListAsync(cancellationToken);

            var workbook = new XLWorkbook();
            WriteGenres(workbook, genres);
            workbook.SaveAs(stream);
        }

        private void WriteGenres(XLWorkbook workbook, IList<Genre> genres)
        {
            foreach (var genre in genres)
            {
                var worksheet = workbook.Worksheets.Add(genre.GrName);
                WriteHeader(worksheet);
                WriteMovies(worksheet, genre.Mvs.ToList());
            }
        }

        private static void WriteHeader(IXLWorksheet worksheet)
        {
            for (int i = 0; i < HeaderNames.Count; i++)
                worksheet.Cell(1, i + 1).Value = HeaderNames[i];
            worksheet.Row(1).Style.Font.Bold = true;
        }

        private static void WriteMovies(IXLWorksheet worksheet, IList<Movie> movies)
        {
            int rowIndex = 2;
            foreach (var movie in movies)
            {
                WriteMovie(worksheet, movie, rowIndex);
                rowIndex++;
            }
        }

        private static void WriteMovie(IXLWorksheet worksheet, Movie movie, int rowIndex)
        {
            worksheet.Cell(rowIndex, 1).Value = movie.MvName;
            worksheet.Cell(rowIndex, 2).Value = movie.MvYear?.ToString() ?? "";
            worksheet.Cell(rowIndex, 3).Value = movie.MvDescription ?? "";

            var actors = movie.Acts.Take(10).ToList();
            for (int i = 0; i < actors.Count; i++)
                worksheet.Cell(rowIndex, 4 + i).Value = actors[i].ActName;
        }
    }
}