using System.Threading.Tasks;

namespace Festify.Promotion;

public interface IPaymentProcessor
{
    Task ProcessCreditCardPayment(string creditCardNumber, decimal total);
}