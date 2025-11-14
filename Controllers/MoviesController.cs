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
        //private readonly ApiKeyCredential _apiCredential;
        //private readonly string _apiEndpoint;
        //private readonly string _aiDeployment;

        private static readonly Uri ApiEndpoint = new("");
        private static readonly ApiKeyCredential ApiCredential = new("");
        private const string AiDeployment = "";

        public MoviesController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            //var ApiKey = _config["AZURE_OPENAI_API_KEY"] ?? throw new InvalidOperationException("API key in Azure does not exist in the current Configuration");
            //_apiEndpoint = "https://fall2025-aif-eastus2.cognitiveservices.azure.com/";
            //_aiDeployment = "gpt-4.1-nano";
            //_apiCredential = new ApiKeyCredential(ApiKey);
        }

        private async Task<List<MovieReviewAndSentiment>> CreateMovieReviewsSentiment(string movieTitle, int movieYear)
        {
            var reviews_and_sents = new List<MovieReviewAndSentiment>();
            Console.WriteLine("Asking reviewers...");

            ChatClient client = new AzureOpenAIClient(ApiEndpoint, ApiCredential).GetChatClient(AiDeployment);

            string[] personas = { "is harsh", "loves romance", "loves comedy", "loves thrillers", "loves fantasy", "always wanted to be an actor", "enjoys the outdoors", "has watched many movies", "is a creative author", "is a director who has won many awards" };
            var messages = new ChatMessage[]
            {
            new SystemChatMessage($"You are an experienced film critic who has watched hundreds of movies and provide heartfelt, honest reviews. You love stories that are not the most predictable, but you normally provide different reviews from different points of view."),
            new UserChatMessage($"Generate 10 reviews where each review consists of a score out of 10 and a short description for the movie {movieTitle} released in {movieYear}. For the description, use more than 50 words but less than 100 words and describe aspects of the movie that you loved/hated and whether you would watch it again or recommend it to others. After every description except the last, add a '|'. Don't number the reviews.")
            //$" represent a group of {personas.Length} film critics who have the following personalities: {string.Join(",", personas)}. When you receive a question, create a response for each member of the group with each response separated by a '|', totalling {personas.Length} responses, but don't indicate which member you are."),
            };
            ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);
            Console.WriteLine(result.Value.Content);
            string[] reviews = result.Value.Content[0].Text.Split('|').Select(s => s.Trim()).ToArray();
            Console.WriteLine(reviews[0]);
            //Console.WriteLine(reviews[9]);
            var analyzer = new SentimentIntensityAnalyzer();

            foreach (var singleReview in reviews)
            {
                SentimentAnalysisResults sentiment = analyzer.PolarityScores(singleReview);

                reviews_and_sents.Add(new MovieReviewAndSentiment
                {
                    Review = singleReview,
                    Sentiment = sentiment.Compound,
                });
            }

            //for (int i = 0; i < reviews.Length; i++)
            //{
            //    string review = reviews[i];
            //    SentimentAnalysisResults sentiment = analyzer.PolarityScores(review);
            //    sentimentTotal += sentiment.Compound;

            //    Console.WriteLine($"Review {i + 1} (sentiment {sentiment.Compound})");
            //    Console.WriteLine(review);
            //    Console.WriteLine();
            //}

            //Console.Write($"#####\n# Sentiment Average: {sentimentAverage:#.###}\n#####\n");

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
            Console.WriteLine("About to create MovieDetailsViewModel");
            var vm = new MovieDetailsViewModel(movie, actors, reviewsAndSents);

            Console.WriteLine("Created MovieDetailsViewModel");

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
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model State is invalid for creation");
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
