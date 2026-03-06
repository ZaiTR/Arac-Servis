using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AracServis.Migrations
{
    /// <inheritdoc />
    public partial class TamSistemSenkronizasyon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Odemeler tablosu zaten varsa olusturma
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Odemeler' AND xtype='U')
                BEGIN
                    CREATE TABLE [Odemeler] (
                        [Id] int NOT NULL IDENTITY,
                        [ServisKaydiId] int NOT NULL,
                        [OdenenTutar] decimal(18,2) NOT NULL,
                        [OdemeTipi] nvarchar(max) NOT NULL,
                        [OdemeTarihi] datetime2 NOT NULL,
                        CONSTRAINT [PK_Odemeler] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Odemeler_ServisKayitlari_ServisKaydiId] FOREIGN KEY ([ServisKaydiId]) REFERENCES [ServisKayitlari] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_Odemeler_ServisKaydiId] ON [Odemeler] ([ServisKaydiId]);
                END
            ");

            // ServisParcalari tablosu zaten varsa olusturma
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ServisParcalari' AND xtype='U')
                BEGIN
                    CREATE TABLE [ServisParcalari] (
                        [Id] int NOT NULL IDENTITY,
                        [ServisKaydiId] int NOT NULL,
                        [ParcaId] int NOT NULL,
                        [Adet] int NOT NULL,
                        [ToplamFiyat] decimal(18,2) NOT NULL,
                        CONSTRAINT [PK_ServisParcalari] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_ServisParcalari_Parcalar_ParcaId] FOREIGN KEY ([ParcaId]) REFERENCES [Parcalar] ([ParcaID]) ON DELETE CASCADE,
                        CONSTRAINT [FK_ServisParcalari_ServisKayitlari_ServisKaydiId] FOREIGN KEY ([ServisKaydiId]) REFERENCES [ServisKayitlari] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_ServisParcalari_ParcaId] ON [ServisParcalari] ([ParcaId]);
                    CREATE INDEX [IX_ServisParcalari_ServisKaydiId] ON [ServisParcalari] ([ServisKaydiId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sysobjects WHERE name='Odemeler' AND xtype='U') DROP TABLE [Odemeler];");
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sysobjects WHERE name='ServisParcalari' AND xtype='U') DROP TABLE [ServisParcalari];");
        }
    }
}
