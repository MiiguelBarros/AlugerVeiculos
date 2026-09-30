using API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Clientes.DTOs
{
    public class ClientFilterDTO
    {
        [EnumDataType(typeof(RecordStatus), ErrorMessage = "O estado indicado não existe.")]
        public RecordStatus? Status { get; set; }
    }
}
