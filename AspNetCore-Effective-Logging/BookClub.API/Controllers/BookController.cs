using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookClub.Data;
using BookClub.Entities;
using BookClub.Logic;
using BookClub.Logic.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BookClub.API.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository _bookRepo;
        private readonly IBookLogic _bookLogic;

        public BookController(IBookRepository bookRepo, IBookLogic bookLogic)
        {
            _bookRepo = bookRepo;
            _bookLogic = bookLogic;
        }

        [HttpGet]
        public async Task<IEnumerable<BookModel>> GetBooks(bool throwException = false)
        {
            //var userId = User.Claims.FirstOrDefault(a => a.Type == "sub")?.Value;
            //var oauth2Scopes = string.Join(',',
            //    User.Claims.Where(c => c.Type == "scope")?.Select(c => c.Value));

            Log
                //.ForContext("UserId", userId)
                //.ForContext("OAuth2Scopes", oauth2Scopes)
                .Information("API ENTRY: Inside get all books API call.");
            return await _bookLogic.GetAllBooks(throwException);
        }
        

        [HttpGet("{id}", Name = "Get")]
        public Book Get(int id)
        {
            return new Book();
        }
        
        [HttpPost]
        public void Post([FromBody] Book bookToSubmit)
        {
            var userId = Convert.ToInt32(User.Claims.FirstOrDefault(a => a.Type == "sub")?.Value);
            _bookRepo.SubmitNewBook(bookToSubmit, userId);
        }

        // PUT: api/Book/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
