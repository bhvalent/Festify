using Festify.Promotion.Messages.Sales;
using Festify.Promotion.Sales;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Festify.Promotion.UnitTest;

public class PromotionServiceTests : IAsyncLifetime
{
    private InMemoryTestHarness _harness;
    private IPublishEndpoint _publishEndpoint;

    public PromotionServiceTests()
    {
        _harness = new InMemoryTestHarness();
    }

    public async Task InitializeAsync()
    {
        await _harness.Start();
        _publishEndpoint = _harness.Bus;
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task WhenCustomerPurchasesItem_ThenPurchaseIsPublished()
    {
        // Arrange
        var producer = new PromotionService(_publishEndpoint);

        // Act
        await producer.PurchaseTicket("123", 0.0m);

        await _harness.InactivityTask;

        // Assert
        _harness.Published.Select<OrderPlaced>()
            .Count().Should().Be(1);

        await _harness.Stop();
    }

    [Fact]
    public async Task WhenCustomerPurchasesItem_ThenCustomerIsCharged()
    {
        // Arrange
        var creditCardNumber = "123456";
        var total = 21.12m;

        var fakePaymentProcessor = new FakePaymentProcessor();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<PromotionService>();
        serviceCollection.AddSingleton<IPaymentProcessor>(fakePaymentProcessor);
        serviceCollection.AddMassTransitInMemoryTestHarness(cfg =>
        { 
            cfg.AddConsumer<CustomerChargeConsumer>(); 
        });

        await using var provider = serviceCollection.BuildServiceProvider(true);

        var harness = provider.GetRequiredService<InMemoryTestHarness>();
        await harness.Start();

        using var scope = provider.CreateScope();

        var producer = scope.ServiceProvider.GetRequiredService<PromotionService>();

        // Act
        await producer.PurchaseTicket(creditCardNumber, total);

        await harness.InactivityTask;

        // Assert
        fakePaymentProcessor.Payments.Count().Should().Be(1);
        fakePaymentProcessor.Payments.Should().Contain(
            new FakePaymentProcessor.Payment(creditCardNumber, total)
        );
        

        await harness.Stop();
    }
}