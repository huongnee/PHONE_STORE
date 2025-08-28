using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(long userId, string? email, IEnumerable<string> roles);
    }

}
