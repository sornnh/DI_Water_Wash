using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class Frm_PasswordInput : Form
{
    public Frm_PasswordInput()
    {
        InitializeComponent();
        this.StartPosition = FormStartPosition.CenterParent;
    }
    public string EnteredPassword { get; private set; }


    private void textBox1_TextChanged(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, EventArgs e)
    {
        EnteredPassword = textBox1.Text;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }

    private void textBox1_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            EnteredPassword = textBox1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
