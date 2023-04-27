using Festify.Promotion.Messages.Sales;
using Festify.Promotion.Sales;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using System.Linq;
using Xunit;

namespace Festify.Promotion.UnitTest
{
    public class PromotionServiceTests
    {
        [Fact]
        public async void WhenCustomerPurchasesItem_ThenPurchaseIsPublished()
        {
            var harness = new InMemoryTestHarness();
            await harness.Start();

            // arrange
            IPublishEndpoint publishEndpoint = harness.Bus;
            var producer = new PromotionService(publishEndpoint);

            // act
            await producer.PurchaseTicket();

            await harness.InactivityTask;

            // assert
            harness.Published.Select<OrderPlaced>().Count().Should().Be(1);

            await harness.Stop();
        }
    }
}
