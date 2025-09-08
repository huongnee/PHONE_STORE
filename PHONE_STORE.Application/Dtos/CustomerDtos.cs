using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos;

public record CustomerDto(long Id, long? UserAccountId, string? Email, string? Phone, string? FullName,
                          DateTime CreatedAt, DateTime? UpdatedAt);
public record CustomerUpsertDto(long? UserAccountId, string? Email, string? Phone, string? FullName);