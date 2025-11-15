using Newtonsoft.Json.Linq;

namespace Fall2025_Project3_jma33.Models
{
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; }
        public IEnumerable<Movie> Movies { get; set; }
        public List<ActorTweetAndSentiment> TweetsandSents { get; set; }
        public string AvgSentiment { get; set; }

        public ActorDetailsViewModel(Actor actor, IEnumerable<Movie> movies, List<ActorTweetAndSentiment> actorTweetSents)
        {
            Actor = actor;
            Movies = movies;
            TweetsandSents = actorTweetSents;
            AvgSentiment = actorTweetSents.Average(r => r.Sentiment).ToString("F2");
        }
    }

    public class ActorTweetAndSentiment
    {
        public string Tweet_Username { get; set; }
        public string Tweet_Text { get; set; }
        public double Sentiment { get; set; }
        public string Sentiment_String { get; set; }
    }
}
