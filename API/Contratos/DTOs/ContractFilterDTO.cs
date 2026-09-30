using API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Contratos.DTOs
{
    public class ContractFilterDTO
    {
        [EnumDataType(typeof(ContractStatus), ErrorMessage = "O estado indicado não existe.")]
        public ContractStatus? Status { get; set; }

        public int? VehicleId { get; set; }

        public int? ClientId { get; set; }
    }
}
