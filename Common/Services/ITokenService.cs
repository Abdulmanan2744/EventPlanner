using Common.Models.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(AppUser user, IList<string> roles);
    }
}
