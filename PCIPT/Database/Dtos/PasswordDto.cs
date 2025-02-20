using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Database.Dtos
{
    public sealed record PasswordDto(string Login, string Passhash, string Salt, string Role);
}
