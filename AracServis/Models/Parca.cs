namespace AracServis.Models
{
    public class Parca
    {
        public int ParcaID { get; set; }
        public string ParcaAdi { get; set; } = null!;
        public decimal BirimFiyat { get; set; }
        public decimal AlisFiyat { get; set; }
        public int Stok { get; set; }
        public string? IlgiliPozisyonlar { get; set; }
    }
}
