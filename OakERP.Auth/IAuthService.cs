using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OakERP.Shared.DTOs.Auth;

namespace OakERP.Auth
{
    public interface IAuthService
    {
        Task<AuthResultDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthResultDTO> LoginAsync(LoginDTO dto);
    }
}
