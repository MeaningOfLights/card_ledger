using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using CardLedger.Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CardLedger.Api.Tests.Infrastructure;

public class ProblemDetailsExtensionsTests
{
    [Theory]
    [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest)]
    [InlineData(typeof(InvalidOperationException), StatusCodes.Status409Conflict)]
    [InlineData(typeof(KeyNotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(ValidationException), StatusCodes.Status422UnprocessableEntity)]
    [InlineData(typeof(Exception), StatusCodes.Status500InternalServerError)]
    public async Task ToProblem_MapsExceptionsToHttpStatus(Type exceptionType, int expectedStatus)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Failure")!;
        var result = exception.ToProblem();

        // Act
        var (statusCode, body) = await ExecuteAsync(result);

        // Assert
        Assert.Equal(expectedStatus, statusCode);
        Assert.Contains("Failure", body);
    }

    private static async Task<(int StatusCode, string Body)> ExecuteAsync(IResult result)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };
        await using var bodyStream = new MemoryStream();
        httpContext.Response.Body = bodyStream;

        await result.ExecuteAsync(httpContext);
        bodyStream.Position = 0;

        using var reader = new StreamReader(bodyStream);
        var body = await reader.ReadToEndAsync();
        return (httpContext.Response.StatusCode, body);
    }
}
