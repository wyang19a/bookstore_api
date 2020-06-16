using BookStore_API.Contracts;
using BookStore_API.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Services
{
    // when you inherit from IAuthorRepository which is inheriting from BASE repository,
    // and `show potential fixes` and implement interface,
    // it will generate the code for you relative to what is defined in the BASE repository.
    public class AuthorRepository : IAuthorRepository
    {
        // enable interacting with db
        private readonly ApplicationDbContext _db;
        // initialize with cnostructor : ctor [tab][tab]
        public AuthorRepository(ApplicationDbContext db)
        {
            _db = db; //"Dependency injection"
        } // through this, we can use the object _db anywhere in the `application`?
        public async Task<bool> Create(Author entity)
        {
            await _db.Authors.AddAsync(entity);
            return await Save(); // calling my own async function.
        }

        public async Task<bool> Delete(Author entity)
        {
            _db.Authors.Remove(entity);
            return await Save();
        }

        public async Task<IList<Author>> FindAll()
        {
            var authors = await _db.Authors
                .Include(q => q.Books)
                .ToListAsync();
            return authors;
        }

        public async Task<Author> FindById(int id)
        {
            var author = await _db.Authors
                .Include(q => q.Books)
                .FirstOrDefaultAsync(q => q.Id == id);
            return author;
        }

        public async Task<bool> isExist(int id)
        {
            return await _db.Authors.AnyAsync(q => q.Id == id);
        }

        public async Task<bool> Save()
        {
            //var changes = _db.SaveChangesAsync();
            //return changes > 0; 
            // here's a funky error with async functions. 
            // `changes` in the definition was type Task<int> and the `changes` being returned is just int.
            // to fix this, add await in front of var changes = definition. it will tell you to make the method async.
            var changes =  await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(Author entity)
        {
            _db.Authors.Update(entity);
            return await Save(); // again, here calling my own function
        }
    }
}
