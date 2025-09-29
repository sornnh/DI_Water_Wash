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

public partial class Frm_ChangePartNumber : Form
{
    private UC_SelectPartNumber ucSelectPartNumber;
    string filePath = @"C:\Aavid_Test\Setup-ini\Flushing_Part_Numbers.txt";
    private string[] allLine = { "Fixture_Id,Part_Number,Work_Order,T2_Off,Nest_Enable,", "111182-TF-001,708977,3121504011,0,Enabled,"};
    public Frm_ChangePartNumber()
    {
        InitializeComponent();
        ucSelectPartNumber = new UC_SelectPartNumber(0);
        if (!File.Exists(filePath))
            CreateFlushingFile();
    }
    private void CreateFlushingFile()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (string line in allLine)
                {
                    writer.WriteLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error when create file Flushing_Part_Numbers.txt: " + ex.Message);
        }
    }
    private void btn_Ok_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.OK;
    }

    private void Frm_ChangePartNumber_Shown(object sender, EventArgs e)
    {
        ucSelectPartNumber.Dock = DockStyle.Fill;
        ucSelectPartNumber.Visible = true;
        ucSelectPartNumber.BringToFront();
        panel1.Controls.Add(ucSelectPartNumber);
    }
}
