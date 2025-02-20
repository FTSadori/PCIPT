using Client.Runtime.Framework.Command;
using Microsoft.Data.SqlClient;
using PCIPT.Core.DataHandler;
using PCIPT.Database.Dtos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace PCIPT.Database.Commands
{
    public sealed class UpdatePasswordCommand : ICommand<UpdatePasswordDto, string>
    {
        SqlConnection SqlConnection { get; set; }

        public UpdatePasswordCommand(SqlConnection sqlConnection)
        {
            SqlConnection = sqlConnection;
        }

        public string Execute(UpdatePasswordDto data)
        {
            try
            {
                SqlCommand command = new()
                {
                    CommandText = $"UPDATE Customers SET passhash = '{data.NewPasshash}', salt = '{data.NewSalt}' WHERE login = {data.Login};",
                    Connection = SqlConnection
                };
                command.ExecuteNonQuery();

                return new DeleteTokenByLoginCommand(SqlConnection).Execute(data.Login);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
