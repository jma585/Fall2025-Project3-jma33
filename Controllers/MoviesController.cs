using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_jma33.Data;
using Fall2025_Project3_jma33.Models;
using System.Numerics;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using VaderSharp2;

namespace Fall2025_Project3_jma33.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        private readonly ApiKeyCredential _apiCredential;
        private readonly Uri _apiEndpoint;
        private const string AiDeployment = "gpt-4.1-nano";

        public MoviesController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;

            var apiKey = _config["ApiCredential"] ?? throw new InvalidOperationException("API credential in Azure does not exist in the current Configuration");
            var apiEndpoint = _config["ApiEndpoint"] ?? throw new InvalidOperationException("API endpoint in Azure does not exist in the current Configuration");

            _apiCredential = new ApiKeyCredential(apiKey);
            _apiEndpoint = new Uri(apiEndpoint);
        }

        private async Task<List<MovieReviewAndSentiment>> CreateMovieReviewsSentiment(string movieTitle, int movieYear)
        {
            var reviews_and_sents = new List<MovieReviewAndSentiment>();
            ChatClient client = new AzureOpenAIClient(_apiEndpoint, _apiCredential).GetChatClient(AiDeployment);

            var messages = new ChatMessage[]
            {
            new SystemChatMessage($"You are an experienced film critic who has watched hundreds of movies and provide heartfelt, honest reviews. You love stories that are not the most predictable, but you normally provide different reviews from different points of view."),
            new UserChatMessage($"Generate 10 reviews where each review consists of a score out of 10 and a short description for the movie {movieTitle} released in {movieYear}. For the description, use more than 50 words but less than 100 words and describe aspects of the movie that you loved/hated and whether you would watch it again or recommend it to others. After every description, add a '|'. Don't number the reviews.")
            };
            ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);
            
            string[] reviews = result.Value.Content[0].Text.Split('|').Select(s => s.Trim()).ToArray();
            
            var analyzer = new SentimentIntensityAnalyzer();

            //foreach (var singleReview in reviews)
            for (int i = 0; i < 10; i++)
            {
                var singleReview = reviews[i];
                SentimentAnalysisResults sentiment = analyzer.PolarityScores(singleReview);

                reviews_and_sents.Add(new MovieReviewAndSentiment
                {
                    Review = singleReview,
                    Sentiment = sentiment.Compound,
                    Sentiment_String = sentiment.Compound.ToString("F2")
                });
            }

            return reviews_and_sents;
        }

        public async Task<IActionResult> Poster(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var movie = await _context.Movie.FindAsync(id);

            if (movie == null || movie.Poster == null)
            {
                return NotFound();
            }

            return File(movie.Poster, "image/jpg");
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            var actors = await _context.MovieActors
                .Include(m => m.Actor)
                .Where(m => m.MovieId == movie.Id)
                .Select(m => m.Actor!)
                .ToListAsync();

            var reviewsAndSents = await CreateMovieReviewsSentiment(movie.Title, movie.Year);
            
            var vm = new MovieDetailsViewModel(movie, actors, reviewsAndSents);

            return View(vm);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IMDBLink,Genre,Year")] Movie movie, IFormFile? Poster)
        {
            if (ModelState.IsValid)
            {
                if (Poster != null && Poster.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await Poster.CopyToAsync(memoryStream);
                    movie.Poster = memoryStream.ToArray();
                }
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IMDBLink,Genre,Year")] Movie movie, IFormFile? Poster)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (Poster != null && Poster.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await Poster.CopyToAsync(memoryStream);
                        movie.Poster = memoryStream.ToArray();
                    }
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
