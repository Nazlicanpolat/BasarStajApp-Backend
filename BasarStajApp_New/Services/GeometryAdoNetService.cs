using System.Data;
using Npgsql;
using BasarStajApp_New.DTOs;
using BasarStajApp_New.Entity;
using BasarStajApp_New.Interfaces;
using BasarStajApp_New.Mappers;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace BasarStajApp_New.Services
{
    public class GeometryAdoNetService : IGeometryService
    {
        private readonly string _connectionString;

        public GeometryAdoNetService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string boş olamaz.", nameof(connectionString));
            _connectionString = connectionString;
        }

        // --- GET ALL ---
        public GeometryListResponse GetAll()
        {
            var list = new List<GeometryEntity>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Name, ST_AsText(WKT) FROM Geometry";
            using var cmd = new NpgsqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            var wktReader = new WKTReader();

            while (reader.Read())
            {
                list.Add(new GeometryEntity
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    WKT = reader.GetString(2)
                });
            }

            var responseList = list.Select(x => new GeometryResponse
            {
                Success = true,
                Message = "Kayıt getirildi",
                Data = x
            }).ToList();

            return new GeometryListResponse
            {
                Success = true,
                Message = "Veriler getirildi",
                Data = responseList
            };
        }

        // --- GET BY ID ---
        public GeometryResponse GetById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Name, ST_AsText(WKT) FROM Geometry WHERE Id=@id";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return new GeometryResponse { Success = false, Message = "Kayıt bulunamadı" };

            var entity = new GeometryEntity
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                WKT = reader.GetString(2)
            };

            return new GeometryResponse
            {
                Success = true,
                Message = "Kayıt getirildi",
                Data = entity
            };
        }

        // --- ADD SINGLE ---
        public GeometryResponse Add(GeometryDTO dto)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // WKT'yi Geometry objesi olarak yazmak
            var wktReader = new WKTReader();
            Geometry geom = wktReader.Read(dto.WKT);

            string sql = "INSERT INTO Geometry (Name, WKT) VALUES (@name, ST_GeomFromText(@wkt, 4326)) RETURNING Id";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("name", dto.Name);
            cmd.Parameters.AddWithValue("wkt", geom.AsText()); // Geometry → WKT string

            int newId = Convert.ToInt32(cmd.ExecuteScalar());

            var entity = new GeometryEntity
            {
                Id = newId,
                Name = dto.Name,
                WKT = dto.WKT
            };

            return new GeometryResponse
            {
                Success = true,
                Message = "Kayıt eklendi",
                Data = entity
            };
        }

        // --- ADD RANGE ---
        public GeometryListResponse AddRange(List<GeometryDTO> dtos)
        {
            var addedList = new List<GeometryEntity>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var wktReader = new WKTReader();

            foreach (var dto in dtos)
            {
                Geometry geom = wktReader.Read(dto.WKT);

                string sql = "INSERT INTO Geometry (Name, WKT) VALUES (@name, ST_GeomFromText(@wkt, 4326)) RETURNING Id";
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("name", dto.Name);
                cmd.Parameters.AddWithValue("wkt", geom.AsText());

                int newId = Convert.ToInt32(cmd.ExecuteScalar());

                addedList.Add(new GeometryEntity
                {
                    Id = newId,
                    Name = dto.Name,
                    WKT = dto.WKT
                });
            }

            var responseList = addedList.Select(x => new GeometryResponse
            {
                Success = true,
                Message = "Kayıt eklendi",
                Data = x
            }).ToList();

            return new GeometryListResponse
            {
                Success = true,
                Message = "Toplu kayıt eklendi",
                Data = responseList
            };
        }

        // --- UPDATE ---
        public GeometryResponse Update(int id, GeometryDTO dto)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var wktReader = new WKTReader();
            Geometry geom = wktReader.Read(dto.WKT);

            string sql = "UPDATE Geometry SET Name=@name, WKT=ST_GeomFromText(@wkt, 4326) WHERE Id=@id";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("name", dto.Name);
            cmd.Parameters.AddWithValue("wkt", geom.AsText());

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
                return new GeometryResponse { Success = false, Message = "Kayıt bulunamadı" };

            return new GeometryResponse
            {
                Success = true,
                Message = "Kayıt güncellendi",
                Data = new GeometryEntity
                {
                    Id = id,
                    Name = dto.Name,
                    WKT = dto.WKT
                }
            };
        }

        // --- DELETE ---
        public GeometryResponse Delete(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = "DELETE FROM Geometry WHERE Id=@id";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("id", id);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
                return new GeometryResponse { Success = false, Message = "Kayıt bulunamadı" };

            return new GeometryResponse
            {
                Success = true,
                Message = "Kayıt silindi"
            };
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
