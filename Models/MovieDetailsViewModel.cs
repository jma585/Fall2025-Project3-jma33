namespace Fall2025_Project3_jma33.Models
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; }
        public IEnumerable<Actor> Actors { get; set; }
        public List<MovieReviewAndSentiment> ReviewsandSents { get; set; }
        public double AvgSentiment { get; set; }

        public MovieDetailsViewModel(Movie movie, IEnumerable<Actor> actors, List<MovieReviewAndSentiment> movieRevSents)
        {
            Movie = movie;
            Actors = actors;
            ReviewsandSents = movieRevSents;
            AvgSentiment = movieRevSents.Average(r => r.Sentiment);
        }
    }

    public class MovieReviewAndSentiment
    {
        public string Review { get; set; }
        public double Sentiment { get; set; }
    }
}
