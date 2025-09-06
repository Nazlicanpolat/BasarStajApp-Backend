// Mappers/GeometryMapper.cs
using BasarStajApp_New.DTOs;
using BasarStajApp_New.Entity;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Collections.Generic;
using System.Linq;

namespace BasarStajApp_New.Mappers
{
    public static class GeometryMapper
    {
        // Tek örnek kullan (GC ve performans için iyi)
        private static readonly WKTReader _reader = new();
        private static readonly WKTWriter _writer = new();

        // ---- Yardımcı: WKT -> Geometry (+ SRID=4326) ----
        private static Geometry ReadWktWithSrid(string wkt)
        {
            var geom = _reader.Read((wkt ?? string.Empty).Trim());
            // Kullanıcı SRID yazmasa da biz 4326 set ediyoruz
            geom.SRID = 4326;
            return geom;
        }

        // ---- DTO -> Entity ----
        public static GeometryEntity ToEntity(GeometryDTO dto, int id = 0)
        {
            var geom = ReadWktWithSrid(dto.WKT);

            // SONRA entity'yi oluştururken Type'ı da set et
            return new GeometryEntity
            {
                Id = id,
                Name = (dto.Name ?? string.Empty).Trim(),
                Feature = geom,                     // SRID=4326
                Type = geom.GeometryType            // <-- EKLENEN SATIR (Point | LineString | Polygon ...)
            };
        }

        // ---- Entity -> DTO (WKT string geri yaz) ----
        public static GeometryDTO ToDTO(GeometryEntity entity)
        {
            return new GeometryDTO
            {
                Name = entity.Name,
                WKT = entity.Feature is null ? string.Empty : _writer.Write(entity.Feature)
            };
        }

        // ---- Response yardımcıları (var olan iki tipine uyumlu) ----
        public static GeometryResponse ToResponse(GeometryEntity entity, string msg = "İşlem başarılı.")
            => new GeometryResponse { Success = true, Message = msg, Data = entity };

        public static GeometryListResponse ToListResponse(IEnumerable<GeometryEntity> list, string msg = "Liste getirildi.")
            => new GeometryListResponse
            {
                Success = true,
                Message = msg,
                Data = list.Select(e => ToResponse(e, "Kayıt")).ToList()
            };

        public static GeometryResponse Fail(string msg)
            => new GeometryResponse { Success = false, Message = msg, Data = null };

        public static GeometryListResponse FailList(string msg)
            => new GeometryListResponse { Success = false, Message = msg, Data = new List<GeometryResponse>() };
    }
}
