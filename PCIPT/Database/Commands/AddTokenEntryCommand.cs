using Client.Runtime.Framework.Command;
using Microsoft.Data.SqlClient;
using PCIPT.Database.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Database.Commands
{
    public sealed class AddTokenEntryCommand : ICommand<TokenDto, string>
    {
        SqlConnection SqlConnection { get; set; }

        public AddTokenEntryCommand(SqlConnection sqlConnection)
        {
            SqlConnection = sqlConnection;
        }

        public string Execute(TokenDto data)
        {
            try
            {
                string msg = new DeleteTokenByLoginCommand(SqlConnection).Execute(data.Login);
                if (msg != "") return msg;

                SqlCommand command = new()
                {
                    CommandText = $"INSERT INTO RememberMeTokens(token, login) VALUES ('{data.Token}', '{data.Login}')",
                    Connection = SqlConnection
                };
                command.ExecuteNonQuery();

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
