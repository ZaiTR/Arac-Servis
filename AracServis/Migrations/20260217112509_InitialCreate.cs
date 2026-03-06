using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AracServis.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
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
            */

            /*
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
            */

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

            /*
            migrationBuilder.CreateIndex(
                name: "IX_Araclar_musteri_id",
                table: "Araclar",
                column: "musteri_id");
            */

            migrationBuilder.CreateIndex(
                name: "IX_Randevular_AracId",
                table: "Randevular",
                column: "AracId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Randevular");

            migrationBuilder.DropTable(
                name: "Araclar");

            migrationBuilder.DropTable(
                name: "Musteriler");
        }
    }
}
