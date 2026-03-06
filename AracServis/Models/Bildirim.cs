using System;

namespace AracServis.Models
{
    public class Bildirim
    {
        public int Id { get; set; }
        public int? ParcaId { get; set; }
        public Parca? Parca { get; set; }
        public int GonderenPersonelId { get; set; }
        public Personel? GonderenPersonel { get; set; }
        public string Mesaj { get; set; } = null!;
        public bool Okundu { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
