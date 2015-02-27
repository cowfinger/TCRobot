namespace TC
{
    partial class FormMoveArmy
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
            this.comboBoxAccount = new System.Windows.Forms.ComboBox();
            this.comboBoxFromCity = new System.Windows.Forms.ComboBox();
            this.comboBoxToCity = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.listViewMoveTask = new System.Windows.Forms.ListView();
            this.listViewHeroInfo = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxAccount
            // 
            this.comboBoxAccount.FormattingEnabled = true;
            this.comboBoxAccount.Location = new System.Drawing.Point(83, 12);
            this.comboBoxAccount.Name = "comboBoxAccount";
            this.comboBoxAccount.Size = new System.Drawing.Size(121, 20);
            this.comboBoxAccount.TabIndex = 0;
            // 
            // comboBoxFromCity
            // 
            this.comboBoxFromCity.FormattingEnabled = true;
            this.comboBoxFromCity.Location = new System.Drawing.Point(83, 38);
            this.comboBoxFromCity.Name = "comboBoxFromCity";
            this.comboBoxFromCity.Size = new System.Drawing.Size(121, 20);
            this.comboBoxFromCity.TabIndex = 1;
            // 
            // comboBoxToCity
            // 
            this.comboBoxToCity.FormattingEnabled = true;
            this.comboBoxToCity.Location = new System.Drawing.Point(83, 64);
            this.comboBoxToCity.Name = "comboBoxToCity";
            this.comboBoxToCity.Size = new System.Drawing.Size(121, 20);
            this.comboBoxToCity.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "账号";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "出发城市";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "目标城市";
            // 
            // listViewMoveTask
            // 
            this.listViewMoveTask.Location = new System.Drawing.Point(12, 216);
            this.listViewMoveTask.Name = "listViewMoveTask";
            this.listViewMoveTask.Size = new System.Drawing.Size(512, 97);
            this.listViewMoveTask.TabIndex = 6;
            this.listViewMoveTask.UseCompatibleStateImageBehavior = false;
            // 
            // listViewHeroInfo
            // 
            this.listViewHeroInfo.CheckBoxes = true;
            this.listViewHeroInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewHeroInfo.GridLines = true;
            this.listViewHeroInfo.Location = new System.Drawing.Point(213, 12);
            this.listViewHeroInfo.Name = "listViewHeroInfo";
            this.listViewHeroInfo.Size = new System.Drawing.Size(311, 152);
            this.listViewHeroInfo.TabIndex = 7;
            this.listViewHeroInfo.UseCompatibleStateImageBehavior = false;
            this.listViewHeroInfo.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Type";
            this.columnHeader1.Width = 63;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 87;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Count";
            this.columnHeader3.Width = 86;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(83, 90);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(121, 21);
            this.numericUpDown1.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 92);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "携带比例";
            // 
            // FormMoveArmy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 524);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.listViewHeroInfo);
            this.Controls.Add(this.listViewMoveTask);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxToCity);
            this.Controls.Add(this.comboBoxFromCity);
            this.Controls.Add(this.comboBoxAccount);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormMoveArmy";
            this.Text = "FormMoveArmy";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxAccount;
        private System.Windows.Forms.ComboBox comboBoxFromCity;
        private System.Windows.Forms.ComboBox comboBoxToCity;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView listViewMoveTask;
        private System.Windows.Forms.ListView listViewHeroInfo;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label4;

    }
}