using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AracServis.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- DROP SECTION ---
            // Dynamic Drop FKs for all tables we want to drop
            string[] tables = new[] { "ServisKayitlari", "Randevular", "Araclar", "Musteriler" };
            foreach (var table in tables)
            {
                migrationBuilder.Sql($@"
                    DECLARE @sql nvarchar(MAX) = N'';
                    SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id))
                        + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
                        ' DROP CONSTRAINT ' + QUOTENAME(name) + ';'
                    FROM sys.foreign_keys
                    WHERE referenced_object_id = OBJECT_ID(N'dbo.{table}');
                    EXEC sp_executesql @sql;
                ");
            }

            // Drop tables
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.ServisKayitlari");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.Randevular");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.Araclar");
            migrationBuilder.Sql("DROP TABLE IF EXISTS dbo.Musteriler");

            // --- CREATE SECTION ---
            migrationBuilder.CreateTable(
                name: "Musteriler",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ad_soyad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    e_posta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    adres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musteriler", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Araclar",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    musteri_id = table.Column<int>(type: "int", nullable: false),
                    plaka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    marka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    yil = table.Column<int>(type: "int", nullable: true),
                    sasi_no = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Araclar", x => x.id);
                    table.ForeignKey(
                        name: "FK_Araclar_Musteriler_musteri_id",
                        column: x => x.musteri_id,
                        principalTable: "Musteriler",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Randevular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AracId = table.Column<int>(type: "int", nullable: false),
                    GirisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CikisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Km = table.Column<int>(type: "int", nullable: true),
                    ToplamUcret = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Randevular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Randevular_Araclar_AracId",
                        column: x => x.AracId,
                        principalTable: "Araclar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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

            // --- INDEX SECTION ---
            migrationBuilder.CreateIndex(
                name: "IX_Araclar_musteri_id",
                table: "Araclar",
                column: "musteri_id");

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_AracId",
                table: "Randevular",
                column: "AracId");

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
                name: "ServisKayitlari");

            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropTable(
                name: "Araclar");

            migrationBuilder.DropTable(
                name: "Musteriler");
        }
    }
}
