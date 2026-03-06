using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AracServis.Migrations
{
    /// <inheritdoc />
    public partial class ServisSistemiEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

             
            migrationBuilder.CreateTable(
                name: "Parcalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParcaAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stok = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcalar", x => x.Id);
                });
            

            
            migrationBuilder.CreateTable(
                name: "personeller",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    soyad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pozisyon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    maas = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ise_giris_tarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    durum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sifre = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personeller", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ServisKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RandevuId = table.Column<int>(type: "int", nullable: false),
                    PersonelId = table.Column<int>(type: "int", nullable: false),
                    IscilikTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ParcaTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServisKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServisKayitlari_Randevular_RandevuId",
                        column: x => x.RandevuId,
                        principalTable: "Randevular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServisKayitlari_personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServisKayitlari_PersonelId",
                table: "ServisKayitlari",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_ServisKayitlari_RandevuId",
                table: "ServisKayitlari",
                column: "RandevuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Parcalar");

            migrationBuilder.DropTable(
                name: "ServisKayitlari");

            migrationBuilder.DropTable(
                name: "personeller");
        }
    }
}
