using System;

namespace AracServis.Models
{
    public class Odeme
    {
        public int Id { get; set; }

        public int ServisKaydiId { get; set; }
        public ServisKaydi? ServisKaydi { get; set; }

        public decimal OdenenTutar { get; set; }
        public string OdemeTipi { get; set; } = "Nakit";

        public DateTime OdemeTarihi { get; set; } = DateTime.Now;
    }
}
