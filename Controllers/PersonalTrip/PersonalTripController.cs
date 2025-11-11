using LocalRAG.DTOs.PersonalTrip;
using LocalRAG.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LocalRAG.Controllers.PersonalTrip
{
    [Authorize]
    [ApiController]
    [Route("api/personal-trips")]
    public class PersonalTripController : ControllerBase
    {
        private readonly IPersonalTripService _personalTripService;

        public PersonalTripController(IPersonalTripService personalTripService)
        {
            _personalTripService = personalTripService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("사용자 정보를 확인할 수 없습니다.");
            }
            return userId;
        }

        /// <summary>
        /// 내 여행 목록 조회
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyTrips()
        {
            try
            {
                var userId = GetCurrentUserId();
                var trips = await _personalTripService.GetUserTripsAsync(userId);
                return Ok(trips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "여행 목록 조회에 실패했습니다.", error = ex.Message });
            }
        }

        /// <summary>
        /// 여행 상세 조회
        /// </summary>
        [HttpGet("{tripId}")]
        public async Task<IActionResult> GetTripById(int tripId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var trip = await _personalTripService.GetTripByIdAsync(tripId, userId);

                if (trip == null)
                    return NotFound(new { message = "여행을 찾을 수 없습니다." });

                return Ok(trip);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "여행 조회에 실패했습니다.", error = ex.Message });
            }
        }

        /// <summary>
        /// 새 여행 생성
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] CreatePersonalTripDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var trip = await _personalTripService.CreateTripAsync(dto, userId);
                return CreatedAtAction(nameof(GetTripById), new { tripId = trip.Id }, trip);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "여행 생성에 실패했습니다.", error = ex.Message });
            }
        }

        /// <summary>
        /// 여행 수정
        /// </summary>
        [HttpPut("{tripId}")]
        public async Task<IActionResult> UpdateTrip(int tripId, [FromBody] UpdatePersonalTripDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var trip = await _personalTripService.UpdateTripAsync(tripId, dto, userId);

                if (trip == null)
                    return NotFound(new { message = "여행을 찾을 수 없습니다." });

                return Ok(trip);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "여행 수정에 실패했습니다.", error = ex.Message });
            }
        }

        /// <summary>
        /// 여행 삭제
        /// </summary>
        [HttpDelete("{tripId}")]
        public async Task<IActionResult> DeleteTrip(int tripId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _personalTripService.DeleteTripAsync(tripId, userId);

                if (!success)
                    return NotFound(new { message = "여행을 찾을 수 없습니다." });

                return Ok(new { message = "여행이 삭제되었습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "여행 삭제에 실패했습니다.", error = ex.Message });
            }
        }

        /// <summary>
        /// 항공권 추가
        /// </summary>
        [HttpPost("{tripId}/flights")]
        public async Task<IActionResult> AddFlight(int tripId, [FromBody] CreateFlightDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var flight = await _personalTripService.AddFlightAsync(tripId, dto, userId);
                return Ok(flight);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "항공권 추가에 실패했습니다.", error = ex.Message });
            }
        }

        /// <summary>
        /// 항공권 수정
        /// </summary>
        [HttpPut("flights/{flightId}")]
        public async Task<IActionResult> UpdateFlight(int flightId, [FromBody] CreateFlightDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var flight = await _personalTripService.UpdateFlightAsync(flightId, dto, userId);

                if (flight == null)
                    return NotFound(new { message = "항공권을 찾을 수 없습니다." });

                return Ok(flight);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "항공권 수정에 실패했습니다.", error = ex.Message });
            }
        }

        /// <summary>
        /// 항공권 삭제
        /// </summary>
        [HttpDelete("flights/{flightId}")]
        public async Task<IActionResult> DeleteFlight(int flightId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _personalTripService.DeleteFlightAsync(flightId, userId);

                if (!success)
                    return NotFound(new { message = "항공권을 찾을 수 없습니다." });

                return Ok(new { message = "항공권이 삭제되었습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "항공권 삭제에 실패했습니다.", error = ex.Message });
            }
        }

        /// <summary>
        /// 숙소 추가
        /// </summary>
        [HttpPost("{tripId}/accommodations")]
        public async Task<IActionResult> AddAccommodation(int tripId, [FromBody] CreateAccommodationDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var accommodation = await _personalTripService.AddAccommodationAsync(tripId, dto, userId);
                return Ok(accommodation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "숙소 추가에 실패했습니다.", error = ex.Message });
            }
        }

        /// <summary>
        /// 숙소 수정
        /// </summary>
        [HttpPut("accommodations/{accommodationId}")]
        public async Task<IActionResult> UpdateAccommodation(int accommodationId, [FromBody] CreateAccommodationDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var accommodation = await _personalTripService.UpdateAccommodationAsync(accommodationId, dto, userId);

                if (accommodation == null)
                    return NotFound(new { message = "숙소를 찾을 수 없습니다." });

                return Ok(accommodation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "숙소 수정에 실패했습니다.", error = ex.Message });
            }
        }

        /// <summary>
        /// 숙소 삭제
        /// </summary>
        [HttpDelete("accommodations/{accommodationId}")]
        public async Task<IActionResult> DeleteAccommodation(int accommodationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _personalTripService.DeleteAccommodationAsync(accommodationId, userId);

                if (!success)
                    return NotFound(new { message = "숙소를 찾을 수 없습니다." });

                return Ok(new { message = "숙소가 삭제되었습니다." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "숙소 삭제에 실패했습니다.", error = ex.Message });
            }
        }
    }
}
