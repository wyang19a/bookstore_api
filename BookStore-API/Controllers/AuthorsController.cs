using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        // dependency injection
        private readonly IAuthorRepository _authorRepository;
        // include logger service
        private readonly ILoggerService _logger;
        // include iMapper
        private readonly IMapper _mapper;

        public AuthorsController(
                IAuthorRepository authorRepository,
                ILoggerService logger,
                IMapper mapper
            )
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }
        /// <summary>
        /// GET all authors
        /// </summary>
        /// <returns>List Of Authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors() // GetAuthors() is just assigned here.
        {
            try
            {
                _logger.LogInfo("Attempted Get All Authors. Custom message can use interpolations(dollar,curly braces) too");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<List<AuthorDTO>>(authors);//DTO is like a serializer in RoR. control what is shown.
                _logger.LogInfo("Successfully got all Authors! Custom message");
                return Ok(response); // return 200 OK and send in the mapped data.\
            }
            catch (Exception e)
            {
                //_logger.LogError($"{e.Message} - {e.InnerException}");
                //return StatusCode(500, "Something went wrong. Please contact the Admin. Custom message");
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// GET one author
        /// </summary>
        /// /// <returns>One Author</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted Get Author id: {id} ");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"Author id: {id} was not found.");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo("Successfully got the author");
                return Ok(response);
            }
            catch (Exception e)
            {
                //_logger.LogError($"{e.Message} - {e.InnerException}");
                //return StatusCode(500, "Something went wrong. Please contact the Admin. Custom message");
                // above two lines of code is becoming repetitive. so I'm just gonna make a  private method that will do the same, call
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// CREATE
        /// </summary>
        /// <param name="author"></param>
        /// <returns> CREATE Author </returns>
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO) // expecting some kind of [Body] from client. Id, First name, Last name, Bio, List of books.
                                                                            // now go look at the AuthorDTO.cs, where we defined AuthorCreateDTO
        {
            try
            {
                _logger.LogInfo($"Author submission attempted");
                if (authorDTO == null)
                {
                    _logger.LogWarn($"Empty Request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"Author data was incomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);
                if (!isSuccess)
                {
                    return InternalError($"Author Creation Failed");
                }
                _logger.LogInfo("Author Created");
                return Created("Create", new { author });
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Update 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO) // Create and Update often share the same DTO function. some devs call it Upsert (Update + insert)
                                                                                              // in case you need to implement logic such as you only allow `bio` field update, but not `firstname`, and `lastname`
                                                                                              // you will need to define new DTO function for that.
                                                                                              // after attempt, we decide we need new DTO just for update because Id is needed for update.
        {
            try
            {
                _logger.LogInfo($"Author Update Attempted - id: {id}");
                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogWarn("Author Update failed with bad data");
                    return BadRequest();
                }
                var isExists = await _authorRepository.isExist(id);
                if (!isExists)
                {
                    _logger.LogWarn($"Author with id: {id} was not found");
                    return NotFound();
                }
                if(!ModelState.IsValid)
                {
                    _logger.LogWarn($"Author Update was incomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);
                if (!isSuccess)
                {
                    return InternalError($"Update failed");
                }
                _logger.LogInfo($"Author with id: {id} successfully updated.");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Remove Author
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInfo($"Author delete attempted");
            try
            {
                if (id < 1)
                {
                    _logger.LogError($"Author with id is less than 1");
                    return BadRequest();
                }
                var isExists = await _authorRepository.isExist(id);
                if (!isExists)
                {
                    _logger.LogWarn($"Author with id: {id} was not found");
                    return NotFound();
                }
                var author = await _authorRepository.FindById(id);
                var isSuccess = await _authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"Delete Failed");
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the Admin. Custom message");
        }
    }
}
