using BasarStajApp_New.DTOs;
using BasarStajApp_New.Entity;
using BasarStajApp_New.Interfaces;
using BasarStajApp_New.Mappers;
using BasarStajApp_New.Repositories;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BasarStajApp_New.Services
{
    public class GeometryEfCoreService : IGeometryService
    {
        private readonly IUnitOfWork _uow;

        public GeometryEfCoreService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // --- GET ALL ---
        public GeometryListResponse GetAll()
        {
            try
            {
                var repo = _uow.Repository<GeometryEntity>();
                var list = repo.GetAllAsync().GetAwaiter().GetResult();

                return GeometryMapper.ToListResponse(list, "Veriler getirildi");
            }
            catch (Exception ex)
            {
                return GeometryMapper.FailList($"Listeleme hatası: {ex.Message}");
            }
        }

        // --- GET BY ID ---
        public GeometryResponse GetById(int id)
        {
            try
            {
                var repo = _uow.Repository<GeometryEntity>();
                var entity = repo.GetByIdAsync(id).GetAwaiter().GetResult();

                if (entity is null)
                    return GeometryMapper.Fail("Kayıt bulunamadı");

                return GeometryMapper.ToResponse(entity, "Kayıt getirildi");
            }
            catch (Exception ex)
            {
                return GeometryMapper.Fail($"Getirme hatası: {ex.Message}");
            }
        }

        // --- ADD (single) ---
        public GeometryResponse Add(GeometryDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Name) || string.IsNullOrWhiteSpace(dto?.WKT))
                return GeometryMapper.Fail("Geçersiz veri");

            try
            {
                // DTO -> Entity (SRID 4326)
                var entity = GeometryMapper.ToEntity(dto);
                if (entity.Feature != null && entity.Feature.SRID == 0)
                    entity.Feature.SRID = 4326;

                var repo = _uow.Repository<GeometryEntity>();
                repo.AddAsync(entity).GetAwaiter().GetResult();
                _uow.CommitAsync().GetAwaiter().GetResult();

                return GeometryMapper.ToResponse(entity, "Kayıt eklendi");
            }
            catch (ParseException)
            {
                return GeometryMapper.Fail("Geçersiz WKT");
            }
            catch (Exception ex)
            {
                return GeometryMapper.Fail($"Ekleme hatası: {ex.Message}");
            }
        }

        // --- ADD RANGE ---
        public GeometryListResponse AddRange(List<GeometryDTO> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return GeometryMapper.FailList("Boş liste");

            try
            {
                var entities = new List<GeometryEntity>();
                foreach (var dto in dtos.Where(d => !string.IsNullOrWhiteSpace(d.Name) && !string.IsNullOrWhiteSpace(d.WKT)))
                {
                    var e = GeometryMapper.ToEntity(dto);
                    if (e.Feature != null && e.Feature.SRID == 0)
                        e.Feature.SRID = 4326;

                    entities.Add(e);
                }

                if (entities.Count == 0)
                    return GeometryMapper.FailList("Geçerli kayıt yok");

                var repo = _uow.Repository<GeometryEntity>();
                repo.AddRangeAsync(entities).GetAwaiter().GetResult();
                _uow.CommitAsync().GetAwaiter().GetResult();

                return GeometryMapper.ToListResponse(entities, "Toplu kayıt eklendi");
            }
            catch (ParseException)
            {
                return GeometryMapper.FailList("Geçersiz WKT tespit edildi");
            }
            catch (Exception ex)
            {
                return GeometryMapper.FailList($"Toplu ekleme hatası: {ex.Message}");
            }
        }

        // --- UPDATE ---
        public GeometryResponse Update(int id, GeometryDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Name) || string.IsNullOrWhiteSpace(dto?.WKT))
                return GeometryMapper.Fail("Geçersiz veri");

            try
            {
                var repo = _uow.Repository<GeometryEntity>();
                var entity = repo.GetByIdAsync(id).GetAwaiter().GetResult();
                if (entity is null)
                    return GeometryMapper.Fail("Kayıt bulunamadı");

                // WKT -> Geometry
                var updated = GeometryMapper.ToEntity(dto, id);
                if (updated.Feature != null && updated.Feature.SRID == 0)
                    updated.Feature.SRID = 4326;

                entity.Name = updated.Name;
                entity.Feature = updated.Feature;

                repo.Update(entity);
                _uow.CommitAsync().GetAwaiter().GetResult();

                return GeometryMapper.ToResponse(entity, "Kayıt güncellendi");
            }
            catch (ParseException)
            {
                return GeometryMapper.Fail("Geçersiz WKT");
            }
            catch (Exception ex)
            {
                return GeometryMapper.Fail($"Güncelleme hatası: {ex.Message}");
            }
        }

        // --- DELETE ---
        public GeometryResponse Delete(int id)
        {
            try
            {
                var repo = _uow.Repository<GeometryEntity>();
                var entity = repo.GetByIdAsync(id).GetAwaiter().GetResult();
                if (entity is null)
                    return GeometryMapper.Fail("Kayıt bulunamadı");

                repo.Delete(entity);
                _uow.CommitAsync().GetAwaiter().GetResult();

                return GeometryMapper.ToResponse(entity, "Kayıt silindi");
            }
            catch (Exception ex)
            {
                return GeometryMapper.Fail($"Silme hatası: {ex.Message}");
            }
        }

        // --- GET PAGED (type + sayfalama) ---
        public GeometryListResponse GetPaged(string? type, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            try
            {
                var repo = _uow.Repository<GeometryEntity>();

                // Repository IQueryable vermediği için tüm listeyi çekiyoruz
                var all = repo.GetAllAsync().GetAwaiter().GetResult();
                var query = all.AsQueryable();

                // type normalizasyonu
                var t = (type ?? "").Trim();
                var isAll = string.IsNullOrEmpty(t)
                            || t.Equals("ALL", StringComparison.OrdinalIgnoreCase)
                            || t.Equals("Hepsi", StringComparison.OrdinalIgnoreCase);

                // Filtre (ALL/boş değilse)
                if (!isAll)
                    query = query.Where(x => x.Type != null && x.Type.Equals(t, StringComparison.OrdinalIgnoreCase));

                var total = query.Count();

                var pageData = query
                    .OrderByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var data = pageData.Select(e => GeometryMapper.ToResponse(e, "Kayıt")).ToList();

                return new GeometryListResponse
                {
                    Success = true,
                    Message = "Liste getirildi.",
                    Data = data,
                    Total = total,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                return new GeometryListResponse
                {
                    Success = false,
                    Message = "Liste hatası: " + ex.Message,
                    Data = new List<GeometryResponse>(),
                    Total = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
        }


    }
}
