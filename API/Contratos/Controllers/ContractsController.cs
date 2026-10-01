using API.Contratos.DTOs;
using API.Contratos.Interfaces;
using API.Models.Enums;
using API.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Contratos.Controllers
{
    [Route("api/contracts")]
    [ApiController]
    [Authorize]
    [ProducesErrorResponseType(typeof(ResponseDTO<string>))]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService contractService;

        public ContractsController(IContractService contractService)
        {
            this.contractService = contractService;
        }

        [HttpGet]
        [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Employee)}")]
        [ProducesResponseType(typeof(ResponseDTO<ICollection<ContractDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<ICollection<ContractDTO>>>> GetAll([FromQuery] ContractFilterDTO filters)
        {
            var contracts = await contractService.GetAllAsync(filters);

            return Ok(ResponseDTO<ICollection<ContractDTO>>.Ok("Contratos obtidos com sucesso.", contracts, StatusCodes.Status200OK));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Employee)}")]
        [ProducesResponseType(typeof(ResponseDTO<ContractDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<ContractDTO>>> GetById(int id)
        {
            var contract = await contractService.GetByIdAsync(id);

            return Ok(ResponseDTO<ContractDTO>.Ok("Contrato obtido com sucesso.", contract, StatusCodes.Status200OK));
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Employee)}")]
        [ProducesResponseType(typeof(ResponseDTO<ContractDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<ResponseDTO<ContractDTO>>> Create([FromBody] CreateContractDTO contractDTO)
        {
            var contract = await contractService.CreateAsync(contractDTO);

            return CreatedAtAction(nameof(GetById), new { id = contract.ContractId },
                ResponseDTO<ContractDTO>.Ok("Contrato criado com sucesso.", contract, StatusCodes.Status201Created));
        }

        [HttpPatch("{id:int}/return")]
        [Authorize(Roles = $"{nameof(Role.Manager)},{nameof(Role.Employee)}")]
        [ProducesResponseType(typeof(ResponseDTO<ContractDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<Dictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<ContractDTO>>> Return(int id, [FromBody] ReturnContractDTO returnDTO)
        {
            var contract = await contractService.ReturnAsync(id, returnDTO);

            return Ok(ResponseDTO<ContractDTO>.Ok("Devolução registada com sucesso.", contract, StatusCodes.Status200OK));
        }

        [HttpPatch("{id:int}/cancel")]
        [Authorize(Roles = nameof(Role.Manager))]
        [ProducesResponseType(typeof(ResponseDTO<ContractDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<ContractDTO>>> Cancel(int id)
        {
            var contract = await contractService.CancelAsync(id);

            return Ok(ResponseDTO<ContractDTO>.Ok("Contrato cancelado com sucesso.", contract, StatusCodes.Status200OK));
        }
    }
}
