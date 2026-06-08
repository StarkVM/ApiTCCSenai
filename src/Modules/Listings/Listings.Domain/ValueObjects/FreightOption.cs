namespace Listings.Domain.ValueObjects;

/// <summary>
/// Represents the optional freight/delivery service offered by the provider.
/// / Representa o serviço opcional de frete/entrega oferecido pelo fornecedor.
/// </summary>
public sealed class FreightOption
{
    public bool IsAvailable { get; private set; }
    
    public decimal FixedPrice { get; private set; }

    private FreightOption()
    {
    }

    private FreightOption(
        bool isAvailable,
        decimal fixedPrice)
    {
        if (isAvailable && fixedPrice <= 0)
        {
            throw new ArgumentException("Freight price must be greater than zero when freight is available.");
        }

        if (!isAvailable && fixedPrice != 0)
        {
            throw new ArgumentException("Freight price must be zero when freight is not available.");
        }

        IsAvailable = isAvailable;
        FixedPrice = fixedPrice;
    }
    
    public static FreightOption NotAvailable()
    {
        return new FreightOption(false, 0);
    }
    
    public static FreightOption Available(decimal fixedPrice)
    {
        return new FreightOption(true, fixedPrice);
    }
}