using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore_API.Data
{
    // Data annotation

    [Table("Books")]
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? Year { get; set; } // adding ? makes it Nullable (Entity Framework)
        public string Isbn { get; set; }
        public string Summary { get; set; }
        public string Image { get; set; }
        public double? Price { get; set; }
        public int? AuthorId { get; set; }
        public virtual Author Author { get; set; } // sets up reference, bringing in data from Author table.
    }
}