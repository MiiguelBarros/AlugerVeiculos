using API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Veiculos.DTOs
{
    public class VehicleFilterDTO
    {
        [EnumDataType(typeof(RecordStatus), ErrorMessage = "O estado indicado não existe.")]
        public RecordStatus? Status { get; set; }

        [EnumDataType(typeof(VehicleAvailability), ErrorMessage = "A disponibilidade indicada não existe.")]
        public VehicleAvailability? Availability { get; set; }
    }
}
