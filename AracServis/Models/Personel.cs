using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AracServis.Models
{
    [Table("personeller")]
    public class Personel
    {
        [Key]
        public int id { get; set; }
        public string? ad { get; set; }
        public string? soyad { get; set; }
        public string? telefon { get; set; }
        public string? email { get; set; }
        public string? pozisyon { get; set; }
        public decimal? maas { get; set; }
        public DateTime? ise_giris_tarihi { get; set; }

        // HATAYI ÇÖZEN DEĞİŞİKLİK: bool yerine string yaptık
        public string? durum { get; set; }

        public string? sifre { get; set; }
    }
}