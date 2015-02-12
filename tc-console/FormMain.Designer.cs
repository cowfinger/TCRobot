namespace TC
{
    partial class FormMain
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnLoadProfile = new System.Windows.Forms.Button();
            this.btnAutoAttack = new System.Windows.Forms.Button();
            this.listViewTasks = new System.Windows.Forms.ListView();
            this.btnConfirmMainTeams = new System.Windows.Forms.Button();
            this.listBoxSrcCities = new System.Windows.Forms.ListBox();
            this.listBoxDstCities = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnLoginAll = new System.Windows.Forms.Button();
            this.listViewAccounts = new System.Windows.Forms.ListView();
            this.webBrowserMain = new System.Windows.Forms.WebBrowser();
            this.checkBoxShowBrowers = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxSysTime = new System.Windows.Forms.TextBox();
            this.dateTimePickerArrival = new System.Windows.Forms.DateTimePicker();
            this.rbtnSeqAttack = new System.Windows.Forms.RadioButton();
            this.rbtnSyncAttack = new System.Windows.Forms.RadioButton();
            this.btnScanCity = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.btnQuickArmy = new System.Windows.Forms.Button();
            this.btnDismissTeam = new System.Windows.Forms.Button();
            this.listViewActiveTasks = new System.Windows.Forms.ListView();
            this.colHeaderTaskType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderAccount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderSourceCity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderDestCity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderDuration = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderETA = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkBoxSelectAllTasks = new System.Windows.Forms.CheckBox();
            this.btnMoveTeam = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(632, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 12);
            this.label1.TabIndex = 7;
            // 
            // btnLoadProfile
            // 
            this.btnLoadProfile.Location = new System.Drawing.Point(475, 22);
            this.btnLoadProfile.Name = "btnLoadProfile";
            this.btnLoadProfile.Size = new System.Drawing.Size(75, 21);
            this.btnLoadProfile.TabIndex = 11;
            this.btnLoadProfile.Text = "载入帐号";
            this.btnLoadProfile.UseVisualStyleBackColor = true;
            this.btnLoadProfile.Click += new System.EventHandler(this.btnLoadProfile_Click);
            // 
            // btnAutoAttack
            // 
            this.btnAutoAttack.Enabled = false;
            this.btnAutoAttack.Location = new System.Drawing.Point(557, 22);
            this.btnAutoAttack.Name = "btnAutoAttack";
            this.btnAutoAttack.Size = new System.Drawing.Size(75, 21);
            this.btnAutoAttack.TabIndex = 12;
            this.btnAutoAttack.Text = "压秒";
            this.btnAutoAttack.UseVisualStyleBackColor = true;
            this.btnAutoAttack.Click += new System.EventHandler(this.AutoAttack_Click);
            // 
            // listViewTasks
            // 
            this.listViewTasks.CheckBoxes = true;
            this.listViewTasks.FullRowSelect = true;
            this.listViewTasks.Location = new System.Drawing.Point(14, 268);
            this.listViewTasks.Name = "listViewTasks";
            this.listViewTasks.Size = new System.Drawing.Size(618, 200);
            this.listViewTasks.TabIndex = 13;
            this.listViewTasks.UseCompatibleStateImageBehavior = false;
            this.listViewTasks.View = System.Windows.Forms.View.Details;
            this.listViewTasks.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewTasks_ItemChecked);
            // 
            // btnConfirmMainTeams
            // 
            this.btnConfirmMainTeams.Location = new System.Drawing.Point(557, 49);
            this.btnConfirmMainTeams.Name = "btnConfirmMainTeams";
            this.btnConfirmMainTeams.Size = new System.Drawing.Size(75, 21);
            this.btnConfirmMainTeams.TabIndex = 15;
            this.btnConfirmMainTeams.Text = "确认攻击";
            this.btnConfirmMainTeams.UseVisualStyleBackColor = true;
            this.btnConfirmMainTeams.Click += new System.EventHandler(this.btnConfirmMainTeams_Click);
            // 
            // listBoxSrcCities
            // 
            this.listBoxSrcCities.FormattingEnabled = true;
            this.listBoxSrcCities.ItemHeight = 12;
            this.listBoxSrcCities.Location = new System.Drawing.Point(236, 22);
            this.listBoxSrcCities.Name = "listBoxSrcCities";
            this.listBoxSrcCities.Size = new System.Drawing.Size(102, 184);
            this.listBoxSrcCities.TabIndex = 16;
            this.listBoxSrcCities.SelectedIndexChanged += new System.EventHandler(this.listBoxSrcCities_SelectedIndexChanged);
            // 
            // listBoxDstCities
            // 
            this.listBoxDstCities.FormattingEnabled = true;
            this.listBoxDstCities.ItemHeight = 12;
            this.listBoxDstCities.Location = new System.Drawing.Point(344, 22);
            this.listBoxDstCities.Name = "listBoxDstCities";
            this.listBoxDstCities.Size = new System.Drawing.Size(110, 184);
            this.listBoxDstCities.TabIndex = 17;
            this.listBoxDstCities.SelectedIndexChanged += new System.EventHandler(this.listBoxDstCities_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(233, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 20;
            this.label2.Text = "出发城市";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(341, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 21;
            this.label3.Text = "目标城市";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 220);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 23;
            this.label5.Text = "压秒攻击进度";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 6);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 24;
            this.label6.Text = "帐号列表";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(145, 222);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(101, 12);
            this.label8.TabIndex = 27;
            this.label8.Text = "指定攻击到达时间";
            // 
            // btnLoginAll
            // 
            this.btnLoginAll.Location = new System.Drawing.Point(475, 49);
            this.btnLoginAll.Name = "btnLoginAll";
            this.btnLoginAll.Size = new System.Drawing.Size(75, 21);
            this.btnLoginAll.TabIndex = 28;
            this.btnLoginAll.Text = "登陆所有";
            this.btnLoginAll.UseVisualStyleBackColor = true;
            this.btnLoginAll.Click += new System.EventHandler(this.btnLoginAll_Click);
            // 
            // listViewAccounts
            // 
            this.listViewAccounts.FullRowSelect = true;
            this.listViewAccounts.Location = new System.Drawing.Point(15, 22);
            this.listViewAccounts.Name = "listViewAccounts";
            this.listViewAccounts.Size = new System.Drawing.Size(202, 184);
            this.listViewAccounts.TabIndex = 29;
            this.listViewAccounts.UseCompatibleStateImageBehavior = false;
            this.listViewAccounts.View = System.Windows.Forms.View.Details;
            // 
            // webBrowserMain
            // 
            this.webBrowserMain.Location = new System.Drawing.Point(656, 22);
            this.webBrowserMain.MinimumSize = new System.Drawing.Size(20, 18);
            this.webBrowserMain.Name = "webBrowserMain";
            this.webBrowserMain.Size = new System.Drawing.Size(534, 312);
            this.webBrowserMain.TabIndex = 0;
            // 
            // checkBoxShowBrowers
            // 
            this.checkBoxShowBrowers.AutoSize = true;
            this.checkBoxShowBrowers.Enabled = false;
            this.checkBoxShowBrowers.Location = new System.Drawing.Point(15, 479);
            this.checkBoxShowBrowers.Name = "checkBoxShowBrowers";
            this.checkBoxShowBrowers.Size = new System.Drawing.Size(108, 16);
            this.checkBoxShowBrowers.TabIndex = 30;
            this.checkBoxShowBrowers.Text = "显示浏览器界面";
            this.checkBoxShowBrowers.UseVisualStyleBackColor = true;
            this.checkBoxShowBrowers.CheckedChanged += new System.EventHandler(this.checkBoxShowBrowers_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(412, 479);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 31;
            this.label7.Text = "系统时间";
            // 
            // textBoxSysTime
            // 
            this.textBoxSysTime.Location = new System.Drawing.Point(471, 474);
            this.textBoxSysTime.Name = "textBoxSysTime";
            this.textBoxSysTime.ReadOnly = true;
            this.textBoxSysTime.Size = new System.Drawing.Size(161, 21);
            this.textBoxSysTime.TabIndex = 32;
            // 
            // dateTimePickerArrival
            // 
            this.dateTimePickerArrival.CustomFormat = "MM/dd/yyyy HH:mm:ss";
            this.dateTimePickerArrival.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerArrival.Location = new System.Drawing.Point(254, 220);
            this.dateTimePickerArrival.Name = "dateTimePickerArrival";
            this.dateTimePickerArrival.ShowUpDown = true;
            this.dateTimePickerArrival.Size = new System.Drawing.Size(200, 21);
            this.dateTimePickerArrival.TabIndex = 33;
            // 
            // rbtnSeqAttack
            // 
            this.rbtnSeqAttack.AutoSize = true;
            this.rbtnSeqAttack.Location = new System.Drawing.Point(475, 198);
            this.rbtnSeqAttack.Name = "rbtnSeqAttack";
            this.rbtnSeqAttack.Size = new System.Drawing.Size(71, 16);
            this.rbtnSeqAttack.TabIndex = 34;
            this.rbtnSeqAttack.TabStop = true;
            this.rbtnSeqAttack.Text = "顺序出发";
            this.rbtnSeqAttack.UseVisualStyleBackColor = true;
            // 
            // rbtnSyncAttack
            // 
            this.rbtnSyncAttack.AutoSize = true;
            this.rbtnSyncAttack.Location = new System.Drawing.Point(475, 221);
            this.rbtnSyncAttack.Name = "rbtnSyncAttack";
            this.rbtnSyncAttack.Size = new System.Drawing.Size(71, 16);
            this.rbtnSyncAttack.TabIndex = 35;
            this.rbtnSyncAttack.TabStop = true;
            this.rbtnSyncAttack.Text = "同时出发";
            this.rbtnSyncAttack.UseVisualStyleBackColor = true;
            // 
            // btnScanCity
            // 
            this.btnScanCity.Enabled = false;
            this.btnScanCity.Location = new System.Drawing.Point(475, 76);
            this.btnScanCity.Name = "btnScanCity";
            this.btnScanCity.Size = new System.Drawing.Size(75, 23);
            this.btnScanCity.TabIndex = 36;
            this.btnScanCity.Text = "扫描";
            this.btnScanCity.UseVisualStyleBackColor = true;
            this.btnScanCity.Click += new System.EventHandler(this.btnScanCity_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInfo.Enabled = false;
            this.txtInfo.Location = new System.Drawing.Point(129, 480);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(271, 14);
            this.txtInfo.TabIndex = 37;
            // 
            // btnQuickArmy
            // 
            this.btnQuickArmy.Location = new System.Drawing.Point(557, 76);
            this.btnQuickArmy.Name = "btnQuickArmy";
            this.btnQuickArmy.Size = new System.Drawing.Size(75, 23);
            this.btnQuickArmy.TabIndex = 38;
            this.btnQuickArmy.Text = "单将部队";
            this.btnQuickArmy.UseVisualStyleBackColor = true;
            this.btnQuickArmy.Click += new System.EventHandler(this.btnQuickArmy_Click);
            // 
            // btnDismissTeam
            // 
            this.btnDismissTeam.Enabled = false;
            this.btnDismissTeam.Location = new System.Drawing.Point(557, 106);
            this.btnDismissTeam.Name = "btnDismissTeam";
            this.btnDismissTeam.Size = new System.Drawing.Size(75, 23);
            this.btnDismissTeam.TabIndex = 39;
            this.btnDismissTeam.Text = "解散部队";
            this.btnDismissTeam.UseVisualStyleBackColor = true;
            this.btnDismissTeam.Click += new System.EventHandler(this.btnDismissTeam_Click);
            // 
            // listViewActiveTasks
            // 
            this.listViewActiveTasks.CheckBoxes = true;
            this.listViewActiveTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colHeaderTaskType,
            this.colHeaderAccount,
            this.colHeaderSourceCity,
            this.colHeaderDestCity,
            this.colHeaderDuration,
            this.colHeaderETA});
            this.listViewActiveTasks.FullRowSelect = true;
            this.listViewActiveTasks.GridLines = true;
            this.listViewActiveTasks.Location = new System.Drawing.Point(656, 353);
            this.listViewActiveTasks.Name = "listViewActiveTasks";
            this.listViewActiveTasks.Size = new System.Drawing.Size(439, 221);
            this.listViewActiveTasks.TabIndex = 40;
            this.listViewActiveTasks.UseCompatibleStateImageBehavior = false;
            this.listViewActiveTasks.View = System.Windows.Forms.View.Details;
            // 
            // colHeaderTaskType
            // 
            this.colHeaderTaskType.Text = "任务类型";
            // 
            // colHeaderAccount
            // 
            this.colHeaderAccount.Text = "账号";
            // 
            // colHeaderSourceCity
            // 
            this.colHeaderSourceCity.Text = "出发城市";
            // 
            // colHeaderDestCity
            // 
            this.colHeaderDestCity.Text = "目标城市";
            // 
            // colHeaderDuration
            // 
            this.colHeaderDuration.Text = "持续时间";
            // 
            // colHeaderETA
            // 
            this.colHeaderETA.Text = "完成时间";
            // 
            // checkBoxSelectAllTasks
            // 
            this.checkBoxSelectAllTasks.AutoSize = true;
            this.checkBoxSelectAllTasks.Location = new System.Drawing.Point(15, 246);
            this.checkBoxSelectAllTasks.Name = "checkBoxSelectAllTasks";
            this.checkBoxSelectAllTasks.Size = new System.Drawing.Size(48, 16);
            this.checkBoxSelectAllTasks.TabIndex = 41;
            this.checkBoxSelectAllTasks.Text = "全选";
            this.checkBoxSelectAllTasks.UseVisualStyleBackColor = true;
            this.checkBoxSelectAllTasks.CheckedChanged += new System.EventHandler(this.checkBoxSelectAllTasks_CheckedChanged);
            // 
            // btnMoveTeam
            // 
            this.btnMoveTeam.Location = new System.Drawing.Point(475, 106);
            this.btnMoveTeam.Name = "btnMoveTeam";
            this.btnMoveTeam.Size = new System.Drawing.Size(75, 23);
            this.btnMoveTeam.TabIndex = 42;
            this.btnMoveTeam.Text = "移动部队";
            this.btnMoveTeam.UseVisualStyleBackColor = true;
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(475, 136);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 43;
            this.btnExport.Text = "导出组队";
            this.btnExport.UseVisualStyleBackColor = true;
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(557, 136);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 44;
            this.btnImport.Text = "导入组队";
            this.btnImport.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1203, 618);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnMoveTeam);
            this.Controls.Add(this.checkBoxSelectAllTasks);
            this.Controls.Add(this.listViewActiveTasks);
            this.Controls.Add(this.btnDismissTeam);
            this.Controls.Add(this.btnQuickArmy);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.btnScanCity);
            this.Controls.Add(this.rbtnSyncAttack);
            this.Controls.Add(this.rbtnSeqAttack);
            this.Controls.Add(this.dateTimePickerArrival);
            this.Controls.Add(this.textBoxSysTime);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.checkBoxShowBrowers);
            this.Controls.Add(this.listViewAccounts);
            this.Controls.Add(this.btnLoginAll);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listBoxDstCities);
            this.Controls.Add(this.listBoxSrcCities);
            this.Controls.Add(this.btnConfirmMainTeams);
            this.Controls.Add(this.listViewTasks);
            this.Controls.Add(this.btnAutoAttack);
            this.Controls.Add(this.btnLoadProfile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.webBrowserMain);
            this.Name = "FormMain";
            this.Text = "压秒(3.0)";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLoadProfile;
		private System.Windows.Forms.Button btnAutoAttack;
		private System.Windows.Forms.ListView listViewTasks;
		private System.Windows.Forms.Button btnConfirmMainTeams;
		private System.Windows.Forms.ListBox listBoxSrcCities;
		private System.Windows.Forms.ListBox listBoxDstCities;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button btnLoginAll;
		private System.Windows.Forms.ListView listViewAccounts;
		private System.Windows.Forms.WebBrowser webBrowserMain;
		private System.Windows.Forms.CheckBox checkBoxShowBrowers;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox textBoxSysTime;
		private System.Windows.Forms.DateTimePicker dateTimePickerArrival;
		private System.Windows.Forms.RadioButton rbtnSeqAttack;
		private System.Windows.Forms.RadioButton rbtnSyncAttack;
        private System.Windows.Forms.Button btnScanCity;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.Button btnQuickArmy;
        private System.Windows.Forms.Button btnDismissTeam;
        private System.Windows.Forms.ListView listViewActiveTasks;
        private System.Windows.Forms.ColumnHeader colHeaderTaskType;
        private System.Windows.Forms.ColumnHeader colHeaderAccount;
        private System.Windows.Forms.ColumnHeader colHeaderSourceCity;
        private System.Windows.Forms.ColumnHeader colHeaderDestCity;
        private System.Windows.Forms.ColumnHeader colHeaderDuration;
        private System.Windows.Forms.ColumnHeader colHeaderETA;
        private System.Windows.Forms.CheckBox checkBoxSelectAllTasks;
        private System.Windows.Forms.Button btnMoveTeam;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnImport;
    }
}

