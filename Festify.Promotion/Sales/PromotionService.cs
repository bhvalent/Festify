using Festify.Promotion.Messages.Sales;
using MassTransit;
using System.Threading.Tasks;

namespace Festify.Promotion.Sales;

public class PromotionService
{
    private IPublishEndpoint _publishEndpoint;

    public PromotionService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PurchaseTicket(string creditCardNumber, decimal total)
    {
        await _publishEndpoint.Publish(new OrderPlaced
        {
            CreditCardNumber = creditCardNumber,
            Total = total
        });
    }
}