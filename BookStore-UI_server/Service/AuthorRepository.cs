using Blazored.LocalStorage;
using BookStore_UI_server.Contracts;
using BookStore_UI_server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookStore_UI_server.Service
{
    public class AuthorRepository : BaseRepository<Author>, IAuthorRepository // inheriting from matching context and also it's own contract
    {
        private readonly IHttpClientFactory _client;
        private readonly ILocalStorageService _store;
        public AuthorRepository(IHttpClientFactory client, ILocalStorageService store) : base(client, store)
        {
            _client = client;
            _store = store;
        }

    }
}
