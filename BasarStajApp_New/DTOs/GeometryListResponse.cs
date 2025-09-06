using BasarStajApp_New.Entity;
using System.Collections.Generic;

namespace BasarStajApp_New.DTOs
{
    public class GeometryListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<GeometryResponse> Data { get; set; }
        public int Total { get; set; }     // toplam kayıt
        public int Page { get; set; }      // mevcut sayfa
        public int PageSize { get; set; }  // sayfa boyutu

    }
}
