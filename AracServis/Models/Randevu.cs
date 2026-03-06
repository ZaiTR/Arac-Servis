namespace AracServis.Models
{
    public class Randevu
    {
        public int Id { get; set; }
        public int AracId { get; set; }
        public DateTime? GirisTarihi { get; set; }
        public DateTime? CikisTarihi { get; set; }
        public int? Km { get; set; }
        public decimal? ToplamUcret { get; set; }
        public string? Durum { get; set; }
        public string? Ihtiyaclar { get; set; }

        public virtual Arac Arac { get; set; } = null!;
        public virtual ServisKaydi? ServisKaydi { get; set; }
    }
}