using CardLedger.Api.Application.DTOs;
using MediatR;

namespace CardLedger.Api.Application.Queries;

/// <summary>
/// Query to get available balance in a target currency.
/// </summary>
public sealed record GetAvailableBalanceQuery(Guid CardId, string TargetCurrency) : IRequest<AvailableBalanceDto>;
