using CardLedger.Api.Application.DTOs;
using MediatR;

namespace CardLedger.Api.Application.Queries;

/// <summary>
/// Query to get a purchase in a target currency.
/// </summary>
public sealed record GetPurchaseQuery(Guid PurchaseId, string TargetCurrency) : IRequest<PurchaseDto>;
