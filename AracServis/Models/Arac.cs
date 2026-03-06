namespace AracServis.Models
{
    public class Arac
    {
        public int Id { get; set; }
        public int MusteriId { get; set; }
        public string Plaka { get; set; } = null!;
        public string Marka { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int? Yil { get; set; }
        public string? SasiNo { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual Musteri Musteri { get; set; } = null!;
    }
}