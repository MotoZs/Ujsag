using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class AuthorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ArticleDto> Articles { get; set; }
    }
}
