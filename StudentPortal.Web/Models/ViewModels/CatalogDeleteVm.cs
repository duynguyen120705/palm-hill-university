using System.ComponentModel.DataAnnotations;

namespace StudentPortal.Models.ViewModels
{
    // FR-26 Xóa mềm danh mục
    public class CatalogDeleteVm
    {
        [Required]
        public string Ma { get; set; } = string.Empty;
    }
}
