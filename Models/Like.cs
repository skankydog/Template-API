namespace DatingApp.API.Models
{
    public class Like
    {
        public int LikerId { get; set; }
        public int LikeeId { get; set; }
        public virtual User Liker { get; set; } // need virtual when using lazy loading
        public virtual User Likee { get; set; } // need virtual when using lazy loading
    }
}