using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using log4net;

namespace DI_Water_Wash
{
    public class Cls_DBMsSQL
    {
        private SqlConnection connection;
        private string connectionString;
        public bool IsConnected { get; private set; }
        private bool bLocal = false;
        private static readonly ILog log = LogManager.GetLogger(typeof(Cls_ASPcontrol));
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
                log.Error("Connection Error: " + ex.Message);
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
        public string GetPasswordByUser(string username)
        {
            string password = null;
            string query = "SELECT [Password] FROM [ApprovalDB].[dbo].[Account] WHERE [User] = @User";

            try
            {

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@User", username);
                    Open();
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        password = result.ToString();
                    }
                }
            }
            catch { }
            return password;
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
                log.Error("Query Error: " + ex.Message);
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
                log.Error("Execute Error: " + ex.Message);
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
                log.Error("Scalar Error: " + ex.Message);
                return null;
            }
            finally
            {
                Close();
            }
        }
        public bool SaveToDIWaterWashLog(string serial,string assyPN,string station,string failCode,string operatorName,string remark,string workOrder,int stepNo, float Flowrate,float Waterpressure, float Airpressure)
        {
            bool result = false;
            string query = @"
                            INSERT INTO DI_Water_Wash_Log
                            (Serial, Assy_PN, Date_Time, Station, FailCode, Operator, Remark, Work_Order, Step_No, Water_Flow_Rate, Water_Pressure, Air_Pressure)
                            VALUES
                            (@Serial, @Assy_PN, @Date_Time, @Station, @FailCode, @Operator, @Remark, @Work_Order, @Step_No, @Water_Flow_Rate, @Water_Pressure, @Air_Pressure);";
            try
            {
                Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Serial", serial);
                    command.Parameters.AddWithValue("@Assy_PN", assyPN);
                    command.Parameters.AddWithValue("@Date_Time", DateTime.Now);
                    command.Parameters.AddWithValue("@Station", station);
                    command.Parameters.AddWithValue("@FailCode", failCode ?? "");
                    command.Parameters.AddWithValue("@Operator", operatorName ?? "");
                    command.Parameters.AddWithValue("@Remark", remark ?? "");
                    command.Parameters.AddWithValue("@Work_Order", workOrder ?? "");
                    command.Parameters.AddWithValue("@Step_No", stepNo);
                    command.Parameters.AddWithValue("@Water_Flow_Rate", Flowrate);
                    command.Parameters.AddWithValue("@Water_Pressure", Waterpressure);
                    command.Parameters.AddWithValue("@Air_Pressure", Airpressure);
                    command.ExecuteNonQuery();
                }
                result =  true;
            }
            catch (Exception ex)
            {
                log.Error("Save DI_Water_Wash_Log Error: " + ex.Message);
                result = false;
            }
            finally
            {
                Close();
            }
            return result;
        }
        // Thêm vào đây:
        public bool SaveToLogMonths_LogTable(string SN, string PN, DateTime dateTime, string Station, string FailCode, string proc, string stepNo)
        {
            bool result = false;
            string query = @"
        INSERT INTO [Production_SZ].[dbo].[Production_Flow_All_Months_Log]
        (Serial, Assy_PN, Date_Time, Station, FailCode, Process_Step, Step_No)
        VALUES 
        (@Serial, @Assy_PN, @Date_Time, @Station, @FailCode, @Process_Step, @Step_No);";

            try
            {
                Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Serial", SN ?? "");
                    command.Parameters.AddWithValue("@Assy_PN", PN ?? "");
                    command.Parameters.AddWithValue("@Date_Time", dateTime);
                    command.Parameters.AddWithValue("@Station", Station ?? "");
                    command.Parameters.AddWithValue("@FailCode", FailCode ?? "");
                    command.Parameters.AddWithValue("@Process_Step", proc ?? "");

                    // Chuyển đổi stepNo sang int trước khi thêm
                    if (int.TryParse(stepNo, out int stepNumber))
                        command.Parameters.AddWithValue("@Step_No", stepNumber);
                    else
                        command.Parameters.AddWithValue("@Step_No", 0); // hoặc throw lỗi tùy logic của bạn

                    command.ExecuteNonQuery();
                }
                result = true;
            }
            catch (Exception ex)
            {
                log.Error("Log Save Error: " + ex.Message);
                result = false;
            }
            finally
            {
                Close();
            }
            return result;
        }
        public bool SaveToCurrentMonthLog(string serial, string assyPN, string station, string failCode, string processStep, int stepNo)
        {
            const string query = @"
        INSERT INTO Production_Flow_Current_Months_Log
        (Serial, Assy_PN, Date_Time, Station, FailCode, Process_Step, Step_No)
        VALUES
        (@Serial, @Assy_PN, @Date_Time, @Station, @FailCode, @Process_Step, @Step_No);";

            try
            {
                Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Serial", serial ?? "");
                    command.Parameters.AddWithValue("@Assy_PN", assyPN ?? "");
                    command.Parameters.AddWithValue("@Date_Time", DateTime.Now);
                    command.Parameters.AddWithValue("@Station", station ?? "");
                    command.Parameters.AddWithValue("@FailCode", failCode ?? "");
                    command.Parameters.AddWithValue("@Process_Step", processStep ?? "");
                    command.Parameters.AddWithValue("@Step_No", stepNo);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                log.Error("SaveToCurrentMonthLog Error: " + ex.Message);
                return false;
            }
            finally
            {
                Close();
            }
        }
    }
}
