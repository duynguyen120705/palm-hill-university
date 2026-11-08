IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Khoa] (
    [MaKhoa] varchar(15) NOT NULL,
    [TenKhoa] nvarchar(100) NOT NULL,
    [DaXoa] bit NOT NULL DEFAULT CAST(0 AS bit),
    [NgayTao] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Khoa] PRIMARY KEY ([MaKhoa])
);
GO

CREATE TABLE [KhoaHoc] (
    [MaKhoaHoc] varchar(15) NOT NULL,
    [NamNhapHoc] int NOT NULL,
    [DaXoa] bit NOT NULL DEFAULT CAST(0 AS bit),
    [NgayTao] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_KhoaHoc] PRIMARY KEY ([MaKhoaHoc]),
    CONSTRAINT [CK_KhoaHoc_NamNhapHoc] CHECK ([NamNhapHoc] BETWEEN 2000 AND YEAR(GETDATE()) + 1)
);
GO

CREATE TABLE [NhatKyThaoTac] (
    [Id] bigint NOT NULL IDENTITY,
    [ThoiDiem] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [Username] nvarchar(256) NOT NULL,
    [HanhDong] varchar(50) NOT NULL,
    [DoiTuongLoai] varchar(30) NOT NULL,
    [DoiTuongMa] nvarchar(256) NOT NULL,
    [ChiTiet] nvarchar(500) NULL,
    CONSTRAINT [PK_NhatKyThaoTac] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Lop] (
    [MaLop] varchar(15) NOT NULL,
    [TenLop] nvarchar(50) NOT NULL,
    [MaKhoa] varchar(15) NOT NULL,
    [MaKhoaHoc] varchar(15) NOT NULL,
    [DaXoa] bit NOT NULL DEFAULT CAST(0 AS bit),
    [NgayTao] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Lop] PRIMARY KEY ([MaLop]),
    CONSTRAINT [FK_Lop_KhoaHoc_MaKhoaHoc] FOREIGN KEY ([MaKhoaHoc]) REFERENCES [KhoaHoc] ([MaKhoaHoc]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Lop_Khoa_MaKhoa] FOREIGN KEY ([MaKhoa]) REFERENCES [Khoa] ([MaKhoa]) ON DELETE NO ACTION
);
GO

CREATE TABLE [SinhVien] (
    [MaSV] varchar(15) NOT NULL,
    [HoTen] nvarchar(100) NOT NULL,
    [HoTenKhongDau] varchar(100) NOT NULL,
    [NgaySinh] date NOT NULL,
    [GioiTinh] tinyint NOT NULL,
    [MaLop] varchar(15) NOT NULL,
    [SDT] varchar(10) NULL,
    [Email] varchar(100) NULL,
    [DiaChi] nvarchar(255) NULL,
    [TrangThai] tinyint NOT NULL,
    [DaCoBienDong] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DaXoa] bit NOT NULL DEFAULT CAST(0 AS bit),
    [NgayXoa] datetime2 NULL,
    [NguoiXoa] nvarchar(256) NULL,
    [LyDoXoa] nvarchar(500) NULL,
    [NgayTao] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [NgayCapNhat] datetime2 NULL,
    CONSTRAINT [PK_SinhVien] PRIMARY KEY ([MaSV]),
    CONSTRAINT [CK_SinhVien_LyDoXoa] CHECK ([DaXoa] = 0 OR [LyDoXoa] IS NOT NULL),
    CONSTRAINT [FK_SinhVien_Lop_MaLop] FOREIGN KEY ([MaLop]) REFERENCES [Lop] ([MaLop]) ON DELETE NO ACTION
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [HoTen] nvarchar(100) NULL,
    [MaSV] varchar(15) NULL,
    [MaKhoaPhuTrach] varchar(15) NULL,
    [TrangThaiTaiKhoan] tinyint NOT NULL,
    [LyDoKhoa] nvarchar(255) NULL,
    [LanDangNhapCuoi] datetime2 NULL,
    [NgayTao] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_Khoa_MaKhoaPhuTrach] FOREIGN KEY ([MaKhoaPhuTrach]) REFERENCES [Khoa] ([MaKhoa]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AspNetUsers_SinhVien_MaSV] FOREIGN KEY ([MaSV]) REFERENCES [SinhVien] ([MaSV]) ON DELETE NO ACTION
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE INDEX [IX_AspNetUsers_MaKhoaPhuTrach] ON [AspNetUsers] ([MaKhoaPhuTrach]);
GO

CREATE UNIQUE INDEX [IX_AspNetUsers_MaSV] ON [AspNetUsers] ([MaSV]) WHERE [MaSV] IS NOT NULL;
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Lop_MaKhoa] ON [Lop] ([MaKhoa]);
GO

CREATE UNIQUE INDEX [IX_Lop_MaKhoa_MaKhoaHoc_TenLop] ON [Lop] ([MaKhoa], [MaKhoaHoc], [TenLop]);
GO

CREATE INDEX [IX_Lop_MaKhoaHoc] ON [Lop] ([MaKhoaHoc]);
GO

CREATE INDEX [IX_NhatKyThaoTac_DoiTuongMa] ON [NhatKyThaoTac] ([DoiTuongMa]);
GO

CREATE INDEX [IX_NhatKyThaoTac_ThoiDiem] ON [NhatKyThaoTac] ([ThoiDiem] DESC);
GO

CREATE INDEX [IX_NhatKyThaoTac_Username] ON [NhatKyThaoTac] ([Username]);
GO

CREATE INDEX [IX_SinhVien_DaXoa] ON [SinhVien] ([DaXoa]);
GO

CREATE INDEX [IX_SinhVien_HoTenKhongDau] ON [SinhVien] ([HoTenKhongDau]);
GO

CREATE INDEX [IX_SinhVien_MaLop] ON [SinhVien] ([MaLop]);
GO

CREATE INDEX [IX_SinhVien_TrangThai] ON [SinhVien] ([TrangThai]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261108103119_InitialCreate', N'8.0.10');
GO

COMMIT;
GO

