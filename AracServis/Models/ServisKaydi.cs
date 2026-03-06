using System;

using System.Collections.Generic;

namespace AracServis.Models
{
    public class ServisKaydi
    {
        public int Id { get; set; }

        public int RandevuId { get; set; }
        public Randevu? Randevu { get; set; }

        public int PersonelId { get; set; }
        public Personel? Personel { get; set; }

        public decimal IscilikTutari { get; set; }
        public decimal ParcaTutari { get; set; }

        public string Durum { get; set; } = "DevamEdiyor";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Odeme> Odemeler { get; set; } = new List<Odeme>();
    }
}
