using System.Data;

namespace Common
{
    public record ArticleDto ()
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int AuthorId { get; set; }

        public AuthorDto Author { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
