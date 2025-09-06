using BasarStajApp_New.DTOs;
using BasarStajApp_New.Entity;
using BasarStajApp_New.Interfaces;
using BasarStajApp_New.Mappers;
using System.Collections.Generic;
using System.Linq;

namespace BasarStajApp_New.Services
{
    public class GeometryService : IGeometryService
    {
        private readonly List<GeometryEntity> _liste = new();
        private int _nextID = 1;

        public GeometryListResponse GetAll()
        {
            var responses = _liste
                .Select(e => GeometryMapper.ToResponse(e, "Kayıt getirildi"))
                .ToList();

            return new GeometryListResponse
            {
                Success = true,
                Message = "Veriler getirildi",
                Data = responses
            };
        }

        public GeometryResponse GetById(int id)
        {
            var entity = _liste.FirstOrDefault(x => x.Id == id);
            if (entity == null)
                return new GeometryResponse { Success = false, Message = "Kayıt bulunamadı" };

            return GeometryMapper.ToResponse(entity, "Kayıt getirildi");
        }

        public GeometryResponse Add(GeometryDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.WKT))
                return new GeometryResponse { Success = false, Message = "Geçersiz veri" };

            var entity = GeometryMapper.ToEntity(dto);
            entity.Id = _nextID++;
            _liste.Add(entity);

            return GeometryMapper.ToResponse(entity, "Kayıt eklendi");
        }

        public GeometryListResponse AddRange(List<GeometryDTO> dtos)
        {
            var responses = new List<GeometryResponse>();

            foreach (var dto in dtos)
            {
                if (!string.IsNullOrWhiteSpace(dto.Name) && !string.IsNullOrWhiteSpace(dto.WKT))
                {
                    var entity = GeometryMapper.ToEntity(dto);
                    entity.Id = _nextID++;
                    _liste.Add(entity);

                    responses.Add(GeometryMapper.ToResponse(entity, "Kayıt eklendi"));
                }
            }

            return new GeometryListResponse
            {
                Success = true,
                Message = "Toplu kayıt eklendi",
                Data = responses
            };
        }

        public GeometryResponse Update(int id, GeometryDTO dto)
        {
            var entity = _liste.FirstOrDefault(x => x.Id == id);
            if (entity == null)
                return new GeometryResponse { Success = false, Message = "Kayıt bulunamadı" };

            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.WKT))
                return new GeometryResponse { Success = false, Message = "Geçersiz veri" };

            entity.Name = dto.Name.Trim();
            entity.Feature = new NetTopologySuite.IO.WKTReader().Read(dto.WKT);

            return GeometryMapper.ToResponse(entity, "Kayıt güncellendi");
        }

        public GeometryResponse Delete(int id)
        {
            var entity = _liste.FirstOrDefault(x => x.Id == id);
            if (entity == null)
                return new GeometryResponse { Success = false, Message = "Kayıt bulunamadı" };

            _liste.Remove(entity);
            return GeometryMapper.ToResponse(entity, "Kayıt silindi");
        }


        // --- GET PAGED (type + sayfalama) ---
        public GeometryListResponse GetPaged(string? type, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            try
            {
                // Mevcut GetAll() zaten var: tüm listeyi al
                var allRes = GetAll();
                if (allRes is null || allRes.Data is null)
                    return GeometryMapper.FailList("Liste alınamadı.");

                // GeometryResponse -> GeometryEntity
                var allEntities = allRes.Data
                    .Select(r => r?.Data)
                    .OfType<GeometryEntity>()
                    .ToList();

                // Filtre
                var query = allEntities.AsQueryable();
                if (!string.IsNullOrWhiteSpace(type) &&
                    !string.Equals(type, "ALL", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.Type == type);
                }

                // Sayfalama
                var total = query.Count();
                var pageData = query
                    .OrderByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // DTO’ya sar
                var data = pageData.Select(e => GeometryMapper.ToResponse(e, "Kayıt")).ToList();

                return new GeometryListResponse
                {
                    Success = true,
                    Message = "Liste getirildi.",
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return GeometryMapper.FailList("Liste hatası: " + ex.Message);
            }
        }

    }
}
