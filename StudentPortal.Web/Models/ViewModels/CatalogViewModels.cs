using System.ComponentModel.DataAnnotations;
using StudentPortal.Infrastructure.Validation;

namespace StudentPortal.Models.ViewModels
{
    // FR-23 Bộ lọc danh mục; MaKhoa, MaKhoaHoc chỉ dùng cho danh mục Lớp
    public class CatalogFilterVm
    {
        public string? MaKhoa { get; set; }
        public string? MaKhoaHoc { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class FacultyListItemDto
    {
        public string MaKhoa { get; set; } = string.Empty;
        public string TenKhoa { get; set; } = string.Empty;
        public int SoLop { get; set; }
    }

    public class CourseListItemDto
    {
        public string MaKhoaHoc { get; set; } = string.Empty;
        public int NamNhapHoc { get; set; }
        public int SoLop { get; set; }

        // Khóa đã có lớp chứa sinh viên: giữ cố định năm nhập học (FR-25)
        public bool CoSinhVien { get; set; }
    }

    public class ClassListItemDto
    {
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public string MaKhoa { get; set; } = string.Empty;
        public string TenKhoa { get; set; } = string.Empty;
        public string MaKhoaHoc { get; set; } = string.Empty;
        public int SoSinhVien { get; set; }
    }

    public class FacultyCreateVm
    {
        [Required(ErrorMessage = "Mã khoa không được để trống")]
        [MaSo(2, 15)]
        public string MaKhoa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khoa tối đa 100 ký tự")]
        public string TenKhoa { get; set; } = string.Empty;
    }

    public class FacultyEditVm
    {
        [Required]
        public string MaKhoa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [StringLength(100, ErrorMessage = "Tên khoa tối đa 100 ký tự")]
        public string TenKhoa { get; set; } = string.Empty;
    }

    public class CourseCreateVm
    {
        [Required(ErrorMessage = "Mã khóa không được để trống")]
        [MaSo(2, 15)]
        public string MaKhoaHoc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm nhập học không được để trống")]
        public int? NamNhapHoc { get; set; }
    }

    public class CourseEditVm
    {
        [Required]
        public string MaKhoaHoc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm nhập học không được để trống")]
        public int? NamNhapHoc { get; set; }
    }

    public class ClassCreateVm
    {
        [Required(ErrorMessage = "Mã lớp không được để trống")]
        [MaSo(2, 15)]
        public string MaLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên lớp không được để trống")]
        [StringLength(50, ErrorMessage = "Tên lớp tối đa 50 ký tự")]
        public string TenLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        public string MaKhoa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn khóa")]
        public string MaKhoaHoc { get; set; } = string.Empty;
    }

    public class ClassEditVm
    {
        [Required]
        public string MaLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên lớp không được để trống")]
        [StringLength(50, ErrorMessage = "Tên lớp tối đa 50 ký tự")]
        public string TenLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        public string MaKhoa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn khóa")]
        public string MaKhoaHoc { get; set; } = string.Empty;
    }

    // FR-26 Xóa mềm danh mục
    public class CatalogDeleteVm
    {
        [Required]
        public string Ma { get; set; } = string.Empty;
    }
}
