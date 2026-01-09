using System.Globalization;
using CardLedger.Api.Application.DTOs;
using CardLedger.Api.Infrastructure;
using MediatR;
using MoneyDataType;

namespace CardLedger.Api.Application.Commands;

/// <summary>
/// Handles creating cards.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateCardHandler"/> class.
/// </remarks>
public sealed class CreateCardHandler(ICardLedgerRepository repo) : IRequestHandler<CreateCardCommand, CardDto>
{
    private readonly ICardLedgerRepository _repo = repo;

    /// <summary>
    /// Creates a card after validating inputs.
    /// </summary>
    public async Task<CardDto> Handle(CreateCardCommand request, CancellationToken ct)
    {
        // Basic validation here; in a larger codebase we'd use FluentValidation.
        var cardNumber = (request.CardNumber ?? "").Trim();
        if (cardNumber.Length != 16 || !cardNumber.All(char.IsDigit))
        {
            throw new ArgumentException("Card number must be a 16-digit numeric string.");
        }

        if (request.CreditLimit <= 0)
        {
            throw new ArgumentException("Credit limit must be positive.");
        }

        if (!string.Equals(request.CurrencyCode, "USD", StringComparison.OrdinalIgnoreCase))
        {
           throw new ArgumentException("Credit limit currency must be USD.");
        }

        var creditLimit = Money.Create(request.CreditLimit, request.CurrencyCode).WithScale(2);        
        var card = await _repo.CreateCardAsync(cardNumber, creditLimit, ct);

        return new CardDto(card.CardId, card.CardNumber, MoneyFormatting.Format(card.CreditLimit.WithScale(2)));
    }

}
