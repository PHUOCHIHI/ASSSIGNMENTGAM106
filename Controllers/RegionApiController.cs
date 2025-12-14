using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;

namespace Minecraft.Controllers
{
    [ApiController]
    [Route("api/regions")]
    public class RegionApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegionApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRegions()
        {
            try
            {
                var regions = await _context.Regions.ToListAsync();
                return Json(new ResponseAPI { Success = true, Data = regions });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRegion(int id)
        {
            try
            {
                var region = await _context.Regions.FindAsync(id);
                if (region == null)
                    return Json(new ResponseAPI { Success = false, Message = "Region not found" });

                return Json(new ResponseAPI { Success = true, Data = region });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        public class CreateRegionRequest
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRegion([FromBody] CreateRegionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Json(new ResponseAPI { Success = false, Message = "Name is required" });

            try
            {
                var name = request.Name.Trim();

                if (await _context.Regions.AnyAsync(r => r.Name == name))
                    return Json(new ResponseAPI { Success = false, Message = "Region name already exists" });

                var region = new Region
                {
                    Name = name,
                    Description = request.Description
                };

                _context.Regions.Add(region);
                await _context.SaveChangesAsync();

                return Json(new ResponseAPI { Success = true, Message = "Region created successfully", Data = region });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        public class UpdateRegionRequest
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRegion(int id, [FromBody] UpdateRegionRequest request)
        {
            if (id <= 0)
                return Json(new ResponseAPI { Success = false, Message = "Invalid id" });

            if (string.IsNullOrWhiteSpace(request.Name))
                return Json(new ResponseAPI { Success = false, Message = "Name is required" });

            try
            {
                var region = await _context.Regions.FindAsync(id);
                if (region == null)
                    return Json(new ResponseAPI { Success = false, Message = "Region not found" });

                var name = request.Name.Trim();
                var nameExists = await _context.Regions.AnyAsync(r => r.RegionId != id && r.Name == name);
                if (nameExists)
                    return Json(new ResponseAPI { Success = false, Message = "Region name already exists" });

                region.Name = name;
                region.Description = request.Description;

                _context.Regions.Update(region);
                await _context.SaveChangesAsync();

                return Json(new ResponseAPI { Success = true, Message = "Region updated successfully", Data = region });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRegion(int id)
        {
            if (id <= 0)
                return Json(new ResponseAPI { Success = false, Message = "Invalid id" });

            try
            {
                var region = await _context.Regions.FindAsync(id);
                if (region == null)
                    return Json(new ResponseAPI { Success = false, Message = "Region not found" });

                _context.Regions.Remove(region);
                await _context.SaveChangesAsync();

                return Json(new ResponseAPI { Success = true, Message = "Region deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }
    }
}
