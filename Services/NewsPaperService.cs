using Data;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class NewsPaperService : INewsPaperService
    {
        private readonly NewspaperDbContext _context;

        public NewsPaperService(NewspaperDbContext context)
        {
            _context = context;
        }

        // ========== Article Methods ==========

        public async Task<IEnumerable<ArticleDto>> GetAllArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .OrderByDescending(a => a.CreatedDate)
                .Select(a => new ArticleDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    AuthorId = a.AuthorId,
                    AuthorName = a.Author.Name,
                    CreatedDate = a.CreatedDate,
                    UpdatedDate = a.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<ArticleDto> GetArticleByIdAsync(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null) return null;

            return new ArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                Description = article.Description,
                AuthorId = article.AuthorId,
                AuthorName = article.Author.Name,
                CreatedDate = article.CreatedDate,
                UpdatedDate = article.UpdatedDate
            };
        }

        public async Task<ArticleDto> CreateArticleAsync(CreateArticleDto dto)
        {
            var article = new Article
            {
                Title = dto.Title,
                Description = dto.Description,
                AuthorId = dto.AuthorId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return await GetArticleByIdAsync(article.Id);
        }

        public async Task<bool> UpdateArticleAsync(int id, UpdateArticleDto dto)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return false;

            article.Title = dto.Title;
            article.Description = dto.Description;
            article.AuthorId = dto.AuthorId;
            article.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return false;

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }

        // ========== Author Methods ==========

        public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync()
        {
            return await _context.Authors
                .Select(a => new AuthorDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    ArticleCount = a.Articles.Count
                })
                .ToListAsync();
        }

        public async Task<AuthorDto> GetAuthorByIdAsync(int id)
        {
            var author = await _context.Authors
                .Where(a => a.Id == id)
                .Select(a => new AuthorDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    ArticleCount = a.Articles.Count
                })
                .FirstOrDefaultAsync();

            return author;
        }

        public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto dto)
        {
            var author = new Author
            {
                Name = dto.Name
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return await GetAuthorByIdAsync(author.Id);
        }
    }
}
