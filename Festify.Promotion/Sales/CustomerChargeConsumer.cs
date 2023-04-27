using Festify.Promotion.Messages.Sales;
using MassTransit;
using System.Threading.Tasks;

namespace Festify.Promotion.Sales
{
    public class CustomerChargeConsumer : IConsumer<OrderPlaced>
    {
        private readonly IPaymentProcessor _paymentProcessor;

        public CustomerChargeConsumer(IPaymentProcessor paymentProcessor)
        {
            _paymentProcessor = paymentProcessor;
        }

        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            await _paymentProcessor.ProcessCreditCardPayment(context.Message.CreditCardNumber, context.Message.Total);
        }
    }
}
