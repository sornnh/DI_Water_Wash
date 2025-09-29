using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

public partial class UC_SelectPartNumber : UserControl
{
    private int UnitIndex;
    private List<string> partNumbersinfor;
    private Cls_DBMsSQL ParameterDB = new Cls_DBMsSQL();
    string filePath = @"C:\Aavid_Test\Setup-ini\Flushing_Part_Numbers.txt";
    private string[] allLine;
    public UC_SelectPartNumber(int unitIndex)
    {
        InitializeComponent();
        UnitIndex = unitIndex;
        partNumbersinfor = GetPartNumberInfor();
            
    }
    private List<string> GetPartNumberInfor()
    {
        List<string> result = new List<string>();
        string query = $"SELECT ASSY_PN, Description FROM Parameters_SZ.dbo.Aavid_Part_Numbers";
        ParameterDB.Initialize("10.102.4.20", "Parameters_SZ", "sa", "nuventixleo");
        ParameterDB.Open();
        DataTable dtResult = ParameterDB.ExecuteQuery(query);
        foreach (DataRow row in dtResult.Rows)
        {
            string assyPn = row["ASSY_PN"].ToString();
            string description = row["Description"].ToString();
            string displayText = $"{assyPn} - {description}";

            result.Add(displayText);
        }
        ParameterDB.Close();
        return result;
    }
        
    private void UC_SelectPartNumber_Load(object sender, EventArgs e)
    {
        this.lb_Fixture.Text = "Fixture " + (UnitIndex + 1).ToString();
        cbb_PartNumberInfo.Items.AddRange(partNumbersinfor.ToArray());
        try
        {
            allLine = File.ReadAllLines(@"C:\Aavid_Test\Setup-ini\Flushing_Part_Numbers.txt");
            string[] infor = allLine[UnitIndex+1].Split(',');
            txt_WO.Text = infor[2];
            txt_PN.Text = infor[1];

            for (int i = 0; i < partNumbersinfor.Count; i++)
            {
                if (partNumbersinfor[i].Contains(infor[1]))
                {
                    cbb_PartNumberInfo.SelectedIndex = i;
                    txt_Description.Text = partNumbersinfor[i].Split('-')[1].Trim();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error when get line text from C:\\Aavid_Test\\Setup-ini\\Flushing_Part_Numbers.txt: " + ex.Message);
        }
    }

    private void cbb_PartNumberInfo_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selectedPartNumber = cbb_PartNumberInfo.SelectedItem.ToString();
        txt_PN.Text = partNumbersinfor[cbb_PartNumberInfo.SelectedIndex].Split('-')[0].Trim();
        txt_Description.Text = partNumbersinfor[cbb_PartNumberInfo.SelectedIndex].Split('-')[1].Trim();
    }

    private void btn_ChangePart_Click(object sender, EventArgs e)
    {
        allLine = File.ReadAllLines(@"C:\Aavid_Test\Setup-ini\Flushing_Part_Numbers.txt");
        string[] newLine = new string[] { $"{txt_PN.Text.Trim()}-TF-00{(UnitIndex+1).ToString()}", txt_PN.Text.Trim(), txt_WO.Text.Trim(), "0", "Enabled" };
        allLine[UnitIndex+1] = string.Join(",", newLine) + ",";
        // Ghi lại file
        File.WriteAllLines(filePath, allLine);
    }
}
