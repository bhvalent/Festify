using Festify.Promotion.Messages.Sales;
using MassTransit;
using System.Threading.Tasks;

namespace Festify.Promotion.Sales;

public class PromotionService
{
    private IPublishEndpoint _publishEndpoint;
    private IPaymentProcessor _paymentProcessor;

    public PromotionService(IPublishEndpoint publishEndpoint, IPaymentProcessor paymentProcessor)
    {
        _publishEndpoint = publishEndpoint;
        _paymentProcessor = paymentProcessor;
    }

    public async Task PurchaseTicket()
    {
        await _paymentProcessor.ProcessCreditCardPayment("123456", 21.12m);
        await _publishEndpoint.Publish(new OrderPlaced());
    }
}