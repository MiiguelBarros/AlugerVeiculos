using API.Shared.DTOs;
using API.Shared.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace API.Utils
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionHandlingMiddleware> logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    logger.LogError(ex, "An exception occurred after the response had started.");
                    throw;
                }

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            int statusCode;
            string message;

            switch (exception)
            {
                case NotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    message = exception.Message;
                    break;

                case ConflictException:
                    statusCode = StatusCodes.Status409Conflict;
                    message = exception.Message;
                    break;

                case BusinessRuleException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = exception.Message;
                    break;

                case UnauthorizedException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = exception.Message;
                    break;

                case ForbiddenException:
                    statusCode = StatusCodes.Status403Forbidden;
                    message = exception.Message;
                    break;

                case DbUpdateConcurrencyException:
                    statusCode = StatusCodes.Status409Conflict;
                    message = "Este registo foi alterado por outro utilizador. Atualize a página e tente novamente.";
                    break;

                case DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } }:
                    statusCode = StatusCodes.Status409Conflict;
                    message = "Já existe um registo com este valor.";
                    break;

                case DbUpdateException { InnerException: SqlException { Number: 547 } }:
                    statusCode = StatusCodes.Status409Conflict;
                    message = "Não é possível concluir a operação porque o registo está associado a outros dados.";
                    break;

                case DbUpdateException { InnerException: SqlException { Number: 1205 } }:
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                    message = "Não foi possível concluir a operação devido a um conflito. Tente novamente.";
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "Ocorreu um erro inesperado.";
                    break;
            }

            if (statusCode >= StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", context.Request.Method, context.Request.Path);
            else
                logger.LogWarning("{ExceptionType} while processing {Method} {Path}: {Message}", exception.GetType().Name, context.Request.Method, context.Request.Path, exception.Message);

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(ResponseDTO<string>.Fail(message, statusCode));
        }
    }
}
