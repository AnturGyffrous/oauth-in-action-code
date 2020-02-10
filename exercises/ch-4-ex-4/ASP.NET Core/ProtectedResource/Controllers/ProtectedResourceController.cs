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

        [HttpGet("produce")]
        public object Produce()
        {
            return new
            {
                fruit = User.IsInRole("fruit")
                    ? new[] { "apple", "banana", "kiwi" }
                    : Array.Empty<string>(),
                veggies = User.IsInRole("veggies")
                    ? new[] { "lettuce", "onion", "potato" }
                    : Array.Empty<string>(),
                meats = User.IsInRole("meats")
                    ? new[] { "bacon", "steak", "chicken breast" }
                    : Array.Empty<string>()
            };
        }

        [HttpGet("favorites")]
        public object Favorites()
        {
            var aliceFavorites = new
            {
                movies = new[] { "The Multidmensional Vector", "Space Fights", "Jewelry Boss" },
                foods = new[] { "bacon", "pizza", "bacon pizza" },
                music = new[] { "techno", "industrial", "alternative" }
            };

            var bobFavorites = new
            {
                movies = new[] { "An Unrequited Love", "Several Shades of Turquoise", "Think Of The Children" },
                foods = new[] { "bacon", "kale", "gravel" },
                music = new[] { "baroque", "ukulele", "baroque ukulele" }
            };

            switch (User.Identity.Name)
            {
                case "alice":
                    return new { user = "Alice", favorites = aliceFavorites };
                case "bob":
                    return new { user = "Bob", favorites = bobFavorites };
                default:
                    return new
                    {
                        user = "Unknown",
                        favorites = new
                        {
                            movies = Array.Empty<string>(),
                            foods = Array.Empty<string>(),
                            music = Array.Empty<string>()
                        }
                    };
            }
        }

        [HttpPost("resource")]
        public object Resource() => new
            { name = "Protected Resource", description = "This data has been protected by OAuth 2.0" };
    }
}