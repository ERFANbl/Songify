using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MadeForUserController : ControllerBase
    {
        private readonly IMadeForUserService _mfuService;

        public MadeForUserController(IMadeForUserService mfuService)
        {
            _mfuService = mfuService;
        }


    }
}
