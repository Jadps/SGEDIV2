using BACKSGEDI.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BACKSGEDI.Infrastructure.Extensions;

public static class ResultExtensions
{
    public static IResult ToResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return TypedResults.NoContent();
        }

        return MapErrorToResult(result.Error);
    }

    public static IResult ToResult<TValue>(this Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value);
        }

        return MapErrorToResult(result.Error);
    }

    private static IResult MapErrorToResult(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return TypedResults.Problem(
            detail: error.Description,
            statusCode: statusCode,
            title: GetTitle(error.Type),
            extensions: new Dictionary<string, object?>
            {
                { "code", error.Code }
            });
    }

    private static string GetTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Error de Validación",
        ErrorType.Unauthorized => "No Autorizado",
        ErrorType.Conflict => "Conflicto de Datos",
        ErrorType.NotFound => "Recurso no Encontrado",
        _ => "Error en la Operación"
    };
}
