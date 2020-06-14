using BookStore_API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Contracts
{
    public interface IAuthorRepository : IRepositoryBase<Author> // not using T in this case because we know which data class it is relative to.
                                                                 // T is used when unknown.
    {
        // "These are just declarations"
    }
}
// Contracts will define, Services will inherit from Contract.