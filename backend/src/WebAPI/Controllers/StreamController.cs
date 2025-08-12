using Application.Interfaces.Services;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {
        private readonly IStreamService _streamService;

        public StreamController(IStreamService streamService)
        {
            _streamService = streamService;
        }

        [Route("{fileKey}")]
        [HttpGet]
        public async Task<IActionResult> Stream([FromRoute] string fileKey)
        {
            var rangeHeader = Request.Headers["Range"].ToString();

            // If no range header, force the first chunk
            if (string.IsNullOrEmpty(rangeHeader))
            {
                // You can adjust this chunk size (here: first 1 MB)
                long chunkSize = 1 * 1024 * 1024; // 1 MB
                rangeHeader = $"bytes=0-{chunkSize - 1}";
            }

            var (stream, start, end, total) = await _streamService.GetAudioChunkAsync(fileKey, rangeHeader);

            Response.ContentType = "audio/mpeg";
            Response.Headers.Add("Accept-Ranges", "bytes");

            // Always partial content now
            Response.StatusCode = StatusCodes.Status206PartialContent;
            Response.ContentLength = end - start + 1;
            Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{total}");

            await stream.CopyToAsync(Response.Body);
            return new EmptyResult();
        }

    }
}
