
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace BasarStajApp_New.DTOs
{
    public class GeometryDTO
    {
        [Required(ErrorMessage = "Name alanı zorunludur.")]
        [MaxLength(100, ErrorMessage = "Name en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "WKT alanı zorunludur.")]
        [MaxLength(500, ErrorMessage = "WKT en fazla 500 karakter olabilir.")]
        public string WKT { get; set; } = string.Empty;
    }
}
