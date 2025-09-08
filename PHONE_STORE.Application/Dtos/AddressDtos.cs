using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos;

public record AddressDto(long Id, long CustomerId, string? Label, string Recipient, string Phone, string Line1,
                         string? Ward, string? District, string? Province, string? PostalCode,
                         string AddressType, bool IsDefault,
                         DateTime CreatedAt, DateTime? UpdatedAt);

public record AddressUpsertDto(string? Label, string Recipient, string Phone, string Line1,
                               string? Ward, string? District, string? Province, string? PostalCode,
                               string AddressType, bool IsDefault);