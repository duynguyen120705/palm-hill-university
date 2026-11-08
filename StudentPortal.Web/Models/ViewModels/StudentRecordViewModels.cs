using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using StudentPortal.Infrastructure.Validation;
using StudentPortal.Models.Entities;

namespace StudentPortal.Models.ViewModels
{
    // FR-13 Thêm hồ sơ sinh viên (phiếu BM01)
    public class StudentCreateVm
    {
        [Required(ErrorMessage = "Mã sinh viên không được để trống")]
        [MaSo]
        [Display(Name = "Mã sinh viên")]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationRules.HoTenMessage)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationRules.HoTenMessage)]
        [HoTen]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [NgaySinh(MinAge = 15)]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public GioiTinh? GioiTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lớp")]
        [Display(Name = "Lớp hành chính")]
        public string MaLop { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public TrangThaiHocTap TrangThai { get; set; } = TrangThaiHocTap.DangHoc;

        [SoDienThoai]
        [Display(Name = "Số điện thoại")]
        public string? SDT { get; set; }

        [StringLength(100, ErrorMessage = ValidationRules.EmailMessage)]
        [EmailAddress(ErrorMessage = ValidationRules.EmailMessage)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Mật khẩu ban đầu không được để trống")]
        [MatKhauManh]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu ban đầu")]
        public string MatKhau { get; set; } = string.Empty;
    }

    // FR-15 Cập nhật hồ sơ: mã SV giữ cố định; lớp đổi theo FR-17, trạng thái theo FR-16
    public class StudentEditVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationRules.HoTenMessage)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = ValidationRules.HoTenMessage)]
        [HoTen]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [NgaySinh(MinAge = 15)]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public GioiTinh? GioiTinh { get; set; }

        [SoDienThoai]
        [Display(Name = "Số điện thoại")]
        public string? SDT { get; set; }

        [StringLength(100, ErrorMessage = ValidationRules.EmailMessage)]
        [EmailAddress(ErrorMessage = ValidationRules.EmailMessage)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
    }

    // FR-16 Đổi trạng thái học tập
    public class ChangeStatusVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn trạng thái mới")]
        public TrangThaiHocTap? TrangThaiMoi { get; set; }

        [StringLength(255, ErrorMessage = "Lý do tối đa 255 ký tự")]
        public string? LyDo { get; set; }
    }

    // FR-17 Chuyển lớp
    public class TransferVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn lớp đích")]
        public string MaLopDich { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Lý do tối đa 255 ký tự")]
        public string? LyDo { get; set; }
    }

    // FR-18 Xóa mềm hồ sơ
    public class StudentDeleteVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [StringLength(500, ErrorMessage = "Lý do tối đa 500 ký tự")]
        public string LyDo { get; set; } = string.Empty;
    }

    // FR-19 Cấp lại mật khẩu tài khoản sinh viên
    public class StudentResetPasswordVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MatKhauManh]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Compare(nameof(MatKhauMoi), ErrorMessage = "Xác nhận mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }

    // FR-20 Khóa/mở khóa tài khoản sinh viên
    public class StudentSetLockVm
    {
        [Required]
        public string MaSV { get; set; } = string.Empty;

        // Bắt buộc gửi lên, tránh thiếu trường bị hiểu ngầm là false (mở khóa)
        [JsonRequired]
        public bool Khoa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [StringLength(255, ErrorMessage = "Lý do tối đa 255 ký tự")]
        public string LyDo { get; set; } = string.Empty;
    }

    // Trạng thái được phép chuyển tới theo vai trò và trạng thái hiện tại (bảng quy tắc mục 3.1.2)
    public class AllowedStatusDto
    {
        public TrangThaiHocTap Value { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool BatBuocLyDo { get; set; }
    }

    // FR-14 Chi tiết hồ sơ, kèm các thao tác được phép theo vai trò và trạng thái hồ sơ
    public class StudentDetailVm
    {
        public string MaSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string NgaySinh { get; set; } = string.Empty;
        public string NgaySinhFormatted { get; set; } = string.Empty;
        public GioiTinh GioiTinh { get; set; }
        public string GioiTinhText { get; set; } = string.Empty;
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public string TenKhoa { get; set; } = string.Empty;
        public string MaKhoaHoc { get; set; } = string.Empty;
        public string? SDT { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public TrangThaiHocTap TrangThai { get; set; }
        public string TrangThaiText { get; set; } = string.Empty;
        public string TrangThaiTaiKhoanText { get; set; } = string.Empty;

        public bool CoTheDoiTrangThai { get; set; }
        public bool CoTheXoa { get; set; }
        public bool CoTheCapLaiMatKhau { get; set; }
        public bool CoTheKhoa { get; set; }
        public bool CoTheMoKhoa { get; set; }
    }
}
