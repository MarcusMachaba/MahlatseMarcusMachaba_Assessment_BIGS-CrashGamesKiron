using KironTest.API.ServiceHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KironTest.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DragonBallCharactersController : ControllerBase
    {
        private readonly IDragonBallCharacterService _service;
        public DragonBallCharactersController(IDragonBallCharacterService service)
        {
            _service = service;
        }


        [HttpGet("characters")]
        public async Task<IActionResult> GetCharacters()
        {
            var characters = await _service.GetCharactersAsync();
            return Ok(characters);
        }
    }
}
