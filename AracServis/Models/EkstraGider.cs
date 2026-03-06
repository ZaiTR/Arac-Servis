using System.ComponentModel.DataAnnotations;

namespace AracServis.Models
{
    public class EkstraGider
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Kategori { get; set; } = string.Empty; // Kira, Fatura, Maaş Dışı Gider vb.

        [Required]
        public decimal Tutar { get; set; }

        [Required]
        public DateTime Tarih { get; set; } = DateTime.Now;

        public string? Aciklama { get; set; }
    }
}
