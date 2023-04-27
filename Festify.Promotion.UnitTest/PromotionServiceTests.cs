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
    private IPaymentProcessor _paymentProcessor;

    public PromotionServiceTests()
    {
        _harness = new InMemoryTestHarness();
    }

    public async Task InitializeAsync()
    {
        await _harness.Start();
        _publishEndpoint = _harness.Bus;
        _paymentProcessor = new FakePaymentProcessor();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
    }

    [Fact]
    public async Task WhenCustomerPurchasesItem_ThenPurchaseIsPublished()
    {
        // Arrange
        var producer = new PromotionService(_publishEndpoint, _paymentProcessor);

        // Act
        await producer.PurchaseTicket();

        await _harness.InactivityTask;

        // Assert
        _harness.Published.Select<OrderPlaced>()
            .Count().Should().Be(1);

        await _harness.Stop();
    }

    [Fact]
    public async Task WhenCustomerPurchasesItem_ThenCustomerIsCharged()
    {
        var fakePaymentProcessor = new FakePaymentProcessor();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<PromotionService>();
        serviceCollection.AddSingleton<IPaymentProcessor>(fakePaymentProcessor);
        serviceCollection.AddMassTransitInMemoryTestHarness(cfg =>
        {
        });
        await using var provider = serviceCollection.BuildServiceProvider(true);

        var harness = provider.GetRequiredService<InMemoryTestHarness>();
        await harness.Start();

        using (var scope = provider.CreateScope())
        {
            var producer = scope.ServiceProvider.GetRequiredService<PromotionService>();
            await producer.PurchaseTicket();

            await harness.InactivityTask;

            fakePaymentProcessor.Payments.Count().Should().Be(1);
            fakePaymentProcessor.Payments.Should().Contain(
                new FakePaymentProcessor.Payment("123456", 21.12m)
            );
        }

        await harness.Stop();
    }
}