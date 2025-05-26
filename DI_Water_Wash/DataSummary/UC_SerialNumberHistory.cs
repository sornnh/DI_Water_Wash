using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DI_Water_Wash.DataSummary
{
    public partial class UC_SerialNumberHistory : UserControl
    {
        private Cls_DBMsSQL ParameterDB = new Cls_DBMsSQL();
        private Cls_DBMsSQL ProductionDB = new Cls_DBMsSQL();
        string[] Process = new string[6] { "Pressure Decay", "Thermal Resistence", "DI Water Wash", "Drying", "Helium Leakage", "Final Valve" };
        string[] AssyPN;
        private bool bSearching = false;
        public UC_SerialNumberHistory(string PN)
        {
            InitializeComponent();
            ParameterDB.Initialize("10.102.4.20", "Parameters_SZ", "sa", "nuventixleo");
            ParameterDB.Open();
            ProductionDB.Initialize("10.102.4.20", "Production_SZ", "sa", "nuventixleo");
            ProductionDB.Open();
            string query = "SELECT \r\nASSY_PN\r\nFROM [Parameters_SZ].[dbo].[Aavid_Part_Numbers]";
            DataTable dt = ParameterDB.ExecuteQuery(query);
            AssyPN = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                AssyPN[i] = dt.Rows[i][0].ToString();
            }
            cbb_PN.Items.AddRange(AssyPN);
            cbb_PN.SelectedItem = PN;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetProcessList(cbb_PN.Text);
            GetPackingInformation(txt_SN.Text);
        }
        private void GetPackingInformation(string PN)
        {
            string query = $"SELECT BOXID,SERIAL,DATE_TIME,ASSY_PN FROM [Production_SZ].[dbo].[LCS_BoxID_SN_XRef] WHERE  Serial = '{txt_SN.Text}' order by DATE_TIME";
            //MessageBox.Show(query);
            DataTable dt = ProductionDB.ExecuteQuery(query);
            string BoxID = "";
            if (dt.Rows.Count == 0)
                return;
            DataRow row = dt.Rows[dt.Rows.Count-1];
            foreach (DataColumn col in dt.Columns)
            {
                string key = col.ColumnName;
                if (key == "BOXID")
                {
                    BoxID = row[key].ToString();
                }
            }
            txt_Packinginfor.Text = BoxID; 
        }

        private void GetProcessList(string PN)
        {
            string query = $"SELECT * FROM Process_Flow_for_Part_Numbers WHERE Assy_PN = '{PN}' order by Date_Time";
            //MessageBox.Show(query);
            DataTable dt = ParameterDB.ExecuteQuery(query);
            string[]Proceslist = LoadVerifyValues(dt);
            DataTable dtAll = null;
            for (int i = 0; i < Proceslist.Length; i++)
            {
                string TableName = "";
                string SummaryTableName = "";
                StateCommon.GetTableName(Proceslist[i],out TableName,out SummaryTableName);
                query = $"SELECT [Serial],[Assy_PN],[Date_Time],[Station],[FailCode] " +
                    $"FROM [Production_SZ].[dbo].[{TableName}] where [Serial] ='{txt_SN.Text}' order by Date_Time";
                dt = ParameterDB.ExecuteQuery(query);
                if (dt != null && dt.Rows.Count > 0)
                {
                    // Thêm cột "Process" nếu chưa có
                    if (!dt.Columns.Contains("Process"))
                        dt.Columns.Add("Process", typeof(string));

                    // Gán giá trị tên process cho từng dòng
                    foreach (DataRow row in dt.Rows)
                    {
                        row["Process"] = Proceslist[i];
                    }

                    // Nếu dtAll chưa được khởi tạo, clone từ dtStep
                    if (dtAll == null)
                        dtAll = dt.Clone();

                    // Merge vào dtAll
                    dtAll.Merge(dt);
                }
            }
            dgv_History.DataSource = null;        // Xóa nguồn cũ nếu có
            dgv_History.DataSource = dtAll;       // Gán nguồn mới
            dgv_History.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells; // Tự động co cột
        }

        public string[] LoadVerifyValues(DataTable dt)
        {
            string[] verifyFlags = null;
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No data found for the given Assy_PN.");
                verifyFlags = new string[1] { "" };
                return verifyFlags;
            }
            DataRow row = dt.Rows[0];
            foreach (DataColumn col in dt.Columns)
            {
                string key = col.ColumnName;
                if (key == "Process_List")
                {
                    string[] processList = row[key].ToString().Split(',');
                    verifyFlags = new string[processList.Length];
                    for (int i = 0; i < processList.Length; i++)
                    {
                        verifyFlags[i] = processList[i].Trim();
                    }
                }
            }
            return verifyFlags;
        }
        
    }
}
