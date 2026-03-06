using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace AracServis.Pages.Admin
{
    public class AdminLoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public AdminLoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // DÜZELTME 1: Başlangıç değeri olarak boş string ("") atadık.
        // Böylece "Bu alan null geldi" hatası vermeyecek.
        [BindProperty]
        public string KullaniciAdi { get; set; } = string.Empty;

        // DÜZELTME 2: Şifre için de aynısını yaptık.
        [BindProperty]
        public string Sifre { get; set; } = string.Empty;

        [BindProperty]
        public bool BeniHatirla { get; set; }

        // DÜZELTME 3: Hata mesajı başta olmadığı için 'string?' yaparak
        // null olabileceğini belirttik.
        public string? Hata { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(KullaniciAdi) || string.IsNullOrEmpty(Sifre))
            {
                Hata = "Lütfen kullanıcı adı ve şifre giriniz.";
                return Page();
            }

            // Kimlik bilgilerini appsettings.json'dan oku
            var gecerliKullaniciAdi = _configuration["AdminCredentials:KullaniciAdi"];
            var gecerliSifre = _configuration["AdminCredentials:Sifre"];

            if (KullaniciAdi == gecerliKullaniciAdi && Sifre == gecerliSifre)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, KullaniciAdi),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = BeniHatirla,
                    ExpiresUtc = BeniHatirla ? DateTimeOffset.UtcNow.AddDays(30) : (DateTimeOffset?)null
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties
                );

                return RedirectToPage("/Admin/AdminPanel"); // Yönlendirme
            }

            Hata = "Hatalı kullanıcı adı veya şifre!";
            return Page();
        }
    }
}