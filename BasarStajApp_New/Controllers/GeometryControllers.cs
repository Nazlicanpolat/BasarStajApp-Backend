

using BasarStajApp_New.DTOs;
using BasarStajApp_New.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace BasarStajApp_New.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GeometryController : ControllerBase
    {
        private readonly IGeometryService _service;

        public GeometryController(IGeometryService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public ActionResult<GeometryListResponse> GetAll()
        {
            var response = _service.GetAll();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public ActionResult<GeometryResponse> GetById(int id)
        {
            var response = _service.GetById(id);
            return Ok(response);
        }

        [HttpPost]
        public ActionResult<GeometryResponse> Add([FromBody] GeometryDTO dto)
        {
            var response = _service.Add(dto);
            return Ok(response);
        }

        [HttpPost("batch")]
        public ActionResult<GeometryListResponse> AddRange([FromBody] List<GeometryDTO> dtos)
        {
            var response = _service.AddRange(dtos);
            return Ok(response);
        }

        [HttpPut("{id}")]
        public ActionResult<GeometryResponse> Update(int id, [FromBody] GeometryDTO dto)
        {
            var response = _service.Update(id, dto);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public ActionResult<GeometryResponse> Delete(int id)
        {
            var response = _service.Delete(id);
            return Ok(response);
        }


        // GET /api/Geometry/GetPaged?type=Point&page=1&pageSize=10
        [HttpGet]
        public IActionResult GetPaged([FromQuery] string? type = "ALL", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var res = _service.GetPaged(type, page, pageSize);
            return Ok(res);
        }

    }
}
