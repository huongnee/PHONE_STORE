using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos
{
    public record AttributeValueUpsertDto(long AttributeId, long? IntValue, decimal? DecValue, bool? BoolValue, string? TextValue);
}
