using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos
{
    public record AttributeDto(long Id, string Code, string Name, string DataType);
    public record AttributeCreateDto(string Code, string Name, string DataType);
    public record AttributeUpdateDto(string Name, string DataType);

}
