using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Entities;

namespace LocalRAG.Controllers
{
    [ApiController]
    [Route("api/conventions/{conventionId}/features")]
    public class FeaturesController : ControllerBase
    {
        private readonly ConventionDbContext _context;
        private readonly ILogger<FeaturesController> _logger;

        public FeaturesController(
            ConventionDbContext context,
            ILogger<FeaturesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Feature>>> GetFeatures(int conventionId)
        {
            try
            {
                var features = await _context.Features
                    .Where(f => f.ConventionId == conventionId)
                    .OrderBy(f => f.MenuName)
                    .ToListAsync();

                return Ok(new { features });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching features for convention {ConventionId}", conventionId);
                return StatusCode(500, "기능 목록을 불러오는 중 오류가 발생했습니다.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Feature>> GetFeature(int conventionId, int id)
        {
            try
            {
                var feature = await _context.Features
                    .FirstOrDefaultAsync(f => f.Id == id && f.ConventionId == conventionId);

                if (feature == null)
                {
                    return NotFound("기능을 찾을 수 없습니다.");
                }

                return Ok(feature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching feature {FeatureId}", id);
                return StatusCode(500, "기능 정보를 불러오는 중 오류가 발생했습니다.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Feature>> CreateFeature(
            int conventionId,
            [FromBody] Feature feature)
        {
            try
            {
                var conventionExists = await _context.Conventions
                    .AnyAsync(c => c.Id == conventionId);

                if (!conventionExists)
                {
                    return NotFound("행사를 찾을 수 없습니다.");
                }

                feature.ConventionId = conventionId;
                feature.CreatedAt = DateTime.UtcNow;
                feature.UpdatedAt = DateTime.UtcNow;

                _context.Features.Add(feature);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetFeature),
                    new { conventionId, id = feature.Id },
                    feature
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feature for convention {ConventionId}", conventionId);
                return StatusCode(500, "기능 생성 중 오류가 발생했습니다.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeature(
            int conventionId,
            int id,
            [FromBody] Feature feature)
        {
            if (id != feature.Id)
            {
                return BadRequest("ID가 일치하지 않습니다.");
            }

            try
            {
                var existingFeature = await _context.Features
                    .FirstOrDefaultAsync(f => f.Id == id && f.ConventionId == conventionId);

                if (existingFeature == null)
                {
                    return NotFound("기능을 찾을 수 없습니다.");
                }

                existingFeature.MenuName = feature.MenuName;
                existingFeature.MenuUrl = feature.MenuUrl;
                existingFeature.IsActive = feature.IsActive;
                existingFeature.IconUrl = feature.IconUrl;
                existingFeature.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feature {FeatureId}", id);
                return StatusCode(500, "기능 수정 중 오류가 발생했습니다.");
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<Feature>> ToggleFeatureStatus(
            int conventionId,
            int id,
            [FromBody] FeatureStatusUpdate statusUpdate)
        {
            try
            {
                var feature = await _context.Features
                    .FirstOrDefaultAsync(f => f.Id == id && f.ConventionId == conventionId);

                if (feature == null)
                {
                    return NotFound("기능을 찾을 수 없습니다.");
                }

                feature.IsActive = statusUpdate.IsActive;
                feature.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(feature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling feature status {FeatureId}", id);
                return StatusCode(500, "기능 상태 변경 중 오류가 발생했습니다.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeature(int conventionId, int id)
        {
            try
            {
                var feature = await _context.Features
                    .FirstOrDefaultAsync(f => f.Id == id && f.ConventionId == conventionId);

                if (feature == null)
                {
                    return NotFound("기능을 찾을 수 없습니다.");
                }

                _context.Features.Remove(feature);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feature {FeatureId}", id);
                return StatusCode(500, "기능 삭제 중 오류가 발생했습니다.");
            }
        }
    }

    public class FeatureStatusUpdate
    {
        public bool IsActive { get; set; }
    }
}
