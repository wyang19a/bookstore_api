using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{

    /// <summary>
    /// Interacts with the Books Table
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class BooksController : ControllerBase
    {
        // First, inject necessary dependencies.
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BooksController(
                IBookRepository bookRepository,
                ILoggerService logger,
                IMapper mapper
            )
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Books
        /// </summary>
        /// <returns>A List of Books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo($"{location}: Successful");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Get One Book
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A Book Record</returns>
        [HttpGet("{id}")] // can be [HttpGet("{id:int}") to specify the type of param.
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call for ID: {id}");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn($"{location}: Failed to retrieve record with ID: {id}");
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                _logger.LogInfo($"{location}: Successfully got record with ID {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Create Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns>Create Book Object</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted POST");
                if (bookDTO == null)
                {
                    _logger.LogWarn($"{location}: Empty request submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Incomplete Data");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);
                if(!isSuccess)
                {
                    return InternalError($"{location}: POST Fail");
                }
                _logger.LogInfo($"{location}: POST Success");
                return Created("Create", new { book });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Update Book
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Success");
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    _logger.LogWarn($"{location}:Update failed with bad data");
                    return BadRequest();
                }
                var isExists = await _bookRepository.isExist(id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location}: with id: {id} was not found");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Update was incomplete");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Update failed");
                }
                _logger.LogInfo($"{location}: ID #{id} successfully updated.");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Remove Book
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            _logger.LogInfo($"{location}: delete attempted");
            try
            {
                if (id < 1)
                {
                    _logger.LogError($"{location}: with id is less than 1");
                    return BadRequest();
                }
                var isExists = await _bookRepository.isExist(id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location}: ID #{id} was not found");
                    return NotFound();
                }
                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Delete Failed");
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the Admin. Custom message");
        }
    }
}
