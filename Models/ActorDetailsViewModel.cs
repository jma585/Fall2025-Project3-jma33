namespace Fall2025_Project3_jma33.Models
{
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; }
        public IEnumerable<Movie> Movies { get; set; }

        public ActorDetailsViewModel(Actor actor, IEnumerable<Movie> movies)
        {
            Actor = actor;
            Movies = movies;
        }
    }
}
