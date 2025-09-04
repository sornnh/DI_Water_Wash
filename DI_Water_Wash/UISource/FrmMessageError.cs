using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoyD_Oven_Monitoring
{
    public partial class FrmMessageError : Form
    {
        private string message = "Message";
        private Color backcolorinit = Color.White;
        private Color backcolor = Color.Green;
        private Color forecolor = Color.Black;
        private System.Windows.Forms.Timer _timer;
        private int _timeout;

        public FrmMessageError(string _message, Color _backcolor, Color _forecolor, int timeoutMs = 10000)
        {
            InitializeComponent();
            this.message = _message;
            this.backcolor = _backcolor;
            this.forecolor = _forecolor;
            timer1.Start();
            _timeout = timeoutMs;

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = _timeout;
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            this.Close();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            ChangeColorMessage(lb_Mess);
        }
        private void ChangeColorMessage(Label _lb)
        {
            if (lb_Mess.InvokeRequired)
            {
                lb_Mess.Invoke(new Action(() => ChangeColorMessage(_lb)));
            }
            else
            {
                if(_lb.BackColor == backcolorinit)
                {
                    _lb.BackColor = backcolor;
                }
                else
                {
                    _lb.BackColor = backcolorinit;
                }
            }
        }

        private void FrmMessage_Shown(object sender, EventArgs e)
        {
            lb_Mess.Text = message;
            lb_Mess.BackColor = backcolorinit;
            lb_Mess.ForeColor = forecolor;
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
