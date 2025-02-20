using Client.Runtime.Framework.Command;
using Microsoft.Data.SqlClient;
using PCIPT.Core.DataHandler;
using PCIPT.Database.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace PCIPT.Database.Commands
{
    public sealed class AddAccountEntryCommand : ICommand<PasswordDto, string>
    {
        SqlConnection SqlConnection { get; set; }

        public AddAccountEntryCommand(SqlConnection sqlConnection)
        {
            SqlConnection = sqlConnection;
        }

        public string Execute(PasswordDto data)
        {
            try
            {
                SqlCommand command = new()
                {
                    CommandText = $"INSERT INTO Passwords(login, passhash, salt, role) VALUES ('{data.Login}', '{data.Passhash}', '{data.Salt}', '{data.Role}')",
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
