using Microsoft.EntityFrameworkCore;
using AracServis.Models;

namespace AracServis.Data
{
    public class ServisDbContext : DbContext
    {
        public ServisDbContext(DbContextOptions<ServisDbContext> options)
            : base(options) { }

        public DbSet<Musteri> Musteriler { get; set; }
        public DbSet<Arac> Araclar { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<Personel> Personeller { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<ServisKaydi> ServisKayitlari { get; set; }
        public DbSet<Parca> Parcalar { get; set; }
        public DbSet<ServisParca> ServisParcalari { get; set; }
        public DbSet<Odeme> Odemeler { get; set; }
        public DbSet<Pozisyon> Pozisyonlar { get; set; }
        public DbSet<EkstraGider> EkstraGiderler { get; set; }
        public DbSet<Bildirim> Bildirimler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Musteri
            modelBuilder.Entity<Musteri>(entity =>
            {
                entity.ToTable("Musteriler");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AdSoyad).HasColumnName("ad_soyad");
                entity.Property(e => e.Telefon).HasColumnName("telefon");
                entity.Property(e => e.EPosta).HasColumnName("e_posta");
                entity.Property(e => e.Adres).HasColumnName("adres");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            // Arac
            modelBuilder.Entity<Arac>(entity =>
            {
                entity.ToTable("Araclar");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.MusteriId).HasColumnName("musteri_id");
                entity.Property(e => e.Plaka).HasColumnName("plaka");
                entity.Property(e => e.Marka).HasColumnName("marka");
                entity.Property(e => e.Model).HasColumnName("model");
                entity.Property(e => e.Yil).HasColumnName("yil");
                entity.Property(e => e.SasiNo).HasColumnName("sasi_no");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(d => d.Musteri)
                    .WithMany(p => p.Araclar)
                    .HasForeignKey(d => d.MusteriId);
            });

            // Randevu
            modelBuilder.Entity<Randevu>(entity =>
            {
                entity.ToTable("Randevular");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.AracId).HasColumnName("AracId");
                entity.Property(e => e.GirisTarihi).HasColumnName("GirisTarihi");
                entity.Property(e => e.CikisTarihi).HasColumnName("CikisTarihi");
                entity.Property(e => e.Km).HasColumnName("Km");
                entity.Property(e => e.ToplamUcret).HasColumnName("ToplamUcret");
                entity.Property(e => e.Durum).HasColumnName("Durum");
                entity.Property(e => e.Ihtiyaclar).HasColumnName("Ihtiyaclar");

                entity.HasOne(r => r.Arac)
                    .WithMany()
                    .HasForeignKey(r => r.AracId);
            });

            // Parca
            modelBuilder.Entity<Parca>(entity =>
            {
                entity.HasKey(e => e.ParcaID);
            });

            // ServisKaydi
            modelBuilder.Entity<ServisKaydi>(entity =>
            {
                entity.ToTable("ServisKayitlari");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.RandevuId).HasColumnName("RandevuId");
                entity.Property(e => e.PersonelId).HasColumnName("PersonelId");
                entity.Property(e => e.IscilikTutari).HasColumnName("IscilikTutari");
                entity.Property(e => e.ParcaTutari).HasColumnName("ParcaTutari");
                entity.Property(e => e.Durum).HasColumnName("Durum");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");

                entity.HasOne(s => s.Randevu)
                    .WithOne(r => r.ServisKaydi)
                    .HasForeignKey<ServisKaydi>(s => s.RandevuId);

                entity.HasOne(s => s.Personel)
                    .WithMany()
                    .HasForeignKey(s => s.PersonelId);
            });

            // ServisParca
            modelBuilder.Entity<ServisParca>(entity =>
            {
                entity.ToTable("ServisParcalari");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.ServisKaydiId).HasColumnName("ServisKaydiId");
                entity.Property(e => e.ParcaId).HasColumnName("ParcaId");
                entity.Property(e => e.Adet).HasColumnName("Adet");
                entity.Property(e => e.ToplamFiyat).HasColumnName("ToplamFiyat");

                entity.HasOne(sp => sp.ServisKaydi)
                    .WithMany()
                    .HasForeignKey(sp => sp.ServisKaydiId);

                entity.HasOne(sp => sp.Parca)
                    .WithMany()
                    .HasForeignKey(sp => sp.ParcaId)
                    .HasPrincipalKey(p => p.ParcaID);
            });

            // Odeme
            modelBuilder.Entity<Odeme>(entity =>
            {
                entity.ToTable("Odemeler");
                entity.Property(e => e.Id).HasColumnName("OdemeID");
                entity.Property(e => e.ServisKaydiId).HasColumnName("ServisID");
                entity.Property(e => e.OdenenTutar).HasColumnName("OdenenTutar");
                entity.Property(e => e.OdemeTipi).HasColumnName("OdemeTipi");
                entity.Property(e => e.OdemeTarihi).HasColumnName("OdemeTarihi");

                entity.HasOne(o => o.ServisKaydi)
                    .WithMany(s => s.Odemeler)
                    .HasForeignKey(o => o.ServisKaydiId);
            });

            modelBuilder.Entity<EkstraGider>(entity =>
            {
                entity.ToTable("EkstraGiderler", "dbo");
                entity.Property(e => e.Tutar)
                      .HasColumnType("decimal(18,2)");
            });
        }
    }
}