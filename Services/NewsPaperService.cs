using Data;
using Microsoft.EntityFrameworkCore;
using Services;

public class NewsPaperService : INewsPaperService
{
    private readonly NewspaperDbContext _context;

    public NewsPaperService(NewspaperDbContext context)
    {
        _context = context;
    }

    // ========== Article Methods ==========

    public async Task<IEnumerable<Article>> GetAllArticlesAsync()
    {
        return await _context.Articles
            .ToListAsync();
    }

    public async Task<Article> GetArticleByIdAsync(int id)
    {
        return await _context.Articles
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Article> CreateArticleAsync(Article article)
    {
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        return await GetArticleByIdAsync(article.Id);
    }

    public async Task<bool> UpdateArticleAsync(Article article)
    {
        var existing = await _context.Articles.FindAsync(article.Id);
        if (existing == null) return false;

        existing.Title = article.Title;
        existing.Description = article.Description;
        existing.AuthorId = article.AuthorId;

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

    public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
    {
        return await _context.Authors
            .ToListAsync();
    }

    public async Task<Author> GetAuthorByIdAsync(int id)
    {
        return await _context.Authors
            .Include(a => a.Articles)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Author> CreateAuthorAsync(Author author)
    {
        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        return await GetAuthorByIdAsync(author.Id);
    }
}