using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CardLedger.Api.Infrastructure;

/// <summary>
/// Maps exceptions to RFC 7807 problem details responses.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Converts an exception to a ProblemDetails response.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <returns></returns>
    public static IResult ToProblem(this Exception ex)
    {
        var status = ex switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        var pd = new ProblemDetails
        {
            Status = status,
            Title = ex.GetType().Name,
            Detail = ex.Message
        };

        return Results.Problem(pd.Detail, statusCode: pd.Status, title: pd.Title);
    }
}

