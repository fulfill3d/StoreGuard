using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StoreGuard.Microservices.VideoStream.Data.Models;

namespace StoreGuard.Microservices.VideoStream
{
    [ApiController]
    [Route("api/signaling")]
    public class SignalingController(IHubContext<VideoSignalingHub> hubContext) : ControllerBase
    {
        [HttpPost("offer")]
        public async Task<IActionResult> SendOffer([FromBody] SignalingMessage message)
        {
            await hubContext.Clients.All.SendAsync("ReceiveOffer", message.Data);
            return Ok();
        }

        [HttpPost("answer")]
        public async Task<IActionResult> SendAnswer([FromBody] SignalingMessage message)
        {
            await hubContext.Clients.All.SendAsync("ReceiveAnswer", message.Data);
            return Ok();
        }

        [HttpPost("candidate")]
        public async Task<IActionResult> SendCandidate([FromBody] SignalingMessage message)
        {
            await hubContext.Clients.All.SendAsync("ReceiveCandidate", message.Data);
            return Ok();
        }
    }
}


