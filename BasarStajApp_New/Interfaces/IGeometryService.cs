using BasarStajApp_New.DTOs;
using System.Collections.Generic;

namespace BasarStajApp_New.Interfaces
{
    public interface IGeometryService
    {
        // --- GET ALL ---
        GeometryListResponse GetAll();

        // --- GET BY ID ---
        GeometryResponse GetById(int id);

        // --- ADD SINGLE ---
        GeometryResponse Add(GeometryDTO dto);

        // --- ADD MULTIPLE ---
        GeometryListResponse AddRange(List<GeometryDTO> dtos);

        // --- UPDATE ---
        GeometryResponse Update(int id, GeometryDTO dto);

        // --- DELETE ---
        GeometryResponse Delete(int id);

        // --- GET PAGED (type + sayfalama) ---
        GeometryListResponse GetPaged(string? type, int page, int pageSize);

    }
}
