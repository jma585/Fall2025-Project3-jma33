using Azure.AI.OpenAI;
using Fall2025_Project3_jma33.Data;
using Fall2025_Project3_jma33.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using VaderSharp2;

namespace Fall2025_Project3_jma33.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        private readonly ApiKeyCredential _apiCredential;
        private readonly Uri _apiEndpoint;
        private const string AiDeployment = "gpt-4.1-nano";

        public ActorsController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;

            var apiKey = _config["ApiCredential"] ?? throw new InvalidOperationException("API credential in Azure does not exist in the current Configuration");
            var apiEndpoint = _config["ApiEndpoint"] ?? throw new InvalidOperationException("API endpoint in Azure does not exist in the current Configuration");

            _apiCredential = new ApiKeyCredential(apiKey);
            _apiEndpoint = new Uri(apiEndpoint);
        }

        private async Task<List<ActorTweetAndSentiment>> CreateActorTweetsSentiment(string actorName)
        {
            var tweets_and_sents = new List<ActorTweetAndSentiment>();
            ChatClient client = new AzureOpenAIClient(_apiEndpoint, _apiCredential).GetChatClient(AiDeployment);

            var messages = new ChatMessage[]
            {
            new SystemChatMessage($"You represent the Twitter social media platform. Generate realistic tweets about actors that could appear on social media and entertainment news. Each answer should be a valid JSON formatted array of objects containing the tweet and username. The response should start with [."),
            new UserChatMessage($"Generate 20 different tweets from a variety of users about the actor {actorName}. For the tweet content, include a mix of comments about their work, public events, social media, or general news. Keep each tweet content under 50 words.")
            };
            ClientResult<ChatCompletion> result = await client.CompleteChatAsync(messages);

            string tweetsJsonString = result.Value.Content.FirstOrDefault()?.Text ?? "[]";
            JsonArray json = JsonNode.Parse(tweetsJsonString)!.AsArray();

            var analyzer = new SentimentIntensityAnalyzer();

            var tweets = json.Select(t => new { Username = t!["username"]?.ToString() ?? "", Text = t!["tweet"]?.ToString() ?? "" }).ToArray();
            
            foreach (var tweet in tweets)
            {
                SentimentAnalysisResults sentiment = analyzer.PolarityScores(tweet.Text);
                tweets_and_sents.Add(new ActorTweetAndSentiment
                {
                    Tweet_Username = tweet.Username,
                    Tweet_Text = tweet.Text,
                    Sentiment = sentiment.Compound,
                    Sentiment_String = sentiment.Compound.ToString("F2")
                });
            }
            return tweets_and_sents;
        }

        public async Task<IActionResult> Photo(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var actor = await _context.Actor.FirstOrDefaultAsync(s => s.Id == id);

            if (actor == null || actor.Photo == null)
            {
                return NotFound();
            }

            return File(actor.Photo, "image/jpg");
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            var movies = await _context.MovieActors
                .Include(m => m.Movie)
                .Where(m => m.ActorId == actor.Id)
                .Select(m => m.Movie!)
                .ToListAsync();

            var tweetsAndSents = await CreateActorTweetsSentiment(actor.Name);
            
            var vm = new ActorDetailsViewModel(actor, movies, tweetsAndSents);

            return View(vm);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,IMDBLink")] Actor actor, IFormFile? Photo)
        {
            if (ModelState.IsValid)
            {
                if (Photo != null && Photo.Length > 0)
                {
                    using var stream = new MemoryStream();
                    await Photo.CopyToAsync(stream);
                    actor.Photo = stream.ToArray();
                }
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,IMDBLink")] Actor actor)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
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
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
