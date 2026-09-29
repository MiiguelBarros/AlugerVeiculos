using API.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Utils
{
    public static class ValidationErrorResponseFactory
    {
        public static IActionResult Create(ActionContext context)
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => JsonNamingPolicy.CamelCase.ConvertName(entry.Key),
                    entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            return new BadRequestObjectResult(
                ResponseDTO<Dictionary<string, string[]>>.Fail("Existem campos inválidos.", errors, StatusCodes.Status400BadRequest));
        }
    }
}
