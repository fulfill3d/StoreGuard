using Microsoft.AspNetCore.SignalR;
using StoreGuard.Microservices.VideoStream.Service.Interfaces;

namespace StoreGuard.Microservices.VideoStream
{
    public class VideoSignalingHub(IVideoStreamService videoStreamService) : Hub
    {
        public async Task SendOffer(string offer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(offer))
                {
                    Console.WriteLine("Error: Received empty offer");
                    throw new HubException("Offer cannot be empty");
                }
                
                Console.WriteLine("Received Offer");
                await Clients.Others.SendAsync("ReceiveOffer", offer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendOffer: {ex.Message}");
                throw;
            }
        }

        public async Task SendAnswer(string answer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(answer))
                {
                    Console.WriteLine("Error: Received empty answer");
                    throw new HubException("Answer cannot be empty");
                }

                Console.WriteLine("Received Answer");
                await Clients.Others.SendAsync("ReceiveAnswer", answer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendAnswer: {ex.Message}");
                throw;
            }
        }

        public async Task SendIceCandidate(string candidate)
        {
            try
            {
                Console.WriteLine("Received ICE Candidate");
                await Clients.Others.SendAsync("ReceiveCandidate", candidate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendIceCandidate: {ex.Message}");
                throw;
            }
        }

        public async Task SendVideoData(string base64Data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(base64Data))
                {
                    Console.WriteLine("Error: Received empty video data");
                    return;
                }

                // Step 1: Decode Base64 to byte array
                byte[] videoData;
                try
                {
                    videoData = Convert.FromBase64String(base64Data);
                }
                catch (FormatException fe)
                {
                    Console.WriteLine("Error decoding Base64 data: " + fe.Message);
                    return;
                }

                Console.WriteLine($"Received video data chunk of length: {videoData.Length} bytes");

                // Step 2: Check if the video data is valid
                if (videoData.Length == 0)
                {
                    Console.WriteLine("Error: Decoded video data is empty");
                    return;
                }

                await videoStreamService.SendVideoDataAsync(videoData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled error in SendVideoData: {ex.Message}");
                throw new HubException($"Server error in SendVideoData: {ex.Message}");
            }
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client disconnected. Reason: {exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
