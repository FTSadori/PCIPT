using Client.Runtime.Framework.Command;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Database.Commands
{
    public sealed class DeleteTokenByLoginCommand : ICommand<string, string>
    {
        SqlConnection SqlConnection { get; set; }

        public DeleteTokenByLoginCommand(SqlConnection sqlConnection)
        {
            SqlConnection = sqlConnection;
        }

        public string Execute(string login)
        {
            try
            {
                SqlCommand command = new()
                {
                    CommandText = $"DELETE FROM RememberMeTokens WHERE login='{login}'",
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
