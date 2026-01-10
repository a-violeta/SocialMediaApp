using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? PostId { get; set; }

        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Proprietati de navigatie: 2

        public ApplicationUser? User { get; set; }
        public Post? Post { get; set; }

        // CAMPURI NOI PENTRU ANALIZA DE SENTIMENT
        // Eticheta sentimentului: "positive", "neutral", "negative"
        //public string? SentimentLabel { get; set; }
        // Scorul de incredere: valoare intre 0.0 si 1.0
        //public double? SentimentConfidence { get; set; }
        // Data si ora la care s-a efectuat analiza
        //public DateTime? SentimentAnalyzedAt { get; set; }

    }
}
