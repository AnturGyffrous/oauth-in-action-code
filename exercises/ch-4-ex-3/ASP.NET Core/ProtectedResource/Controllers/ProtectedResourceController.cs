using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProtectedResource.Controllers
{
    [ApiController]
    [Authorize]
    public class ProtectedResourceController : Controller
    {
        private static readonly Stack<string> SavedWords = new Stack<string>();

        [HttpPost("words")]
        [Authorize(Roles = "write")]
        public StatusCodeResult AddWord([FromForm] string word)
        {
            SavedWords.Push(word);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpDelete("words")]
        [Authorize(Roles = "delete")]
        public StatusCodeResult DeleteWord()
        {
            SavedWords.Pop();
            return NoContent();
        }

        [HttpGet("words")]
        [Authorize(Roles = "read")]
        public object GetWords() =>
            new
            {
                words = string.Join(" ", SavedWords),
                timestamp = (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds
            };

        [AllowAnonymous]
        [HttpGet("")]
        public IActionResult Index() => View();

        [HttpPost("resource")]
        public object Resource() => new
            { name = "Protected Resource", description = "This data has been protected by OAuth 2.0" };
    }
}