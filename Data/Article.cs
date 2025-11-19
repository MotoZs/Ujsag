using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Data
{
    public class Article
    {

        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int AuthorId { get; set; }

        public Author Author { get; set; }
    }
}
