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
    public sealed class SelectAllNodesCoordsCommand : ICommand<DataSet?>
    {
        SqlConnection SqlConnection { get; set; }

        public SelectAllNodesCoordsCommand(SqlConnection sqlConnection)
        {
            SqlConnection = sqlConnection;
        }

        public DataSet? Execute()
        {
            try
            {
                SqlCommand cmd = new("SELECT * FROM NodesCoords", SqlConnection);
                DataSet data = new();
                SqlDataAdapter adapter = new(cmd);
                adapter.Fill(data);
                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
