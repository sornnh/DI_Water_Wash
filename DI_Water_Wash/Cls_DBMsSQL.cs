using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace DI_Water_Wash
{
    public class Cls_DBMsSQL
    {
        private SqlConnection connection;
        private string connectionString;
        public bool IsConnected { get; private set; }
        private bool bLocal = true;
        public void Initialize(string server, string database, string user, string password)
        {
            if (bLocal)
                return;
            connectionString = $"Server={server};Database={database};User Id={user};Password={password};";
            connection = new SqlConnection(connectionString);
        }
        public void Open()
        {
            if (bLocal)
                return;
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                if(connection.State == ConnectionState.Open)
                {
                    IsConnected = true;
                }
                else
                {
                    IsConnected = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection Error: " + ex.Message);
                IsConnected = false;
            }
        }
        public void Close()
        {
            if (bLocal)
                return;
            if (!IsConnected)
                connection.Close();
        }
        // Truy vấn dữ liệu (SELECT)
        public DataTable ExecuteQuery(string query)
        {
            
            DataTable dt = new DataTable();
            if (bLocal)
                return dt;
            try
            {
                Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Query Error: " + ex.Message);
            }
            finally
            {
                Close();
            }
            return dt;
        }
        // Thực thi lệnh INSERT, UPDATE, DELETE
        public bool ExecuteNonQuery(string commandText)
        {
            if (bLocal)
                return false;
            try
            {
                Open();
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    int affectedRows = cmd.ExecuteNonQuery();
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Execute Error: " + ex.Message);
                return false;
            }
            finally
            {
                Close();
            }
        }

        // Truy vấn giá trị đơn (SELECT COUNT(*)...)
        public object ExecuteScalar(string commandText)
        {
            if (bLocal)
                return null;
            try
            {
                Open();
                using (SqlCommand cmd = new SqlCommand(commandText, connection))
                {
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Scalar Error: " + ex.Message);
                return null;
            }
            finally
            {
                Close();
            }
        }
    }
}
