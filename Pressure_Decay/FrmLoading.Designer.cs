partial class FrmLoading
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.pictureBox1 = new System.Windows.Forms.PictureBox();
        this.progressBar1 = new System.Windows.Forms.ProgressBar();
        this.richTextBox1 = new System.Windows.Forms.RichTextBox();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
        this.SuspendLayout();
        // 
        // pictureBox1
        // 
        this.pictureBox1.Image = global::Pressure_Decay.Properties.Resources.Capture;
        this.pictureBox1.Location = new System.Drawing.Point(15, 12);
        this.pictureBox1.Name = "pictureBox1";
        this.pictureBox1.Size = new System.Drawing.Size(314, 106);
        this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.pictureBox1.TabIndex = 2;
        this.pictureBox1.TabStop = false;
        // 
        // progressBar1
        // 
        this.progressBar1.Location = new System.Drawing.Point(15, 145);
        this.progressBar1.Name = "progressBar1";
        this.progressBar1.Size = new System.Drawing.Size(311, 20);
        this.progressBar1.TabIndex = 3;
        // 
        // richTextBox1
        // 
        this.richTextBox1.Location = new System.Drawing.Point(15, 189);
        this.richTextBox1.Name = "richTextBox1";
        this.richTextBox1.Size = new System.Drawing.Size(311, 291);
        this.richTextBox1.TabIndex = 4;
        this.richTextBox1.Text = "";
        // 
        // FrmLoading
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.White;
        this.ClientSize = new System.Drawing.Size(341, 492);
        this.ControlBox = false;
        this.Controls.Add(this.richTextBox1);
        this.Controls.Add(this.progressBar1);
        this.Controls.Add(this.pictureBox1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "FrmLoading";
        this.ShowIcon = false;
        this.Text = "FrmLoading";
        ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.RichTextBox richTextBox1;
}
