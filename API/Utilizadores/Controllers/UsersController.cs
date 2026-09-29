using API.Models.Enums;
using API.Shared.DTOs;
using API.Utilizadores.DTOs;
using API.Utilizadores.Interfaces;
using API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Utilizadores.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesErrorResponseType(typeof(ResponseDTO<string>))]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ResponseDTO<ICollection<UserDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<ICollection<UserDTO>>>> GetAll()
        {
            var users = await userService.GetAllAsync();

            return Ok(ResponseDTO<ICollection<UserDTO>>.Ok("Utilizadores obtidos com sucesso.", users, StatusCodes.Status200OK));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ResponseDTO<UserDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<UserDTO>>> GetById(int id)
        {
            var user = await userService.GetByIdAsync(id);

            return Ok(ResponseDTO<UserDTO>.Ok("Utilizador obtido com sucesso.", user, StatusCodes.Status200OK));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseDTO<UserDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<UserDTO>>> Create([FromBody] CreateUserDTO createUserDTO)
        {
            var user = await userService.CreateAsync(createUserDTO);

            return CreatedAtAction(nameof(GetById), new { id = user.UserId },
                ResponseDTO<UserDTO>.Ok("Utilizador criado com sucesso.", user, StatusCodes.Status201Created));
        }

        [HttpPatch("{id:int}/deactivate")]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<string>>> Deactivate(int id)
        {
            await userService.DeactivateAsync(id, User.GetUserId());

            return Ok(ResponseDTO<string>.Ok("Utilizador desativado com sucesso.", StatusCodes.Status200OK));
        }

        [HttpPatch("{id:int}/reactivate")]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<string>>> Reactivate(int id)
        {
            await userService.ReactivateAsync(id);

            return Ok(ResponseDTO<string>.Ok("Utilizador reativado com sucesso.", StatusCodes.Status200OK));
        }
    }
}
