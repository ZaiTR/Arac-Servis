using AracServis.Data;
using AracServis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AracServis.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelModel : PageModel
    {
        private readonly ServisDbContext _context;

        public AdminPanelModel(ServisDbContext context)
        {
            _context = context;
        }

        public string KullaniciAdi { get; set; } = "Admin";

        // Listeler
        public List<Personel> Personeller { get; set; } = new();
        public List<Randevu> Randevular { get; set; } = new();
        public List<Arac> Araclar { get; set; } = new();
        public List<ServisKaydi> ServisKayitlari { get; set; } = new();
        public List<Musteri> Musteriler { get; set; } = new();
        public List<Pozisyon> Pozisyonlar { get; set; } = new();

        // Dashboard istatistikleri
        public int ToplamRandevu { get; set; }
        public int BekleyenRandevu { get; set; }
        public int DevamEdenRandevu { get; set; }
        public int OnayliRandevu { get; set; }
        public int KontrolRandevu { get; set; }
        public int TamamlananRandevu { get; set; }
        public int ToplamArac { get; set; }
        public int ToplamMusteri { get; set; }
        public int ToplamServis { get; set; }
        public decimal ToplamGelir { get; set; }

        // Bind Properties
        [BindProperty]
        public Personel YeniPersonel { get; set; } = new Personel();

        [BindProperty]
        public string YeniRandevuPlaka { get; set; } = "";
        [BindProperty]
        public string YeniRandevuMarka { get; set; } = "";
        [BindProperty]
        public string YeniRandevuModel { get; set; } = "";
        [BindProperty]
        public int YeniRandevuMusteriId { get; set; }
        [BindProperty]
        public DateTime? YeniRandevuTarih { get; set; }
        [BindProperty]
        public int? YeniRandevuKm { get; set; }

        [BindProperty]
        public string YeniAracPlaka { get; set; } = "";
        [BindProperty]
        public string YeniAracMarka { get; set; } = "";
        [BindProperty]
        public string YeniAracModel { get; set; } = "";
        [BindProperty]
        public int? YeniAracYil { get; set; }
        [BindProperty]
        public string? YeniAracSasiNo { get; set; }
        [BindProperty]
        public int YeniAracMusteriId { get; set; }

        [BindProperty]
        public string YeniPozisyonAd { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true || !User.IsInRole("Admin"))
            {
                return RedirectToPage("/Admin/AdminLogin");
            }

            // Personeller
            Personeller = await _context.Personeller.ToListAsync();

            // Musteriler
            Musteriler = await _context.Musteriler.ToListAsync();

            // Pozisyonlar
            Pozisyonlar = await _context.Pozisyonlar.ToListAsync();

            // Araclar (musteri bilgisiyle)
            Araclar = await _context.Araclar
                .Include(a => a.Musteri)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            // Randevular (arac ve musteri bilgileriyle)
            Randevular = await _context.Randevular
                .Include(r => r.Arac)
                    .ThenInclude(a => a.Musteri)
                .OrderByDescending(r => r.GirisTarihi)
                .ToListAsync();

            // Servis Kayitlari
            ServisKayitlari = await _context.ServisKayitlari
                .Include(s => s.Randevu)
                    .ThenInclude(r => r!.Arac)
                        .ThenInclude(a => a!.Musteri)
                .Include(s => s.Personel)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            // Dashboard istatistikleri
            ToplamRandevu = Randevular.Count;
            BekleyenRandevu = Randevular.Count(r => r.Durum == "Bekliyor");
            DevamEdenRandevu = Randevular.Count(r => r.Durum == "DevamEdiyor");
            OnayliRandevu = Randevular.Count(r => r.Durum == "Onaylandi");
            KontrolRandevu = Randevular.Count(r => r.Durum == "Kontrol");
            TamamlananRandevu = Randevular.Count(r => r.Durum == "Tamamlandi");
            ToplamArac = Araclar.Count;
            ToplamMusteri = Musteriler.Count;
            ToplamServis = ServisKayitlari.Count;
            ToplamGelir = await _context.Odemeler.SumAsync(o => o.OdenenTutar);

            return Page();
        }

        // === PERSONEL ===
        public async Task<IActionResult> OnPostEkleAsync()
        {
            if (YeniPersonel != null)
            {
                YeniPersonel.ise_giris_tarihi = DateTime.Now;
                YeniPersonel.durum = "Aktif";
                _context.Personeller.Add(YeniPersonel);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Personel başarıyla eklendi.";
                TempData["Status"] = "success";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSilAsync(int id)
        {
            var personel = await _context.Personeller.FindAsync(id);
            if (personel != null)
            {
                _context.Personeller.Remove(personel);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Personel silindi.";
                TempData["Status"] = "success";
            }
            return RedirectToPage();
        }

        // === RANDEVU ===
        public async Task<IActionResult> OnPostRandevuEkleAsync()
        {
            // Güvenlik: Zorunlu alan kontrolleri
            if (string.IsNullOrWhiteSpace(YeniRandevuPlaka) || YeniRandevuPlaka.Length > 20)
                return RedirectToPage();
            if (YeniRandevuMarka.Length > 50 || YeniRandevuModel.Length > 50)
                return RedirectToPage();

            // Plakaya gore arac var mi kontrol et
            var arac = await _context.Araclar
                .FirstOrDefaultAsync(a => a.Plaka == YeniRandevuPlaka);

            if (arac == null)
            {
                // Yeni arac olustur
                arac = new Arac
                {
                    Plaka = YeniRandevuPlaka,
                    Marka = YeniRandevuMarka,
                    Model = YeniRandevuModel,
                    MusteriId = YeniRandevuMusteriId,
                    CreatedAt = DateTime.Now
                };
                _context.Araclar.Add(arac);
                await _context.SaveChangesAsync();
            }

            var randevu = new Randevu
            {
                AracId = arac.Id,
                GirisTarihi = YeniRandevuTarih ?? DateTime.Now,
                Km = YeniRandevuKm,
                Durum = "Bekliyor"
            };

            _context.Randevular.Add(randevu);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Randevu oluşturuldu.";
            TempData["Status"] = "success";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRandevuSilAsync(int randevuId)
        {
            var randevu = await _context.Randevular.FindAsync(randevuId);
            if (randevu != null)
            {
                var servisKayitlari = _context.ServisKayitlari.Where(s => s.RandevuId == randevuId);
                _context.ServisKayitlari.RemoveRange(servisKayitlari);
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // === ARAC ===
        public async Task<IActionResult> OnPostAracEkleAsync()
        {
            // Güvenlik: Zorunlu alan ve uzunluk kontrolleri
            if (string.IsNullOrWhiteSpace(YeniAracPlaka) || YeniAracPlaka.Length > 20)
                return RedirectToPage();
            if (YeniAracMarka.Length > 50 || YeniAracModel.Length > 50)
                return RedirectToPage();

            var arac = new Arac
            {
                Plaka = YeniAracPlaka,
                Marka = YeniAracMarka,
                Model = YeniAracModel,
                Yil = YeniAracYil,
                SasiNo = YeniAracSasiNo,
                MusteriId = YeniAracMusteriId,
                CreatedAt = DateTime.Now
            };

            _context.Araclar.Add(arac);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Araç başarıyla eklendi.";
            TempData["Status"] = "success";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAracSilAsync(int aracId)
        {
            var arac = await _context.Araclar.FindAsync(aracId);
            if (arac != null)
            {
                // Iliskili randevulari ve servis kayitlarini kontrol et
                var randevular = _context.Randevular.Where(r => r.AracId == aracId);
                foreach (var r in randevular)
                {
                    var sk = _context.ServisKayitlari.Where(s => s.RandevuId == r.Id);
                    _context.ServisKayitlari.RemoveRange(sk);
                }
                _context.Randevular.RemoveRange(randevular);
                _context.Araclar.Remove(arac);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // === POZISYON ===
        public async Task<IActionResult> OnPostPozisyonEkleAsync()
        {
            if (!string.IsNullOrWhiteSpace(YeniPozisyonAd))
            {
                var pozisyon = new Pozisyon { Ad = YeniPozisyonAd };
                _context.Pozisyonlar.Add(pozisyon);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Pozisyon başarıyla eklendi.";
                TempData["Status"] = "success";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostPozisyonSilAsync(int id)
        {
            var pozisyon = await _context.Pozisyonlar.FindAsync(id);
            if (pozisyon != null)
            {
                _context.Pozisyonlar.Remove(pozisyon);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // === LOGOUT ===
        public async Task<IActionResult> OnPostLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Admin/AdminLogin");
        }
    }
}