using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentPortal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KHOA",
                columns: table => new
                {
                    MaKhoa = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenKhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KHOA", x => x.MaKhoa);
                });

            migrationBuilder.CreateTable(
                name: "KHOA_HOC",
                columns: table => new
                {
                    MaKhoaHoc = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    NamNhapHoc = table.Column<int>(type: "int", nullable: false),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KHOA_HOC", x => x.MaKhoaHoc);
                });

            migrationBuilder.CreateTable(
                name: "NHAT_KY_THAO_TAC",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThoiDiem = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    HanhDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DoiTuongLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DoiTuongMa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NHAT_KY_THAO_TAC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LOP",
                columns: table => new
                {
                    MaLop = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenLop = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaKhoa = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    MaKhoaHoc = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOP", x => x.MaLop);
                    table.ForeignKey(
                        name: "FK_LOP_KHOA_HOC_MaKhoaHoc",
                        column: x => x.MaKhoaHoc,
                        principalTable: "KHOA_HOC",
                        principalColumn: "MaKhoaHoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LOP_KHOA_MaKhoa",
                        column: x => x.MaKhoa,
                        principalTable: "KHOA",
                        principalColumn: "MaKhoa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SINH_VIEN",
                columns: table => new
                {
                    MaSV = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "date", nullable: false),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    MaLop = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SDT = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NgayXoa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NguoiXoa = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LyDoXoa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINH_VIEN", x => x.MaSV);
                    table.ForeignKey(
                        name: "FK_SINH_VIEN_LOP_MaLop",
                        column: x => x.MaLop,
                        principalTable: "LOP",
                        principalColumn: "MaLop",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaSV = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    MaKhoaPhuTrach = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    TrangThaiTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    LanDangNhapDauTien = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_KHOA_MaKhoaPhuTrach",
                        column: x => x.MaKhoaPhuTrach,
                        principalTable: "KHOA",
                        principalColumn: "MaKhoa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_SINH_VIEN_MaSV",
                        column: x => x.MaSV,
                        principalTable: "SINH_VIEN",
                        principalColumn: "MaSV",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LICH_SU_HO_SO_SINH_VIEN",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSV = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    Loai = table.Column<int>(type: "int", nullable: false),
                    GiaTriCu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GiaTriMoi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LyDo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NguoiThucHien = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ThoiDiem = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LICH_SU_HO_SO_SINH_VIEN", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LICH_SU_HO_SO_SINH_VIEN_SINH_VIEN_MaSV",
                        column: x => x.MaSV,
                        principalTable: "SINH_VIEN",
                        principalColumn: "MaSV",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MaKhoaPhuTrach",
                table: "AspNetUsers",
                column: "MaKhoaPhuTrach");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MaSV",
                table: "AspNetUsers",
                column: "MaSV",
                unique: true,
                filter: "[MaSV] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LICH_SU_HO_SO_SINH_VIEN_MaSV",
                table: "LICH_SU_HO_SO_SINH_VIEN",
                column: "MaSV");

            migrationBuilder.CreateIndex(
                name: "IX_LICH_SU_HO_SO_SINH_VIEN_ThoiDiem",
                table: "LICH_SU_HO_SO_SINH_VIEN",
                column: "ThoiDiem");

            migrationBuilder.CreateIndex(
                name: "IX_LOP_MaKhoa_MaKhoaHoc_TenLop",
                table: "LOP",
                columns: new[] { "MaKhoa", "MaKhoaHoc", "TenLop" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LOP_MaKhoaHoc",
                table: "LOP",
                column: "MaKhoaHoc");

            migrationBuilder.CreateIndex(
                name: "IX_NHAT_KY_THAO_TAC_DoiTuongLoai_DoiTuongMa",
                table: "NHAT_KY_THAO_TAC",
                columns: new[] { "DoiTuongLoai", "DoiTuongMa" });

            migrationBuilder.CreateIndex(
                name: "IX_NHAT_KY_THAO_TAC_ThoiDiem",
                table: "NHAT_KY_THAO_TAC",
                column: "ThoiDiem");

            migrationBuilder.CreateIndex(
                name: "IX_NHAT_KY_THAO_TAC_Username",
                table: "NHAT_KY_THAO_TAC",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_SINH_VIEN_Email",
                table: "SINH_VIEN",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SINH_VIEN_HoTen",
                table: "SINH_VIEN",
                column: "HoTen");

            migrationBuilder.CreateIndex(
                name: "IX_SINH_VIEN_MaLop",
                table: "SINH_VIEN",
                column: "MaLop");

            migrationBuilder.CreateIndex(
                name: "IX_SINH_VIEN_TrangThai",
                table: "SINH_VIEN",
                column: "TrangThai");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "LICH_SU_HO_SO_SINH_VIEN");

            migrationBuilder.DropTable(
                name: "NHAT_KY_THAO_TAC");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "SINH_VIEN");

            migrationBuilder.DropTable(
                name: "LOP");

            migrationBuilder.DropTable(
                name: "KHOA_HOC");

            migrationBuilder.DropTable(
                name: "KHOA");
        }
    }
}
