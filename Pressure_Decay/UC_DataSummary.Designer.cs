
    partial class UC_DataSummary
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.cbb_Station = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbb_PN = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.dtPicker_End = new System.Windows.Forms.DateTimePicker();
            this.dtPicker_Start = new System.Windows.Forms.DateTimePicker();
            this.btn_ExportExcel = new System.Windows.Forms.Button();
            this.btn_GetData = new System.Windows.Forms.Button();
            this.cBox_OnlyPass = new System.Windows.Forms.CheckBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5.569948F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 94.43005F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1249, 773);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel2.ColumnCount = 11;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.107162F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 13.65105F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3.231018F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.93538F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.512116F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.67044F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.907916F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15.42811F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.027463F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.384231F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.697539F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.cbb_Station, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.cbb_PN, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.label3, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.label5, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this.dtPicker_End, 7, 0);
            this.tableLayoutPanel2.Controls.Add(this.dtPicker_Start, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.btn_ExportExcel, 10, 0);
            this.tableLayoutPanel2.Controls.Add(this.btn_GetData, 9, 0);
            this.tableLayoutPanel2.Controls.Add(this.cBox_OnlyPass, 8, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 5);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1239, 34);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // cbb_Station
            // 
            this.cbb_Station.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbb_Station.FormattingEnabled = true;
            this.cbb_Station.Location = new System.Drawing.Point(93, 5);
            this.cbb_Station.Margin = new System.Windows.Forms.Padding(4);
            this.cbb_Station.Name = "cbb_Station";
            this.cbb_Station.Size = new System.Drawing.Size(160, 25);
            this.cbb_Station.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(5, 1);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Station";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(262, 1);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 32);
            this.label2.TabIndex = 1;
            this.label2.Text = "PN";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cbb_PN
            // 
            this.cbb_PN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbb_PN.FormattingEnabled = true;
            this.cbb_PN.Location = new System.Drawing.Point(302, 5);
            this.cbb_PN.Margin = new System.Windows.Forms.Padding(4);
            this.cbb_PN.Name = "cbb_PN";
            this.cbb_PN.Size = new System.Drawing.Size(114, 25);
            this.cbb_PN.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(425, 1);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 32);
            this.label3.TabIndex = 1;
            this.label3.Text = "Date : From";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(712, 1);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(27, 32);
            this.label5.TabIndex = 1;
            this.label5.Text = "to";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dtPicker_End
            // 
            this.dtPicker_End.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtPicker_End.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtPicker_End.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtPicker_End.Location = new System.Drawing.Point(747, 4);
            this.dtPicker_End.Name = "dtPicker_End";
            this.dtPicker_End.Size = new System.Drawing.Size(184, 23);
            this.dtPicker_End.TabIndex = 4;
            // 
            // dtPicker_Start
            // 
            this.dtPicker_Start.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtPicker_Start.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtPicker_Start.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtPicker_Start.Location = new System.Drawing.Point(517, 4);
            this.dtPicker_Start.Name = "dtPicker_Start";
            this.dtPicker_Start.Size = new System.Drawing.Size(187, 23);
            this.dtPicker_Start.TabIndex = 3;
            // 
            // btn_ExportExcel
            // 
            this.btn_ExportExcel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_ExportExcel.Location = new System.Drawing.Point(1129, 4);
            this.btn_ExportExcel.Name = "btn_ExportExcel";
            this.btn_ExportExcel.Size = new System.Drawing.Size(106, 26);
            this.btn_ExportExcel.TabIndex = 5;
            this.btn_ExportExcel.Text = "Export Excel";
            this.btn_ExportExcel.UseVisualStyleBackColor = true;
            this.btn_ExportExcel.Click += new System.EventHandler(this.btn_ExportExcel_Click);
            // 
            // btn_GetData
            // 
            this.btn_GetData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_GetData.Location = new System.Drawing.Point(1025, 4);
            this.btn_GetData.Name = "btn_GetData";
            this.btn_GetData.Size = new System.Drawing.Size(97, 26);
            this.btn_GetData.TabIndex = 5;
            this.btn_GetData.Text = "Get Data";
            this.btn_GetData.UseVisualStyleBackColor = true;
            this.btn_GetData.Click += new System.EventHandler(this.btn_GetData_Click);
            // 
            // cBox_OnlyPass
            // 
            this.cBox_OnlyPass.AutoSize = true;
            this.cBox_OnlyPass.Location = new System.Drawing.Point(938, 4);
            this.cBox_OnlyPass.Name = "cBox_OnlyPass";
            this.cBox_OnlyPass.Size = new System.Drawing.Size(78, 21);
            this.cBox_OnlyPass.TabIndex = 6;
            this.cBox_OnlyPass.Text = "OnlyPass";
            this.cBox_OnlyPass.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(4, 47);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(1241, 722);
            this.dataGridView1.TabIndex = 2;
            // 
            // UC_DataSummary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Century Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UC_DataSummary";
            this.Size = new System.Drawing.Size(1249, 773);
            this.Load += new System.EventHandler(this.UC_DataSummary_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ComboBox cbb_Station;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbb_PN;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtPicker_End;
        private System.Windows.Forms.DateTimePicker dtPicker_Start;
        private System.Windows.Forms.Button btn_ExportExcel;
        private System.Windows.Forms.Button btn_GetData;
        private System.Windows.Forms.CheckBox cBox_OnlyPass;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
