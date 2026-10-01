using Microsoft.AspNetCore.Mvc;
using MiniCrm.Application.Exceptions;
using MiniCrm.Domain.Exceptions;

namespace MiniCrm.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            ProblemDetails problema;
            switch (exception)
            {
                case DatosInvalidosException datos:
                    problema = new ValidationProblemDetails(datos.Errores)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = datos.Message
                    };
                    break;
                case RecursoNoEncontradoException:
                    problema = new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = exception.Message
                    };
                    break;
                case CuitDuplicadoException:
                    problema = new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Title = exception.Message
                    };
                    break;
                default:
                    _logger.LogError(exception, "Error al procesar la solicitud.");
                    problema = new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Ocurrió un error al procesar la solicitud. Intentá nuevamente."
                    };
                    break;
            }

            problema.Instance = context.Request.Path;
            context.Response.StatusCode = problema.Status!.Value;
            await context.Response.WriteAsJsonAsync((object)problema, options: null,
                contentType: "application/problem+json", cancellationToken: context.RequestAborted);
        }
    }
}
