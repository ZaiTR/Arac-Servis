namespace AracServis.Models
{
    public class Kullanici
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; } = null!;
        public string Sifre { get; set; } = null!;
        public string Rol { get; set; } = "Personel"; // Admin / Personel
    }
}
