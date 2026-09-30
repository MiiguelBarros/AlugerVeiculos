using API.Models.Enums;
using API.Shared.DTOs;
using API.Veiculos.DTOs;
using API.Veiculos.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Veiculos.Controllers
{
    [Route("api/vehicles")]
    [ApiController]
    [Authorize]
    [ProducesErrorResponseType(typeof(ResponseDTO<string>))]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            this.vehicleService = vehicleService;
        }

        [HttpGet]
        [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Employee)}")]
        [ProducesResponseType(typeof(ResponseDTO<ICollection<VehicleDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<ICollection<VehicleDTO>>>> GetAll([FromQuery] VehicleFilterDTO filters)
        {
            var vehicles = await vehicleService.GetAllAsync(filters);

            return Ok(ResponseDTO<ICollection<VehicleDTO>>.Ok("Veículos obtidos com sucesso.", vehicles, StatusCodes.Status200OK));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Employee)}")]
        [ProducesResponseType(typeof(ResponseDTO<VehicleDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<VehicleDTO>>> GetById(int id)
        {
            var vehicle = await vehicleService.GetByIdAsync(id);

            return Ok(ResponseDTO<VehicleDTO>.Ok("Veículo obtido com sucesso.", vehicle, StatusCodes.Status200OK));
        }

        [HttpPost]
        [Authorize(Roles = nameof(Role.Manager))]
        [ProducesResponseType(typeof(ResponseDTO<VehicleDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<VehicleDTO>>> Create([FromBody] VehicleRequestDTO vehicleDTO)
        {
            var vehicle = await vehicleService.CreateAsync(vehicleDTO);

            return CreatedAtAction(nameof(GetById), new { id = vehicle.VehicleId },
                ResponseDTO<VehicleDTO>.Ok("Veículo registado com sucesso.", vehicle, StatusCodes.Status201Created));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = nameof(Role.Manager))]
        [ProducesResponseType(typeof(ResponseDTO<VehicleDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<VehicleDTO>>> Update(int id, [FromBody] VehicleRequestDTO vehicleDTO)
        {
            var vehicle = await vehicleService.UpdateAsync(id, vehicleDTO);

            return Ok(ResponseDTO<VehicleDTO>.Ok("Veículo atualizado com sucesso.", vehicle, StatusCodes.Status200OK));
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
            await vehicleService.DeactivateAsync(id);

            return Ok(ResponseDTO<string>.Ok("Veículo desativado com sucesso.", StatusCodes.Status200OK));
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
            await vehicleService.ReactivateAsync(id);

            return Ok(ResponseDTO<string>.Ok("Veículo reativado com sucesso.", StatusCodes.Status200OK));
        }
    }
}
