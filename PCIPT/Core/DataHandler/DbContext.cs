using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCIPT.Core.DataHandler
{
    public sealed class DbContext
    {
        public SqlConnection? SqlConnection { get; private set; } = null;

        public DbContext() { }
        public DbContext(string connectionString)
        {
            Connect(connectionString);
        }

        public void Connect(string connectionString)
        {
            if (SqlConnection != null)
            {
                Close();
            }

            SqlConnection = new SqlConnection(connectionString);
            SqlConnection.Open();
        }

        public void Close()
        {
            SqlConnection?.Close();
        }

        /*
        public DataSet SelectAllFrom(string tablename)
        {
            return Select("SELECT * FROM " + tablename);
        }

        public void InsertInto(string table, List<string> names, List<object> values)
        {
            for (int i = 0; i < values.Count; i++)
                if (values[i] is string)
                    values[i] = "'" + values[i] + "'";

            SqlCommand command = new()
            {
                CommandText = $"INSERT INTO {table}({String.Join(", ", names)}) VALUES ({String.Join(", ", values)})",
                Connection = SqlConnection
            };
            command.ExecuteNonQuery();
        }

        public void Delete(string table, string condition)
        {
            SqlCommand command = new()
            {
                CommandText = $"DELETE FROM {table} WHERE {condition}",
                Connection = SqlConnection
            };
            command.ExecuteNonQuery();
        }

        public DataSet Select(string command)
        {
            if (SqlConnection != null)
            {
                SqlCommand cmd = new(command, SqlConnection);
                DataSet data = new();
                SqlDataAdapter adapter = new(cmd);
                adapter.Fill(data);
                return data;
            }
            throw new Exception("DBHandler.SelectAllFrom: SqlConnection is null");
        }
        */
    }
}
