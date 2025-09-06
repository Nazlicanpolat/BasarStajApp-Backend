using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BasarStajApp_New.Entity
{
    public class GeometryEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Geometry'yi JSON'a göndermiyoruz (Infinity/NaN hatasını engeller)
        [JsonIgnore]
        [Column("Feature")]
        public Geometry Feature { get; set; } = default!;

        // ADO.NET kodunun kullandığı yardımcı alan (DB'ye map edilmez, JSON'a da gönderilmez)
        [NotMapped]
        [JsonIgnore]
        public string WKT
        {
            get => Feature is null ? string.Empty : new WKTWriter().Write(Feature);
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Feature = null!;
                    return;
                }
                Feature = new WKTReader().Read(value);
            }
        }

        // İstemciye (Swagger/Frontend) WKT olarak gösterilecek alan (DB'ye map edilmez)
        [NotMapped]
        [JsonPropertyName("wkt")]
        public string WktOut => Feature is null ? string.Empty : new WKTWriter().Write(Feature);

        public string? Type { get; set; }   // Point | LineString | Polygon ...
    }
}
