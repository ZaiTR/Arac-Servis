using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AracServis.Models;
using AracServis.Data;

namespace AracServis.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ServisDbContext _context;

        // Gunluk saat slotlari
        private static readonly int[] SaatSlotlari = { 10, 12, 15 };
        private const int GunlukMaxRandevu = 3;

        public IndexModel(ServisDbContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        // AJAX: Plakaya göre son randevu durumunu sorgular
        public async Task<IActionResult> OnGetSorgulaAsync(string plaka)
        {
            if (string.IsNullOrWhiteSpace(plaka))
            {
                return new JsonResult(new { hata = "Lütfen plaka giriniz." });
            }

            var sonRandevu = await _context.Randevular
                .Include(r => r.Arac)
                .Where(r => r.Arac!.Plaka == plaka)
                .OrderByDescending(r => r.GirisTarihi)
                .FirstOrDefaultAsync();

            if (sonRandevu == null)
            {
                return new JsonResult(new { hata = "Bu plakaya ait aktif randevu bulunamadı." });
            }

            return new JsonResult(new { 
                plaka = sonRandevu.Arac?.Plaka,
                durum = sonRandevu.Durum,
                tarih = sonRandevu.GirisTarihi?.ToString("dd.MM.yyyy HH:mm"),
                ihtiyaclar = sonRandevu.Ihtiyaclar
            });
        }

        // AJAX: Secilen gune ait dolu slotlari dondurur
        public async Task<IActionResult> OnGetDoluSlotlarAsync(string tarih)
        {
            if (!DateTime.TryParse(tarih, out var secilenTarih))
            {
                return new JsonResult(new { hata = "Geçersiz tarih" });
            }

            var gunBaslangic = secilenTarih.Date;
            var gunBitis = gunBaslangic.AddDays(1);

            // O gune ait randevulari getir
            var randevular = await _context.Randevular
                .Where(r => r.GirisTarihi >= gunBaslangic && r.GirisTarihi < gunBitis)
                .ToListAsync();

            // Hangi saatler dolu
            var doluSaatler = randevular
                .Where(r => r.GirisTarihi.HasValue)
                .Select(r => r.GirisTarihi!.Value.Hour)
                .ToList();

            // Tum slotlarin durumunu dondur
            var slotlar = SaatSlotlari.Select(saat => new
            {
                saat = $"{saat:D2}:00",
                dolu = doluSaatler.Contains(saat)
            }).ToList();

            return new JsonResult(new { slotlar, toplamRandevu = randevular.Count });
        }

        public async Task<IActionResult> OnPostAsync(string adSoyad, string tel, string plaka, string servisTuru, DateTime tarih, string saat)
        {
            // === G\u00dcVENL\u0130K: Girdi dogrulama ===
            if (string.IsNullOrWhiteSpace(adSoyad) || adSoyad.Length > 100)
            {
                TempData["Message"] = "Ge\u00e7erli bir ad soyad giriniz (max 100 karakter).";
                TempData["Status"] = "error";
                return RedirectToPage("/Index");
            }
            if (string.IsNullOrWhiteSpace(tel) || tel.Length > 20)
            {
                TempData["Message"] = "Ge\u00e7erli bir telefon numaras\u0131 giriniz (max 20 karakter).";
                TempData["Status"] = "error";
                return RedirectToPage("/Index");
            }
            if (string.IsNullOrWhiteSpace(plaka) || plaka.Length > 20)
            {
                TempData["Message"] = "Ge\u00e7erli bir plaka giriniz (max 20 karakter).";
                TempData["Status"] = "error";
                return RedirectToPage("/Index");
            }

            // === DOGRULAMA ===

            // 1. Gecmis tarih kontrolu
            if (tarih.Date < DateTime.Today)
            {
                TempData["Message"] = "Geçmiş tarihe randevu alınamaz!";
                TempData["Status"] = "error";
                return RedirectToPage("/Index");
            }

            // 2. Saat slotu kontrolu
            if (string.IsNullOrEmpty(saat) || !int.TryParse(saat.Split(':')[0], out var saatDegeri)
                || !SaatSlotlari.Contains(saatDegeri))
            {
                TempData["Message"] = "Lütfen geçerli bir saat seçiniz!";
                TempData["Status"] = "error";
                return RedirectToPage("/Index");
            }

            // 3. Gunluk limit kontrolu
            var gunBaslangic = tarih.Date;
            var gunBitis = gunBaslangic.AddDays(1);
            var gunlukRandevuSayisi = await _context.Randevular
                .CountAsync(r => r.GirisTarihi >= gunBaslangic && r.GirisTarihi < gunBitis);

            if (gunlukRandevuSayisi >= GunlukMaxRandevu)
            {
                TempData["Message"] = "Bu gün için tüm randevu saatleri dolu!";
                TempData["Status"] = "error";
                return RedirectToPage("/Index");
            }

            // 4. Ayni saat slotu dolu mu kontrolu
            var randevuTarihi = tarih.Date.AddHours(saatDegeri);
            var slotDolu = await _context.Randevular
                .AnyAsync(r => r.GirisTarihi == randevuTarihi);

            if (slotDolu)
            {
                TempData["Message"] = $"Seçtiğiniz saat ({saat}) zaten dolu! Başka bir saat seçiniz.";
                TempData["Status"] = "error";
                return RedirectToPage("/Index");
            }

            // === KAYIT ISLEMI ===

            // 1. Müşteri Kontrolü
            var musteri = await _context.Musteriler.FirstOrDefaultAsync(m => m.Telefon == tel)
                          ?? new Musteri { AdSoyad = adSoyad, Telefon = tel, CreatedAt = DateTime.Now };

            if (musteri.Id == 0)
            {
                _context.Musteriler.Add(musteri);
                await _context.SaveChangesAsync();
            }

            // 2. Araç Kontrolü
            var arac = await _context.Araclar.FirstOrDefaultAsync(a => a.Plaka == plaka)
                       ?? new Arac
                       {
                           Plaka = plaka,
                           MusteriId = musteri.Id,
                           Marka = "Belirtilmedi",
                           Model = "Belirtilmedi",
                           CreatedAt = DateTime.Now
                       };

            if (arac.Id == 0)
            {
                _context.Araclar.Add(arac);
                await _context.SaveChangesAsync();
            }

            // 3. Randevu olustur (tarih + saat)
            var servisKaydi = new Randevu
            {
                AracId = arac.Id,
                GirisTarihi = randevuTarihi,
                Durum = "Bekliyor",
            };

            _context.Randevular.Add(servisKaydi);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{adSoyad}, {randevuTarihi:dd.MM.yyyy} tarihinde saat {saat} icin randevunuz olusturuldu.";
            TempData["Status"] = "success";

            return RedirectToPage("/Index");
        }
    }
}
