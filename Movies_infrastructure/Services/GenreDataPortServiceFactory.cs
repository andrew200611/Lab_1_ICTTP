using Movies_domain.Model;

namespace Movies_infrastructure.Services
{
    public class GenreDataPortServiceFactory : IDataPortServiceFactory<Genre>
    {
        private const string ExcelContentType =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly Lab1dbContext _context;

        public GenreDataPortServiceFactory(Lab1dbContext context)
        {
            _context = context;
        }

        public IImportService<Genre> GetImportService(string contentType)
        {
            if (contentType is ExcelContentType)
                return new GenreImportService(_context);

            throw new NotImplementedException(
                $"Імпорт для типу '{contentType}' не реалізовано");
        }

        public IExportService<Genre> GetExportService(string contentType)
        {
            if (contentType is ExcelContentType)
                return new GenreExportService(_context);

            throw new NotImplementedException(
                $"Експорт для типу '{contentType}' не реалізовано");
        }
    }
}