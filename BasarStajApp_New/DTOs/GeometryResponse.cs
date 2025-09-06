using BasarStajApp_New.Entity;

namespace BasarStajApp_New.DTOs
{
    public class GeometryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GeometryEntity? Data { get; set; }
        public string? Type { get; set; }

    }
}
