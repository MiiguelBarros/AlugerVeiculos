using API.Clientes.DTOs;
using API.Clientes.Interfaces;
using API.Models.Enums;
using API.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Clientes.Controllers
{
    [Route("api/clients")]
    [ApiController]
    [Authorize]
    [ProducesErrorResponseType(typeof(ResponseDTO<string>))]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService clientService;

        public ClientsController(IClientService clientService)
        {
            this.clientService = clientService;
        }

        [HttpGet]
        [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Employee)}")]
        [ProducesResponseType(typeof(ResponseDTO<ICollection<ClientDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<ICollection<ClientDTO>>>> GetAll([FromQuery] ClientFilterDTO filters)
        {
            var clients = await clientService.GetAllAsync(filters);

            return Ok(ResponseDTO<ICollection<ClientDTO>>.Ok("Clientes obtidos com sucesso.", clients, StatusCodes.Status200OK));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Employee)}")]
        [ProducesResponseType(typeof(ResponseDTO<ClientDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<ClientDTO>>> GetById(int id)
        {
            var client = await clientService.GetByIdAsync(id);

            return Ok(ResponseDTO<ClientDTO>.Ok("Cliente obtido com sucesso.", client, StatusCodes.Status200OK));
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Employee)}")]
        [ProducesResponseType(typeof(ResponseDTO<ClientDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<ClientDTO>>> Create([FromBody] ClientRequestDTO clientDTO)
        {
            var client = await clientService.CreateAsync(clientDTO);

            return CreatedAtAction(nameof(GetById), new { id = client.ClientId },
                ResponseDTO<ClientDTO>.Ok("Cliente registado com sucesso.", client, StatusCodes.Status201Created));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Employee)}")]
        [ProducesResponseType(typeof(ResponseDTO<ClientDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<ClientDTO>>> Update(int id, [FromBody] ClientRequestDTO clientDTO)
        {
            var client = await clientService.UpdateAsync(id, clientDTO);

            return Ok(ResponseDTO<ClientDTO>.Ok("Cliente atualizado com sucesso.", client, StatusCodes.Status200OK));
        }

        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Roles = nameof(Role.Manager))]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<string>>> Deactivate(int id)
        {
            await clientService.DeactivateAsync(id);

            return Ok(ResponseDTO<string>.Ok("Cliente desativado com sucesso.", StatusCodes.Status200OK));
        }

        [HttpPatch("{id:int}/reactivate")]
        [Authorize(Roles = nameof(Role.Manager))]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<string>>> Reactivate(int id)
        {
            await clientService.ReactivateAsync(id);

            return Ok(ResponseDTO<string>.Ok("Cliente reativado com sucesso.", StatusCodes.Status200OK));
        }
    }
}
