using Client.Runtime.Framework.Command;
using Microsoft.Data.SqlClient;
using PCIPT.Database.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace PCIPT.Database.Commands
{
    public sealed class DeleteAccountEntryCommand : ICommand<string, string>
    {
        SqlConnection SqlConnection { get; set; }

        public DeleteAccountEntryCommand(SqlConnection sqlConnection)
        {
            SqlConnection = sqlConnection;
        }

        public string Execute(string login)
        {
            try
            {
                SqlCommand command = new()
                {
                    CommandText = $"DELETE FROM Passwords WHERE login='{login}'",
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
