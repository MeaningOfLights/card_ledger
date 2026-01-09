using CardLedger.Api.Application.Commands;
using CardLedger.Api.Application.DTOs;
using CardLedger.Api.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CardLedger.Api.Tests.Controllers;

public class CardLedgerControllerTests
{
    [Fact]
    public async Task CreateCard_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var controller = new CardLedgerController(mediator.Object);
        var cardId = Guid.NewGuid();
        var dto = new CardDto(cardId, "1234567812345678", "$1,000.00");
        var request = new CreateCardRequest
        {
            CardNumber = "1234567812345678",
            CreditLimit = 1000m,
            CurrencyCode = "USD"
        };

        mediator.Setup(m => m.Send(It.IsAny<CreateCardCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await controller.CreateCard(request, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(CardLedgerController.GetAvailableBalance), created.ActionName);
        Assert.Equal(dto, created.Value);
        mediator.VerifyAll();
    }

    [Fact]
    public async Task CreatePurchase_HeaderIdempotencyKey_UsesProvidedKey()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var controller = new CardLedgerController(mediator.Object);
        var cardId = Guid.NewGuid();
        var idempotencyKey = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var request = new CreatePurchaseRequest
        {
            Description = "Lunch",
            TransactionDate = DateTimeOffset.UtcNow,
            Amount = 20m,
            CurrencyCode = "USD"
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.Request.Headers["Idempotency-Key"] = idempotencyKey.ToString();

        mediator.Setup(m => m.Send(
                It.Is<CreatePurchaseCommand>(c => c.IdempotencyKey == idempotencyKey),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseId);

        // Act
        var result = await controller.CreatePurchase(cardId, request, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal($"/purchases/{purchaseId}", created.Location);
        Assert.Contains(idempotencyKey.ToString(), created.Value!.ToString());
        mediator.VerifyAll();
    }

    [Fact]
    public async Task CreatePurchase_MissingIdempotencyKey_GeneratesOne()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var controller = new CardLedgerController(mediator.Object);
        var cardId = Guid.NewGuid();
        var purchaseId = Guid.NewGuid();
        var request = new CreatePurchaseRequest
        {
            Description = "Dinner",
            TransactionDate = DateTimeOffset.UtcNow,
            Amount = 25m,
            CurrencyCode = "USD"
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        CreatePurchaseCommand? captured = null;
        mediator.Setup(m => m.Send(It.IsAny<CreatePurchaseCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Guid>, CancellationToken>((command, _) => captured = (CreatePurchaseCommand)command)
            .ReturnsAsync(purchaseId);

        // Act
        var result = await controller.CreatePurchase(cardId, request, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedResult>(result);
        Assert.Contains(purchaseId.ToString(), created.Value!.ToString());
        Assert.NotNull(captured);
        Assert.NotEqual(Guid.Empty, captured!.IdempotencyKey);
        mediator.VerifyAll();
    }
}
