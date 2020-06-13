using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.DTOs
{
    public class AuthorDTO
    {
        //id Firstname Lastname Bio
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Bio { get; set; }

        // Include list of Book 'class' and call it "Books"
        public virtual IList<BookDTO> Books { get; set; }
    }
}
