namespace Festify.Promotion.Messages.Sales;
public class OrderPlaced
{
    public string CreditCardNumber { get; init; } = "";
    public decimal Total { get; init; } = 0.0m;
}
