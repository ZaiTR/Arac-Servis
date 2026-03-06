using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AracServis.Data;

namespace AracServis.Pages
{
    public class PersonelLoginModel : PageModel
    {
        private readonly ServisDbContext _context;

        public PersonelLoginModel(ServisDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string KullaniciAdi { get; set; } = string.Empty;

        [BindProperty]
        public string Sifre { get; set; } = string.Empty;

        public string? HataMesaji { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(KullaniciAdi) || string.IsNullOrEmpty(Sifre))
            {
                HataMesaji = "Kullanici adi ve sifre giriniz.";
                return Page();
            }

            // Veritabanindan personel kontrolu (ad + sifre)
            var personel = await _context.Personeller
                .FirstOrDefaultAsync(p => p.ad == KullaniciAdi && p.sifre == Sifre);

            if (personel != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, $"{personel.ad} {personel.soyad}"),
                    new Claim(ClaimTypes.NameIdentifier, personel.id.ToString()),
                    new Claim(ClaimTypes.Role, "Personel"),
                    new Claim("PersonelId", personel.id.ToString()),
                    new Claim("Pozisyon", personel.pozisyon ?? "Personel")
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return RedirectToPage("/PersonelPanel");
            }

            HataMesaji = "Hatali kullanici adi veya sifre!";
            return Page();
        }
    }
}
