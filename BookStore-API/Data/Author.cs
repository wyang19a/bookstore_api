using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore_API.Data
{
    // Data annotation
    [Table("Authors")]
    public partial class Author // make it partial
    {
        //id Firstname Lastname Bio
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Bio { get; set; }
        
        // Include list of Book 'class' and call it "Books"
        public virtual IList<Book> Books { get; set; }
    }
}