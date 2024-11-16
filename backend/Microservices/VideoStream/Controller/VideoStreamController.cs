using Microsoft.AspNetCore.Mvc;

namespace StoreGuard.Microservices.VideoStream
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoStreamController() : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] byte[] videoData)
        {
            if (videoData == null || videoData.Length == 0)
                return BadRequest("No video data received.");

            try
            {
                return Ok("Video data sent to Event Hub");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to Event Hub: {ex.Message}");
                return StatusCode(500, "Failed to send video data");
            }
        }
    }
}