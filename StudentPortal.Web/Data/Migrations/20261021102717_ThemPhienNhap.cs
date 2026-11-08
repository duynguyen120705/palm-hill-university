using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPortal.Migrations
{
    /// <inheritdoc />
    public partial class ThemPhienNhap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PHIEN_NHAP",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThoiDiem = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TenFile = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SoThemMoi = table.Column<int>(type: "int", nullable: false),
                    SoCapNhat = table.Column<int>(type: "int", nullable: false),
                    SoLoi = table.Column<int>(type: "int", nullable: false),
                    ChiTietLoi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHIEN_NHAP", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PHIEN_NHAP_ThoiDiem",
                table: "PHIEN_NHAP",
                column: "ThoiDiem");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEN_NHAP_Username",
                table: "PHIEN_NHAP",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PHIEN_NHAP");
        }
    }
}
