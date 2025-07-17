using Core.ApplicationModels.KironTestAPI;
using KironTest.API.ServiceHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

        /// <summary>
        /// Retrieves a list of Dragon Ball characters from the external API.
        /// This data is cached and updated hourly to minimize external traffic.
        /// </summary>
        /// <returns>A list of Dragon Ball character data.</returns>
        /// <response code="200">Returns the navigation tree</response>
        /// <response code="401">Unauthorized request</response>
        [HttpGet("characters")]
        [ProducesResponseType(typeof(List<DragonBallCharacter>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(HttpStatusCode), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetCharacters()
        {
            var characters = await _service.GetCharactersAsync();
            return Ok(characters);
        }
    }
}
