using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DI_Water_Wash.ParameterInitial
{
    public partial class Frm_ShowInputStation : Form
    {
        public Frm_ShowInputStation()
        {
            InitializeComponent();
        }
        public string EnteredStation { get; private set; }
        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.Trim().Length > 6)
            {
                MessageBox.Show("Station leng must be 6 key, Re-input again....");
                return;
            } 
            EnteredStation = textBox1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (textBox1.Text.Trim().Length > 6)
                {
                    MessageBox.Show("Station leng must be 6 key, Re-input again....");
                    return;
                }
                EnteredStation = textBox1.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
