using CardLedger.Api.Application.DTOs;
using MediatR;

namespace CardLedger.Api.Application.Commands;

/// <summary>
/// Command to create a new card.
/// </summary>
public sealed class CreateCardCommand : IRequest<CardDto>
{
    /// <summary>
    /// Card number as a 16-digit numeric string.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Credit limit amount.
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Currency code for the credit limit.
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Constructor.
    /// </summary>
    public CreateCardCommand() { }
}
