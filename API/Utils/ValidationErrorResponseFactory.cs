using API.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Utils
{
    public static class ValidationErrorResponseFactory
    {
        private const string InvalidValueMessage = "O valor indicado não é válido ou não está no formato correto.";
        private const string InvalidBodyMessage = "O pedido está vazio ou não é um JSON válido.";

        public static IActionResult Create(ActionContext context)
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .GroupBy(entry => FormatFieldName(entry.Key))
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .SelectMany(entry => IsDeserializationError(entry.Key)
                            ? new[] { string.IsNullOrEmpty(group.Key) ? InvalidBodyMessage : InvalidValueMessage }
                            : entry.Value!.Errors.Select(error => error.ErrorMessage))
                        .Distinct()
                        .ToArray());

            return new BadRequestObjectResult(
                ResponseDTO<Dictionary<string, string[]>>.Fail("Existem campos inválidos.", errors, StatusCodes.Status400BadRequest));
        }

        private static bool IsDeserializationError(string key) => key.Length == 0 || key.StartsWith('$');

        private static string FormatFieldName(string key)
        {
            var fieldName = key.StartsWith("$.") ? key[2..] : key.TrimStart('$');

            return JsonNamingPolicy.CamelCase.ConvertName(fieldName);
        }
    }
}
