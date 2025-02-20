using Client.Runtime.Framework.Command;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Database.Commands
{
    public sealed class SelectLoginByTokenCommand : ICommand<string, DataRow?>
    {
        SqlConnection SqlConnection { get; set; }

        public SelectLoginByTokenCommand(SqlConnection sqlConnection)
        {
            SqlConnection = sqlConnection;
        }

        public DataRow? Execute(string token)
        {
            try
            {
                SqlCommand cmd = new($"SELECT * FROM RememberMeTokens WHERE token='{token}'", SqlConnection);
                DataSet data = new();
                SqlDataAdapter adapter = new(cmd);
                adapter.Fill(data);
                return data.Tables[0].Rows[0];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
