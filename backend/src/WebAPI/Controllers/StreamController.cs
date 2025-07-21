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
            var range = Request.Headers["Range"].ToString();

            var (stream, start, end, total) = await _streamService.GetAudioChunkAsync(fileKey, range);

            Response.StatusCode = StatusCodes.Status206PartialContent;
            Response.ContentType = "audio/mpeg";
            Response.ContentLength = end - start + 1;
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{total}");

            await stream.CopyToAsync(Response.Body);
            return new EmptyResult();
        }
    }
}
