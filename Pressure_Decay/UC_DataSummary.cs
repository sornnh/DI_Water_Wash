using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

public partial class UC_DataSummary : UserControl
{
    private Cls_DBMsSQL ProductionDB = new Cls_DBMsSQL();
    string[] Process = new string[6] { "Pressure Decay", "Thermal Resistence", "DI Water Wash", "Drying", "Helium Leakage", "Final Valve" };
    string[] AssyPN;
    private bool bSearching = false;
    string CurentPartNumber = "";
    string CurentStation = "";
    public UC_DataSummary(string PN, string Station)
    {
        InitializeComponent();
        ProductionDB.Initialize("10.102.4.20", "Production_SZ", "sa", "nuventixleo");
        ProductionDB.Open();
        string query = "SELECT \r\nASSY_PN\r\nFROM [Parameters_SZ].[dbo].[Aavid_Part_Numbers]";
        DataTable dt = ProductionDB.ExecuteQuery(query);
        AssyPN = new string[dt.Rows.Count];
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            AssyPN[i] = dt.Rows[i][0].ToString();
        }
        cbb_PN.Items.AddRange(AssyPN);
        CurentPartNumber = PN;
        CurentStation = Station;
        cbb_PN.SelectedItem = CurentPartNumber;
    }

    private void UC_DataSummary_Load(object sender, EventArgs e)
    {
        cbb_Station.Items.AddRange(Process);
        cbb_PN.Items.AddRange(AssyPN);
        cbb_Station.SelectedItem = CurentStation;
    }
    private void btn_GetData_Click(object sender, EventArgs e)
    {
        DateTime startDate = dtPicker_Start.Value.Date;
        DateTime endDate = dtPicker_End.Value.Date.AddDays(1).AddTicks(-1); // lấy hết ngày đó đến 23:59:59
        if (bSearching)
        {
            return;
        }
        if (startDate > endDate)
        {
            MessageBox.Show("End date must be greater than start date");
            return;
        }
        if ((dtPicker_End.Value - dtPicker_Start.Value).TotalDays > 5)
        {
            MessageBox.Show("The date range must not exceed 5 days");
            return;
        }
        if (cbb_Station.Text == "")
        {
            MessageBox.Show("Please select a station");
            return;
        }
        if (cbb_PN.Text == "")
        {
            MessageBox.Show("Please select a part number");
            return;
        }
        bSearching = true;
        string startDateStr = startDate.ToString("yyyy-MM-dd HH:mm:ss");
        string endDateStr = endDate.ToString("yyyy-MM-dd HH:mm:ss");
        string PN = cbb_PN.Text;
        switch (cbb_Station.Text)
        {
            case "Pressure Decay":
                {
                    string query = "";
                    if (cBox_OnlyPass.Checked)
                    {
                        query = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[LCS_Pressure_and_Sniffing_Test_Log]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}' AND FailCode ='OK' AND (FailCode = 'Pass'OR FailCode ='PASS' OR FailCode ='Ok')
                                ORDER BY [Date_Time] DESC";
                    }
                    else
                    {
                        query = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[LCS_Pressure_and_Sniffing_Test_Log]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}'
                                ORDER BY [Date_Time] DESC";
                    }
                    DataTable dt = ProductionDB.ExecuteQuery(query);
                    dataGridView1.DataSource = dt;
                    break;
                }
            case "Drying":
                {
                    string query = "";
                    if (cBox_OnlyPass.Checked)
                    {
                        query = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[LCS_Drying_Humidity_Log]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}' AND FailCode ='OK' AND (FailCode = 'Pass'OR FailCode ='PASS' OR FailCode ='Ok')
                                ORDER BY [Date_Time] DESC";
                    }
                    else
                    {
                        query = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[LCS_Drying_Humidity_Log]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}'
                                ORDER BY [Date_Time] DESC";
                    }
                    DataTable dt = ProductionDB.ExecuteQuery(query);
                    dataGridView1.DataSource = dt;
                    break;
                }
            case "Thermal Resistence":
                {
                    string query1 = "";
                    string query2 = "";
                    if (cBox_OnlyPass.Checked)
                    {
                        query1 = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[LCS_Thermal_Test_Data_Extended]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}' AND FailCode ='OK' AND (FailCode = 'Pass'OR FailCode ='PASS' OR FailCode ='Ok')
                                ORDER BY [Date_Time] DESC";
                    }
                    else
                    {
                        query1 = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[LCS_Thermal_Test_Data_Extended]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}'
                                ORDER BY [Date_Time] DESC";
                    }
                    if (cBox_OnlyPass.Checked)
                    {
                        query2 = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[LCS_Thermal_Test_Data]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}' AND FailCode ='OK' AND (FailCode = 'Pass'OR FailCode ='PASS' OR FailCode ='Ok')
                                ORDER BY [Date_Time] DESC";
                    }
                    else
                    {
                        query2 = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[LCS_Thermal_Test_Data]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}'
                                ORDER BY [Date_Time] DESC";
                    }
                    DataTable dt1 = ProductionDB.ExecuteQuery(query1);
                    DataTable dt2 = ProductionDB.ExecuteQuery(query2);
                    dataGridView1.DataSource = MergerTable(dt1, dt2);
                    break;
                }
            case "DI Water Wash":
                {
                    string query = "";
                    if (cBox_OnlyPass.Checked)
                    {
                        query = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[DI_Water_Wash_Log]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}' AND FailCode ='OK' AND (FailCode = 'Pass'OR FailCode ='PASS' OR FailCode ='Ok')
                                ORDER BY [Date_Time] DESC";
                    }
                    else
                    {
                        query = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[DI_Water_Wash_Log]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}'
                                ORDER BY [Date_Time] DESC";
                    }
                    DataTable dt = ProductionDB.ExecuteQuery(query);
                    dataGridView1.DataSource = dt;
                    break;
                }
            case "Helium Leakage":
                {
                    string query = "";
                    if (cBox_OnlyPass.Checked)
                    {
                        query = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[Leak_Test_Data]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}' AND FailCode ='OK' AND (FailCode = 'Pass'OR FailCode ='PASS' OR FailCode ='Ok')
                                ORDER BY [Date_Time] DESC";
                    }
                    else
                    {
                        query = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[Leak_Test_Data]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}'
                                ORDER BY [Date_Time] DESC";
                    }
                    DataTable dt = ProductionDB.ExecuteQuery(query);
                    dataGridView1.DataSource = dt;
                    break;
                }
            case "Final Valve":
                {
                    string query = "";
                    if (cBox_OnlyPass.Checked)
                    {
                        query = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[Aavid_LCS_Final_Test_Station]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}' AND FailCode ='OK' AND (FailCode = 'Pass'OR FailCode ='PASS' OR FailCode ='Ok')
                                ORDER BY [Date_Time] DESC";
                    }
                    else
                    {
                        query = $@"
                                SELECT 
                                *FROM [Production_SZ].[dbo].[Aavid_LCS_Final_Test_Station]
                                WHERE [Date_Time] BETWEEN '{startDateStr}' AND '{endDateStr}'
                                AND [Assy_PN] = '{PN}'
                                ORDER BY [Date_Time] DESC";
                    }
                    DataTable dt = ProductionDB.ExecuteQuery(query);
                    dataGridView1.DataSource = dt;
                    break;
                }
        }
        foreach (DataGridViewColumn column in dataGridView1.Columns)
        {
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
        dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
        dataGridView1.RowHeadersVisible = false;
        dataGridView1.DefaultCellStyle.SelectionBackColor = dataGridView1.DefaultCellStyle.BackColor;
        dataGridView1.DefaultCellStyle.SelectionForeColor = dataGridView1.DefaultCellStyle.ForeColor;
        bSearching = false;
    }
    private DataTable MergerTable(DataTable dt1, DataTable dt2)
    {

        // Tạo bảng gộp kết quả
        DataTable dtMerged = new DataTable();

        // Thêm các cột từ dt1
        foreach (DataColumn col in dt1.Columns)
        {
            string colName = "T1_" + col.ColumnName;
            dtMerged.Columns.Add(colName, col.DataType);
        }
        // Thêm các cột từ dt2
        foreach (DataColumn col in dt2.Columns)
        {
            string colName = "T2_" + col.ColumnName;
            if (!dtMerged.Columns.Contains(colName)) // tránh trùng tên
                dtMerged.Columns.Add(colName, col.DataType);
        }
        // Duyệt từng dòng trong dt1
        foreach (DataRow row1 in dt1.Rows)
        {
            var key1 = row1["Record_Key_ID"].ToString();

            // Tìm dòng trong dt2 có cùng Record_Key_ID
            DataRow[] matchedRows = dt2.Select($"Record_Key_ID = '{key1}'");

            if (matchedRows.Length > 0)
            {
                DataRow row2 = matchedRows[0]; // dùng dòng đầu tiên nếu có nhiều

                // Tạo dòng mới cho dtMerged
                DataRow newRow = dtMerged.NewRow();

                // Copy dữ liệu từ row1
                foreach (DataColumn col in dt1.Columns)
                {
                    newRow["T1_" + col.ColumnName] = row1[col];
                }

                // Copy dữ liệu từ row2
                foreach (DataColumn col in dt2.Columns)
                {
                    string colName = "T2_" + col.ColumnName;
                    if (dtMerged.Columns.Contains(colName))
                        newRow[colName] = row2[col];
                }

                dtMerged.Rows.Add(newRow);
            }
        }
        return dtMerged;
    }
    private void btn_ExportExcel_Click(object sender, EventArgs e)
    {
        ExportToExcel_Interop(dataGridView1 as DataGridView);
    }
    private void ExportToExcel_Interop(DataGridView dgv)
    {
        try
        {
            var excelApp = new Excel.Application();
            excelApp.Visible = false;

            Excel.Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
            Excel.Worksheet worksheet = workbook.ActiveSheet;
            worksheet.Name = "Dữ liệu";

            // Ghi tiêu đề
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1] = dgv.Columns[i].HeaderText;
            }

            // Ghi dữ liệu
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                for (int j = 0; j < dgv.Columns.Count; j++)
                {
                    worksheet.Cells[i + 2, j + 1] = dgv.Rows[i].Cells[j].Value?.ToString();
                }
            }
            DateTime dateTime = DateTime.Now;
            // Mở hộp thoại lưu
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel Workbook|*.xlsx";
            saveDialog.Title = "Lưu file Excel";
            saveDialog.FileName = $"DuLieuTest_{cbb_PN.Text.ToString()}_{dateTime.ToString("yyyyMMdd_HHmm")}.xlsx";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                workbook.SaveAs(saveDialog.FileName);
                MessageBox.Show("Xuất Excel thành công!");
            }

            workbook.Close(false);
            excelApp.Quit();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message);
        }
    }    
}
