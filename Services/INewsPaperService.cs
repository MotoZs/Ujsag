using Data;

namespace Services
{
    public interface INewsPaperService
    {
        Task<IEnumerable<Article>> GetAllArticlesAsync();
        Task<Article> GetArticleByIdAsync(int id);
        Task<Article> CreateArticleAsync(Article article);
        Task<bool> UpdateArticleAsync(Article article);
        Task<bool> DeleteArticleAsync(int id);
    }
}
