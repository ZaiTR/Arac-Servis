using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AracServis.Models
{
    public class Musteri
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = null!;
        public string? Telefon { get; set; }
        public string? EPosta { get; set; }
        public string? Adres { get; set; }
        public DateTime? CreatedAt { get; set; }
        
        // İlişki
        public virtual ICollection<Arac> Araclar { get; set; } = new List<Arac>();
    }
}