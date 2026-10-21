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

CREATE TABLE [KHOA] (
    [MaKhoa] varchar(20) NOT NULL,
    [TenKhoa] nvarchar(100) NOT NULL,
    [DaXoa] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_KHOA] PRIMARY KEY ([MaKhoa])
);
GO

CREATE TABLE [KHOA_HOC] (
    [MaKhoaHoc] varchar(20) NOT NULL,
    [NamNhapHoc] int NOT NULL,
    [DaXoa] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_KHOA_HOC] PRIMARY KEY ([MaKhoaHoc])
);
GO

CREATE TABLE [NHAT_KY_THAO_TAC] (
    [Id] bigint NOT NULL IDENTITY,
    [ThoiDiem] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [Username] nvarchar(256) NOT NULL,
    [HanhDong] nvarchar(100) NOT NULL,
    [DoiTuongLoai] nvarchar(100) NOT NULL,
    [DoiTuongMa] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_NHAT_KY_THAO_TAC] PRIMARY KEY ([Id])
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

CREATE TABLE [LOP] (
    [MaLop] varchar(20) NOT NULL,
    [TenLop] nvarchar(100) NOT NULL,
    [MaKhoa] varchar(20) NOT NULL,
    [MaKhoaHoc] varchar(20) NOT NULL,
    [DaXoa] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_LOP] PRIMARY KEY ([MaLop]),
    CONSTRAINT [FK_LOP_KHOA_HOC_MaKhoaHoc] FOREIGN KEY ([MaKhoaHoc]) REFERENCES [KHOA_HOC] ([MaKhoaHoc]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LOP_KHOA_MaKhoa] FOREIGN KEY ([MaKhoa]) REFERENCES [KHOA] ([MaKhoa]) ON DELETE NO ACTION
);
GO

CREATE TABLE [SINH_VIEN] (
    [MaSV] varchar(15) NOT NULL,
    [HoTen] nvarchar(100) NOT NULL,
    [NgaySinh] date NOT NULL,
    [GioiTinh] int NOT NULL,
    [MaLop] varchar(20) NOT NULL,
    [SDT] varchar(10) NULL,
    [Email] nvarchar(100) NULL,
    [DiaChi] nvarchar(250) NULL,
    [TrangThai] int NOT NULL,
    [DaXoa] bit NOT NULL DEFAULT CAST(0 AS bit),
    [NgayXoa] datetime2 NULL,
    [NguoiXoa] nvarchar(256) NULL,
    [LyDoXoa] nvarchar(500) NULL,
    [NgayTao] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [NgayCapNhat] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [RowVersion] rowversion NULL,
    CONSTRAINT [PK_SINH_VIEN] PRIMARY KEY ([MaSV]),
    CONSTRAINT [FK_SINH_VIEN_LOP_MaLop] FOREIGN KEY ([MaLop]) REFERENCES [LOP] ([MaLop]) ON DELETE NO ACTION
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [HoTen] nvarchar(100) NULL,
    [MaSV] varchar(15) NULL,
    [MaKhoaPhuTrach] varchar(20) NULL,
    [TrangThaiTaiKhoan] int NOT NULL,
    [LanDangNhapDauTien] datetime2 NULL,
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
    CONSTRAINT [FK_AspNetUsers_KHOA_MaKhoaPhuTrach] FOREIGN KEY ([MaKhoaPhuTrach]) REFERENCES [KHOA] ([MaKhoa]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AspNetUsers_SINH_VIEN_MaSV] FOREIGN KEY ([MaSV]) REFERENCES [SINH_VIEN] ([MaSV]) ON DELETE SET NULL
);
GO

CREATE TABLE [LICH_SU_HO_SO_SINH_VIEN] (
    [Id] bigint NOT NULL IDENTITY,
    [MaSV] varchar(15) NOT NULL,
    [Loai] int NOT NULL,
    [GiaTriCu] nvarchar(500) NULL,
    [GiaTriMoi] nvarchar(500) NULL,
    [LyDo] nvarchar(500) NULL,
    [NguoiThucHien] nvarchar(256) NOT NULL,
    [ThoiDiem] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_LICH_SU_HO_SO_SINH_VIEN] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LICH_SU_HO_SO_SINH_VIEN_SINH_VIEN_MaSV] FOREIGN KEY ([MaSV]) REFERENCES [SINH_VIEN] ([MaSV]) ON DELETE CASCADE
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

CREATE INDEX [IX_LICH_SU_HO_SO_SINH_VIEN_MaSV] ON [LICH_SU_HO_SO_SINH_VIEN] ([MaSV]);
GO

CREATE INDEX [IX_LICH_SU_HO_SO_SINH_VIEN_ThoiDiem] ON [LICH_SU_HO_SO_SINH_VIEN] ([ThoiDiem]);
GO

CREATE UNIQUE INDEX [IX_LOP_MaKhoa_MaKhoaHoc_TenLop] ON [LOP] ([MaKhoa], [MaKhoaHoc], [TenLop]);
GO

CREATE INDEX [IX_LOP_MaKhoaHoc] ON [LOP] ([MaKhoaHoc]);
GO

CREATE INDEX [IX_NHAT_KY_THAO_TAC_DoiTuongLoai_DoiTuongMa] ON [NHAT_KY_THAO_TAC] ([DoiTuongLoai], [DoiTuongMa]);
GO

CREATE INDEX [IX_NHAT_KY_THAO_TAC_ThoiDiem] ON [NHAT_KY_THAO_TAC] ([ThoiDiem]);
GO

CREATE INDEX [IX_NHAT_KY_THAO_TAC_Username] ON [NHAT_KY_THAO_TAC] ([Username]);
GO

CREATE UNIQUE INDEX [IX_SINH_VIEN_Email] ON [SINH_VIEN] ([Email]) WHERE [Email] IS NOT NULL;
GO

CREATE INDEX [IX_SINH_VIEN_HoTen] ON [SINH_VIEN] ([HoTen]);
GO

CREATE INDEX [IX_SINH_VIEN_MaLop] ON [SINH_VIEN] ([MaLop]);
GO

CREATE INDEX [IX_SINH_VIEN_TrangThai] ON [SINH_VIEN] ([TrangThai]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261014132133_InitialCreate', N'8.0.10');
GO

COMMIT;
GO

