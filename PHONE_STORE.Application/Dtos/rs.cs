using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos
{
    public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

}
