using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Application.Abstracts
{
    public interface ITokenService
    {
        string GenerateAccessToken(int userId, string email, string role);
        string GenerateRefreshToken();
    }
}
