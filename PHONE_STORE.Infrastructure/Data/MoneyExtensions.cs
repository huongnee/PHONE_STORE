using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PHONE_STORE.Infrastructure.Data;

public static class MoneyExtensions
{
    public static PropertyBuilder<decimal> HasMoney(this PropertyBuilder<decimal> prop)
        => prop.HasPrecision(18, 2);

    public static PropertyBuilder<decimal?> HasMoney(this PropertyBuilder<decimal?> prop)
        => prop.HasPrecision(18, 2);
}
