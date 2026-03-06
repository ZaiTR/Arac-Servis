using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using AracServis.Models;
using AracServis.Data;

namespace AracServis.Pages
{
    public class AccountingSummary
    {
        public decimal ToplamGelir { get; set; }
        public decimal ToplamGiderParca { get; set; }
        public decimal ToplamGiderMaas { get; set; }
        public decimal ToplamEkstraGider { get; set; }
        public decimal NetKar => ToplamGelir - (ToplamGiderParca + ToplamGiderMaas + ToplamEkstraGider);
    }

    public class PersonelPanelModel : PageModel
    {
        private readonly ServisDbContext _context;

        public static readonly string[] UzmanRoller = { "Kaportacı", "Boyacı", "Elektrikçi", "Mekanik", "Yıkamacı" };

        public PersonelPanelModel(ServisDbContext context)
        {
            _context = context;
        }

        public List<Randevu> Randevular { get; set; } = new();
        public List<ServisKaydi> ServisKayitlari { get; set; } = new();
        public List<Parca> Parcalar { get; set; } = new();
        public List<Bildirim> Bildirimler { get; set; } = new();
        public string PersonelAdi { get; set; } = "";
        public int PersonelId { get; set; }
        public string PersonelPozisyon { get; set; } = "Personel";

        // Muhasebe verileri
        public AccountingSummary MuhasebeOzet { get; set; } = new();
        public List<Odeme> MuhasebeOdemeler { get; set; } = new();
        public List<Personel> MuhasebePersoneller { get; set; } = new();
        public List<EkstraGider> EkstraGiderler { get; set; } = new();
        public List<ServisKaydi> BekleyenOdemeler { get; set; } = new();

        public string KullaniciMesaji { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public DateTime? BaslangicTarihi { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? BitisTarihi { get; set; }

        [BindProperty]
        public Parca YeniParca { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/PersonelLogin");
            }

            PersonelAdi = User.Identity?.Name ?? "Personel";

            var personelIdClaim = User.FindFirst("PersonelId")?.Value;
            if (personelIdClaim != null)
            {
                PersonelId = int.Parse(personelIdClaim);
            }

            PersonelPozisyon = User.FindFirst("Pozisyon")?.Value ?? "Personel";

            // Is Akisi:
            // Bekliyor -> [Danisman Onayla] -> Onaylandi -> [Usta Basla] -> DevamEdiyor -> [Usta Tamamla] -> Kontrol -> [Danisman Tamamla] -> Tamamlandi

            if (PersonelPozisyon == "Danışman")
            {
                // Danisman: Bekliyor + Kontrol randevularini gorur
                Randevular = await _context.Randevular
                    .Where(r => r.Durum == "Bekliyor" || r.Durum == "Kontrol")
                    .Include(r => r.Arac)
                        .ThenInclude(a => a.Musteri)
                    .OrderByDescending(r => r.GirisTarihi)
                    .ToListAsync();
            }
            else if (PersonelPozisyon == "Usta")
            {
                // Usta: Onaylandi + DevamEdiyor randevularini gorur
                Randevular = await _context.Randevular
                    .Where(r => r.Durum == "Onaylandi" || r.Durum == "DevamEdiyor")
                    .Include(r => r.Arac)
                        .ThenInclude(a => a.Musteri)
                    .OrderByDescending(r => r.GirisTarihi)
                    .ToListAsync();

                // Usta ayrica kendi servis kayitlarini gorur
                if (PersonelId > 0)
                {
                    ServisKayitlari = await _context.ServisKayitlari
                        .Where(s => s.PersonelId == PersonelId)
                        .Include(s => s.Randevu)
                            .ThenInclude(r => r!.Arac)
                        .OrderByDescending(s => s.CreatedAt)
                        .ToListAsync();
                }
            }
            else if (PersonelPozisyon == "Depocu")
            {
                // Depocu: Tum parcalari/malzemeleri gorur
                Parcalar = await _context.Parcalar
                    .OrderBy(p => p.ParcaAdi)
                    .ToListAsync();

                // Bildirimleri gorur (okunmamislar)
                Bildirimler = await _context.Bildirimler
                    .Where(b => !b.Okundu)
                    .Include(b => b.Parca)
                    .Include(b => b.GonderenPersonel)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();
            }
            else if (UzmanRoller.Contains(PersonelPozisyon))
            {
                // Uzman Roller: Sadece kendi pozisyonlariyla ilgili parcaları gorurler
                Parcalar = await _context.Parcalar
                    .Where(p => p.IlgiliPozisyonlar != null && p.IlgiliPozisyonlar.Contains(PersonelPozisyon))
                    .OrderBy(p => p.ParcaAdi)
                    .ToListAsync();

                // Kendi randevularini gorur (Usta gibi)
                Randevular = await _context.Randevular
                    .Where(r => r.Durum == "Onaylandi" || r.Durum == "DevamEdiyor")
                    .Include(r => r.Arac)
                        .ThenInclude(a => a.Musteri)
                    .Include(r => r.ServisKaydi) // Servis kaydini da getir
                    .OrderByDescending(r => r.GirisTarihi)
                    .ToListAsync();
            }
            else if (PersonelPozisyon == "Muhasebe")
            {
                // Varsayılan tarih aralığı (Ayın başından bugüne)
                BaslangicTarihi ??= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                BitisTarihi ??= DateTime.Now;

                // Muhasebe: Finansal verileri hazirla
                var odemeSorgu = _context.Odemeler
                    .Include(o => o.ServisKaydi)
                        .ThenInclude(s => s!.Randevu)
                            .ThenInclude(r => r!.Arac)
                    .Where(o => o.OdemeTarihi >= BaslangicTarihi && o.OdemeTarihi <= BitisTarihi);

                MuhasebeOdemeler = await odemeSorgu.OrderByDescending(o => o.OdemeTarihi).ToListAsync();

                MuhasebePersoneller = await _context.Personeller
                    .OrderBy(p => p.ad)
                    .ToListAsync();

                EkstraGiderler = await _context.EkstraGiderler
                    .Where(e => e.Tarih >= BaslangicTarihi && e.Tarih <= BitisTarihi)
                    .OrderByDescending(e => e.Tarih)
                    .ToListAsync();

                // Finansal Ozet Hesaplama
                MuhasebeOzet.ToplamGelir = MuhasebeOdemeler.Sum(o => o.OdenenTutar);
                MuhasebeOzet.ToplamGiderMaas = MuhasebePersoneller.Sum(p => p.maas ?? 0);
                MuhasebeOzet.ToplamEkstraGider = EkstraGiderler.Sum(e => e.Tutar);

                // Parca Gideri (COGS): Secili tarih araliginda TAMAMLANAN servislerin parca maliyetleri
                MuhasebeOzet.ToplamGiderParca = await _context.ServisParcalari
                    .Include(sp => sp.Parca)
                    .Include(sp => sp.ServisKaydi)
                    .Where(sp => sp.ServisKaydi != null && sp.ServisKaydi.CreatedAt >= BaslangicTarihi && sp.ServisKaydi.CreatedAt <= BitisTarihi)
                    .SumAsync(sp => (sp.Parca != null ? sp.Parca.AlisFiyat : 0) * sp.Adet);

                // Odeme Bekleyen Servisler: Tamamlanmis ama toplam odemesi iscilik+parca tutarindan az olanlar
                // Basitlik adina: Odeme kaydi hic olmayan veya eksik olan servis kayitlari
                var tumServisler = await _context.ServisKayitlari
                    .Include(s => s.Randevu).ThenInclude(r => r!.Arac).ThenInclude(a => a!.Musteri)
                    .Include(s => s.Odemeler)
                    .Where(s => s.Randevu != null && s.Randevu.Durum == "Tamamlandi")
                    .ToListAsync();

                BekleyenOdemeler = tumServisler
                    .Where(s => s.Odemeler.Sum(o => o.OdenenTutar) < (s.IscilikTutari + (s.ParcaTutari)))
                    .ToList();
            }
            else
            {
                // Diger roller: tum randevular + servis kayitlari
                Randevular = await _context.Randevular
                    .Include(r => r.Arac)
                        .ThenInclude(a => a.Musteri)
                    .OrderByDescending(r => r.GirisTarihi)
                    .ToListAsync();

                if (PersonelId > 0)
                {
                    ServisKayitlari = await _context.ServisKayitlari
                        .Where(s => s.PersonelId == PersonelId)
                        .Include(s => s.Randevu)
                            .ThenInclude(r => r!.Arac)
                        .OrderByDescending(s => s.CreatedAt)
                        .ToListAsync();
                }

                KullaniciMesaji = $"Hoş geldiniz. Sistemde '{PersonelPozisyon}' olarak tanımlısınız. Aşağıdaki listeden genel randevu durumlarını takip edebilirsiniz.";
            }

            return Page();
        }

        // Randevu durumunu guncelle
        public async Task<IActionResult> OnPostDurumGuncelleAsync(int randevuId, string yeniDurum)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Danışman" && pozisyon != "Usta" && !UzmanRoller.Contains(pozisyon))
            {
                return RedirectToPage();
            }

            // Güvenlik: Sadece izin verilen durum geçişlerine izin ver
            var izinliDurumlar = new[] { "Onaylandi", "DevamEdiyor", "Kontrol", "Tamamlandi" };
            if (!izinliDurumlar.Contains(yeniDurum))
            {
                return RedirectToPage();
            }

            var randevu = await _context.Randevular.FindAsync(randevuId);
            if (randevu != null)
            {
                randevu.Durum = yeniDurum;

                if (yeniDurum == "Tamamlandi")
                {
                    randevu.CikisTarihi = DateTime.Now;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Usta: Arac bilgilerini duzenle ve ise basla
        public async Task<IActionResult> OnPostAracDuzenleVeBaslaAsync(int randevuId, int? km, string? sasiNo, int? yil, List<string>? ihtiyaclar)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Usta") return RedirectToPage();

            var randevu = await _context.Randevular
                .Include(r => r.Arac)
                .FirstOrDefaultAsync(r => r.Id == randevuId);

            if (randevu != null)
            {
                // Ihtiyaclari virgulle birlestirip kaydet
                if (ihtiyaclar != null && ihtiyaclar.Any())
                {
                    randevu.Ihtiyaclar = string.Join(",", ihtiyaclar);
                }
                else
                {
                    randevu.Ihtiyaclar = null;
                }
                // Randevu km guncelle
                if (km.HasValue)
                {
                    randevu.Km = km;
                }

                // Arac bilgilerini guncelle
                if (randevu.Arac != null)
                {
                    if (!string.IsNullOrEmpty(sasiNo))
                    {
                        randevu.Arac.SasiNo = sasiNo;
                    }
                    if (yil.HasValue)
                    {
                        randevu.Arac.Yil = yil;
                    }
                }

                // Durumu DevamEdiyor yap
                randevu.Durum = "DevamEdiyor";

                // === YENİ: Servis Kaydı Oluştur (Eğer yoksa) ===
                var mevcutKayit = await _context.ServisKayitlari.FirstOrDefaultAsync(s => s.RandevuId == randevuId);
                if (mevcutKayit == null)
                {
                    var personelIdClaim = User.FindFirst("PersonelId")?.Value;
                    int pId = personelIdClaim != null ? int.Parse(personelIdClaim) : 0;

                    var yeniKayit = new ServisKaydi
                    {
                        RandevuId = randevuId,
                        PersonelId = pId,
                        Durum = "DevamEdiyor",
                        CreatedAt = DateTime.Now
                    };
                    _context.ServisKayitlari.Add(yeniKayit);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Parca Ekle (Servis Kaydına)
        public async Task<IActionResult> OnPostServisParcaEkleAsync(int servisId, int parcaId, int adet)
        {
            if (adet <= 0) return RedirectToPage();

            var servis = await _context.ServisKayitlari.FindAsync(servisId);
            var parca = await _context.Parcalar.FindAsync(parcaId);

            if (servis != null && parca != null && parca.Stok >= adet)
            {
                var servisParca = new ServisParca
                {
                    ServisKaydiId = servisId,
                    ParcaId = parcaId,
                    Adet = adet,
                    ToplamFiyat = parca.BirimFiyat * adet
                };

                // Stok dus
                parca.Stok -= adet;

                // Servis Kaydi parca tutarini guncelle
                servis.ParcaTutari += servisParca.ToplamFiyat;

                _context.ServisParcalari.Add(servisParca);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // Odeme Ekle (Muhasebe)
        public async Task<IActionResult> OnPostOdemeEkleAsync(int servisId, decimal tutar, string tip)
        {
            if (tutar <= 0) return RedirectToPage();

            var servis = await _context.ServisKayitlari.FindAsync(servisId);
            if (servis != null)
            {
                var odeme = new Odeme
                {
                    ServisKaydiId = servisId,
                    OdenenTutar = tutar,
                    OdemeTipi = tip,
                    OdemeTarihi = DateTime.Now
                };

                _context.Odemeler.Add(odeme);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // Iscilik Guncelle (Danisman)
        public async Task<IActionResult> OnPostIscilikGuncelleAsync(int servisId, decimal iscilik)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Danışman") return RedirectToPage();

            var servis = await _context.ServisKayitlari.FindAsync(servisId);
            if (servis != null)
            {
                servis.IscilikTutari = iscilik;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // Randevu sil
        public async Task<IActionResult> OnPostRandevuSilAsync(int randevuId)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Danışman") return RedirectToPage();

            var randevu = await _context.Randevular.FindAsync(randevuId);
            if (randevu != null)
            {
                // Iliskili servis kayitlarini da sil
                var servisKayitlari = _context.ServisKayitlari.Where(s => s.RandevuId == randevuId);
                _context.ServisKayitlari.RemoveRange(servisKayitlari);

                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Randevu duzenle
        public async Task<IActionResult> OnPostRandevuDuzenleAsync(int randevuId, string musteriAd, string plaka, string marka, string model, string? girisTarihi)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Danışman" && pozisyon != "Usta") return RedirectToPage();

            var randevu = await _context.Randevular
                .Include(r => r.Arac)
                    .ThenInclude(a => a.Musteri)
                .FirstOrDefaultAsync(r => r.Id == randevuId);

            if (randevu != null)
            {
                // Musteri adini guncelle
                if (randevu.Arac?.Musteri != null && !string.IsNullOrEmpty(musteriAd))
                {
                    randevu.Arac.Musteri.AdSoyad = musteriAd;
                }

                // Arac bilgilerini guncelle
                if (randevu.Arac != null)
                {
                    if (!string.IsNullOrEmpty(plaka))
                        randevu.Arac.Plaka = plaka;
                    if (!string.IsNullOrEmpty(marka))
                        randevu.Arac.Marka = marka;
                    if (!string.IsNullOrEmpty(model))
                        randevu.Arac.Model = model;
                }

                // Giris tarihini guncelle
                if (!string.IsNullOrEmpty(girisTarihi) && DateTime.TryParse(girisTarihi, out var parsedDate))
                {
                    randevu.GirisTarihi = parsedDate;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Parca ekle (Depocu)
        public async Task<IActionResult> OnPostParcaEkleAsync(List<string>? pozisyonlar)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Depocu") return RedirectToPage();

            if (YeniParca != null && !string.IsNullOrEmpty(YeniParca.ParcaAdi))
            {
                if (pozisyonlar != null && pozisyonlar.Any())
                {
                    YeniParca.IlgiliPozisyonlar = string.Join(",", pozisyonlar);
                }
                _context.Parcalar.Add(YeniParca);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // Parca duzenle (Depocu)
        public async Task<IActionResult> OnPostParcaDuzenleAsync(int parcaId, string parcaAdi, decimal birimFiyat, decimal alisFiyat, int stok, List<string>? pozisyonlar)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Depocu") return RedirectToPage();

            var parca = await _context.Parcalar.FindAsync(parcaId);
            if (parca != null)
            {
                parca.ParcaAdi = parcaAdi;
                parca.BirimFiyat = birimFiyat;
                parca.AlisFiyat = alisFiyat;
                parca.Stok = stok;

                if (pozisyonlar != null && pozisyonlar.Any())
                {
                    parca.IlgiliPozisyonlar = string.Join(",", pozisyonlar);
                }
                else
                {
                    parca.IlgiliPozisyonlar = null;
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // Parca sil (Depocu)
        public async Task<IActionResult> OnPostParcaSilAsync(int parcaId)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Depocu") return RedirectToPage();

            var parca = await _context.Parcalar.FindAsync(parcaId);
            if (parca != null)
            {
                _context.Parcalar.Remove(parca);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // Stok Bildir (Uzman Roller)
        public async Task<IActionResult> OnPostStokBildirAsync(int parcaId)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (!UzmanRoller.Contains(pozisyon)) return RedirectToPage();

            var personelIdClaim = User.FindFirst("PersonelId")?.Value;
            if (personelIdClaim == null) return RedirectToPage();

            int pId = int.Parse(personelIdClaim);
            var parca = await _context.Parcalar.FindAsync(parcaId);

            if (parca != null)
            {
                var bildirim = new Bildirim
                {
                    ParcaId = parcaId,
                    GonderenPersonelId = pId,
                    Mesaj = $"{parca.ParcaAdi} malzemesi tükenmiş veya kritik seviyede! (Bildiren: {User.Identity?.Name})",
                    CreatedAt = DateTime.Now,
                    Okundu = false
                };
                _context.Bildirimler.Add(bildirim);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Stok bildirimi depoya iletildi.";
                TempData["Status"] = "success";
            }
            return RedirectToPage();
        }

        // Bildirim Oku (Depocu)
        public async Task<IActionResult> OnPostBildirimOkuAsync(int bildirimId)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Depocu") return RedirectToPage();

            var bildirim = await _context.Bildirimler.FindAsync(bildirimId);
            if (bildirim != null)
            {
                bildirim.Okundu = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // Ekstra Gider Ekle
        public async Task<IActionResult> OnPostEkstraGiderEkleAsync(string kategori, decimal tutar, string? aciklama, string? tarih)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Muhasebe") return RedirectToPage();

            // Güvenlik: Sadece izin verilen kategorilere izin ver
            var izinliKategoriler = new[] { "Kira", "Elektrik", "Su/Doğalgaz", "Yemek/Mutfak", "Reklam", "Diğer" };
            if (!izinliKategoriler.Contains(kategori))
            {
                return RedirectToPage();
            }

            // Güvenlik: Tutar pozitif olmali
            if (tutar <= 0)
            {
                return RedirectToPage();
            }

            // Güvenlik: Aciklama uzunluk sınırı
            if (aciklama != null && aciklama.Length > 500)
            {
                aciklama = aciklama.Substring(0, 500);
            }

            var gider = new EkstraGider
            {
                Kategori = kategori,
                Tutar = tutar,
                Aciklama = aciklama,
                Tarih = !string.IsNullOrEmpty(tarih) && DateTime.TryParse(tarih, out var parsedTarih)
                    ? parsedTarih
                    : DateTime.Now
            };

            _context.EkstraGiderler.Add(gider);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Gider başarıyla kaydedildi.";
            TempData["Status"] = "success";
            return RedirectToPage();
        }

        // Ekstra Gider Sil
        public async Task<IActionResult> OnPostEkstraGiderSilAsync(int id)
        {
            var pozisyon = User.FindFirst("Pozisyon")?.Value;
            if (pozisyon != "Muhasebe") return RedirectToPage();

            var gider = await _context.EkstraGiderler.FindAsync(id);
            if (gider != null)
            {
                _context.EkstraGiderler.Remove(gider);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // Cikis yap
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/PersonelLogin");
        }
    }
}