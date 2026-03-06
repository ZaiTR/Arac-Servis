namespace AracServis.Models
{
    public class ServisParca
    {
        public int Id { get; set; }

        public int ServisKaydiId { get; set; }
        public ServisKaydi? ServisKaydi { get; set; }

        public int ParcaId { get; set; }
        public Parca? Parca { get; set; }

        public int Adet { get; set; }
        public decimal ToplamFiyat { get; set; }
    }
}
