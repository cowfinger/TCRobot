namespace TC
{
    partial class FormSelectCity
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
            this.listViewCityList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.checkBoxSelecteAllCities = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listViewCityList
            // 
            this.listViewCityList.CheckBoxes = true;
            this.listViewCityList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewCityList.GridLines = true;
            this.listViewCityList.Location = new System.Drawing.Point(13, 48);
            this.listViewCityList.Name = "listViewCityList";
            this.listViewCityList.Size = new System.Drawing.Size(225, 498);
            this.listViewCityList.TabIndex = 0;
            this.listViewCityList.UseCompatibleStateImageBehavior = false;
            this.listViewCityList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "城市";
            this.columnHeader1.Width = 192;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "选择城市";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(81, 562);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(162, 562);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 3;
            this.buttonOk.Text = "Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // checkBoxSelecteAllCities
            // 
            this.checkBoxSelecteAllCities.AutoSize = true;
            this.checkBoxSelecteAllCities.Location = new System.Drawing.Point(15, 27);
            this.checkBoxSelecteAllCities.Name = "checkBoxSelecteAllCities";
            this.checkBoxSelecteAllCities.Size = new System.Drawing.Size(72, 16);
            this.checkBoxSelecteAllCities.TabIndex = 4;
            this.checkBoxSelecteAllCities.Text = "选择全部";
            this.checkBoxSelecteAllCities.UseVisualStyleBackColor = true;
            this.checkBoxSelecteAllCities.CheckedChanged += new System.EventHandler(this.checkBoxSelecteAllCities_CheckedChanged);
            // 
            // FormSelectCity
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(259, 601);
            this.Controls.Add(this.checkBoxSelecteAllCities);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listViewCityList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormSelectCity";
            this.Text = "FormSelectCity";
            this.Load += new System.EventHandler(this.FormSelectCity_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewCityList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.CheckBox checkBoxSelecteAllCities;

    }
}