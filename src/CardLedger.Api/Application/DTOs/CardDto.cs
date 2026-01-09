namespace CardLedger.Api.Application.DTOs;

/// <summary>
/// DTO representing a card.
/// </summary>
public sealed record CardDto(Guid CardId, string CardNumber, string CreditLimitFormatted);
