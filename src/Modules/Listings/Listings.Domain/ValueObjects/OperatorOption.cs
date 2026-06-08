namespace Listings.Domain.ValueObjects;

/// <summary>
/// Represents the optional operator service offered with the machine.
/// / Representa o serviço opcional de operador oferecido junto com a máquina.
/// </summary>
public sealed class OperatorOption
{
    public bool IsAvailable { get; private set; }
    
    public decimal AdditionalDailyPrice { get; private set; }

    private OperatorOption()
    {
    }

    private OperatorOption(
        bool isAvailable,
        decimal additionalDailyPrice)
    {
        if (isAvailable && additionalDailyPrice <= 0)
        {
            throw new ArgumentException("Operator daily price must be greater than zero when operator is available.");
        }

        if (!isAvailable && additionalDailyPrice != 0)
        {
            throw new ArgumentException("Operator daily price must be zero when operator is not available.");
        }

        IsAvailable = isAvailable;
        AdditionalDailyPrice = additionalDailyPrice;
    }
    
    public static OperatorOption NotAvailable()
    {
        return new OperatorOption(false, 0);
    }
    
    public static OperatorOption Available(decimal additionalDailyPrice)
    {
        return new OperatorOption(true, additionalDailyPrice);
    }
}