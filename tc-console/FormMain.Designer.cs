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
            this.listViewTroops = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnConfirmMainTroops = new System.Windows.Forms.Button();
            this.listBoxSrcCities = new System.Windows.Forms.ListBox();
            this.listBoxDstCities = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnLoginAll = new System.Windows.Forms.Button();
            this.listViewAccounts = new System.Windows.Forms.ListView();
            this.columnHeaderAccountName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLoginStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.webBrowserMain = new System.Windows.Forms.WebBrowser();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxSysTime = new System.Windows.Forms.TextBox();
            this.dateTimePickerArrival = new System.Windows.Forms.DateTimePicker();
            this.btnScanCity = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.btnQuickCreateTroop = new System.Windows.Forms.Button();
            this.btnDismissTroop = new System.Windows.Forms.Button();
            this.listViewTasks = new System.Windows.Forms.ListView();
            this.colHeaderTaskType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderAccount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderSourceCity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderDestCity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderDuration = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHeaderETA = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderCountingDown = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkBoxSelectAllTasks = new System.Windows.Forms.CheckBox();
            this.checkBoxDefend = new System.Windows.Forms.CheckBox();
            this.btnContribute = new System.Windows.Forms.Button();
            this.listViewInfluence = new System.Windows.Forms.ListView();
            this.columnHeaderAccount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderWood = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMud = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderIron = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFood = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label4 = new System.Windows.Forms.Label();
            this.btnGroupTroop = new System.Windows.Forms.Button();
            this.radioButtonCenturion = new System.Windows.Forms.RadioButton();
            this.radioButtonSmallTroop = new System.Windows.Forms.RadioButton();
            this.radioButtonFullTroop = new System.Windows.Forms.RadioButton();
            this.listViewCompletedTasks = new System.Windows.Forms.ListView();
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnCancelTasks = new System.Windows.Forms.Button();
            this.btnReliveHeroTask = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(631, 184);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 7;
            // 
            // btnLoadProfile
            // 
            this.btnLoadProfile.Location = new System.Drawing.Point(236, 24);
            this.btnLoadProfile.Name = "btnLoadProfile";
            this.btnLoadProfile.Size = new System.Drawing.Size(75, 23);
            this.btnLoadProfile.TabIndex = 11;
            this.btnLoadProfile.Text = "载入帐号";
            this.btnLoadProfile.UseVisualStyleBackColor = true;
            this.btnLoadProfile.Click += new System.EventHandler(this.btnLoadProfile_Click);
            // 
            // btnAutoAttack
            // 
            this.btnAutoAttack.Enabled = false;
            this.btnAutoAttack.Location = new System.Drawing.Point(654, 24);
            this.btnAutoAttack.Name = "btnAutoAttack";
            this.btnAutoAttack.Size = new System.Drawing.Size(75, 23);
            this.btnAutoAttack.TabIndex = 12;
            this.btnAutoAttack.Text = "压秒";
            this.btnAutoAttack.UseVisualStyleBackColor = true;
            this.btnAutoAttack.Click += new System.EventHandler(this.AutoAttack_Click);
            // 
            // listViewTroops
            // 
            this.listViewTroops.CheckBoxes = true;
            this.listViewTroops.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.listViewTroops.FullRowSelect = true;
            this.listViewTroops.GridLines = true;
            this.listViewTroops.Location = new System.Drawing.Point(14, 290);
            this.listViewTroops.Name = "listViewTroops";
            this.listViewTroops.Size = new System.Drawing.Size(618, 454);
            this.listViewTroops.TabIndex = 13;
            this.listViewTroops.UseCompatibleStateImageBehavior = false;
            this.listViewTroops.View = System.Windows.Forms.View.Details;
            this.listViewTroops.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewTasks_ItemChecked);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "账号";
            this.columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "部队编号";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "攻击强度";
            this.columnHeader3.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "途中耗时";
            this.columnHeader4.Width = 80;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "出发倒计时";
            this.columnHeader5.Width = 80;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "组编号";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "部队类型";
            // 
            // btnConfirmMainTroops
            // 
            this.btnConfirmMainTroops.Enabled = false;
            this.btnConfirmMainTroops.Location = new System.Drawing.Point(735, 24);
            this.btnConfirmMainTroops.Name = "btnConfirmMainTroops";
            this.btnConfirmMainTroops.Size = new System.Drawing.Size(75, 23);
            this.btnConfirmMainTroops.TabIndex = 15;
            this.btnConfirmMainTroops.Text = "确认攻击";
            this.btnConfirmMainTroops.UseVisualStyleBackColor = true;
            this.btnConfirmMainTroops.Click += new System.EventHandler(this.btnConfirmMainTroops_Click);
            // 
            // listBoxSrcCities
            // 
            this.listBoxSrcCities.FormattingEnabled = true;
            this.listBoxSrcCities.Location = new System.Drawing.Point(412, 24);
            this.listBoxSrcCities.Name = "listBoxSrcCities";
            this.listBoxSrcCities.Size = new System.Drawing.Size(102, 199);
            this.listBoxSrcCities.TabIndex = 16;
            this.listBoxSrcCities.SelectedIndexChanged += new System.EventHandler(this.listBoxSrcCities_SelectedIndexChanged);
            // 
            // listBoxDstCities
            // 
            this.listBoxDstCities.FormattingEnabled = true;
            this.listBoxDstCities.Location = new System.Drawing.Point(520, 24);
            this.listBoxDstCities.Name = "listBoxDstCities";
            this.listBoxDstCities.Size = new System.Drawing.Size(110, 199);
            this.listBoxDstCities.TabIndex = 17;
            this.listBoxDstCities.SelectedIndexChanged += new System.EventHandler(this.listBoxDstCities_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(409, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "出发城市";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(517, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "目标城市";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 7);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(55, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "帐号列表";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(652, 61);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(103, 13);
            this.label8.TabIndex = 27;
            this.label8.Text = "指定攻击到达时间";
            // 
            // btnLoginAll
            // 
            this.btnLoginAll.Enabled = false;
            this.btnLoginAll.Location = new System.Drawing.Point(236, 52);
            this.btnLoginAll.Name = "btnLoginAll";
            this.btnLoginAll.Size = new System.Drawing.Size(75, 23);
            this.btnLoginAll.TabIndex = 28;
            this.btnLoginAll.Text = "登陆所有";
            this.btnLoginAll.UseVisualStyleBackColor = true;
            this.btnLoginAll.Click += new System.EventHandler(this.btnLoginAll_Click);
            // 
            // listViewAccounts
            // 
            this.listViewAccounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAccountName,
            this.columnHeaderLoginStatus});
            this.listViewAccounts.FullRowSelect = true;
            this.listViewAccounts.Location = new System.Drawing.Point(15, 24);
            this.listViewAccounts.Name = "listViewAccounts";
            this.listViewAccounts.Size = new System.Drawing.Size(202, 232);
            this.listViewAccounts.TabIndex = 29;
            this.listViewAccounts.UseCompatibleStateImageBehavior = false;
            this.listViewAccounts.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderAccountName
            // 
            this.columnHeaderAccountName.Text = "账号";
            this.columnHeaderAccountName.Width = 121;
            // 
            // columnHeaderLoginStatus
            // 
            this.columnHeaderLoginStatus.Text = "状态";
            // 
            // webBrowserMain
            // 
            this.webBrowserMain.Location = new System.Drawing.Point(1252, 666);
            this.webBrowserMain.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserMain.Name = "webBrowserMain";
            this.webBrowserMain.Size = new System.Drawing.Size(110, 53);
            this.webBrowserMain.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(1010, 62);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 31;
            this.label7.Text = "系统时间";
            // 
            // textBoxSysTime
            // 
            this.textBoxSysTime.Location = new System.Drawing.Point(1071, 59);
            this.textBoxSysTime.Name = "textBoxSysTime";
            this.textBoxSysTime.ReadOnly = true;
            this.textBoxSysTime.Size = new System.Drawing.Size(161, 20);
            this.textBoxSysTime.TabIndex = 32;
            // 
            // dateTimePickerArrival
            // 
            this.dateTimePickerArrival.CustomFormat = "MM/dd/yyyy HH:mm:ss";
            this.dateTimePickerArrival.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerArrival.Location = new System.Drawing.Point(759, 58);
            this.dateTimePickerArrival.Name = "dateTimePickerArrival";
            this.dateTimePickerArrival.ShowUpDown = true;
            this.dateTimePickerArrival.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerArrival.TabIndex = 33;
            // 
            // btnScanCity
            // 
            this.btnScanCity.Enabled = false;
            this.btnScanCity.Location = new System.Drawing.Point(236, 80);
            this.btnScanCity.Name = "btnScanCity";
            this.btnScanCity.Size = new System.Drawing.Size(75, 25);
            this.btnScanCity.TabIndex = 36;
            this.btnScanCity.Text = "扫描";
            this.btnScanCity.UseVisualStyleBackColor = true;
            this.btnScanCity.Click += new System.EventHandler(this.btnScanCity_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInfo.Enabled = false;
            this.txtInfo.Location = new System.Drawing.Point(129, 757);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(271, 13);
            this.txtInfo.TabIndex = 37;
            // 
            // btnQuickCreateTroop
            // 
            this.btnQuickCreateTroop.Enabled = false;
            this.btnQuickCreateTroop.Location = new System.Drawing.Point(236, 262);
            this.btnQuickCreateTroop.Name = "btnQuickCreateTroop";
            this.btnQuickCreateTroop.Size = new System.Drawing.Size(75, 25);
            this.btnQuickCreateTroop.TabIndex = 38;
            this.btnQuickCreateTroop.Text = "快速部队";
            this.btnQuickCreateTroop.UseVisualStyleBackColor = true;
            this.btnQuickCreateTroop.Click += new System.EventHandler(this.btnQuickCreateTroop_Click);
            // 
            // btnDismissTroop
            // 
            this.btnDismissTroop.Enabled = false;
            this.btnDismissTroop.Location = new System.Drawing.Point(152, 262);
            this.btnDismissTroop.Name = "btnDismissTroop";
            this.btnDismissTroop.Size = new System.Drawing.Size(75, 25);
            this.btnDismissTroop.TabIndex = 39;
            this.btnDismissTroop.Text = "解散部队";
            this.btnDismissTroop.UseVisualStyleBackColor = true;
            this.btnDismissTroop.Click += new System.EventHandler(this.btnDismissTroop_Click);
            // 
            // listViewTasks
            // 
            this.listViewTasks.CheckBoxes = true;
            this.listViewTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colHeaderTaskType,
            this.colHeaderAccount,
            this.colHeaderSourceCity,
            this.colHeaderDestCity,
            this.colHeaderDuration,
            this.colHeaderETA,
            this.columnHeaderCountingDown});
            this.listViewTasks.FullRowSelect = true;
            this.listViewTasks.GridLines = true;
            this.listViewTasks.Location = new System.Drawing.Point(654, 84);
            this.listViewTasks.Name = "listViewTasks";
            this.listViewTasks.Size = new System.Drawing.Size(578, 303);
            this.listViewTasks.TabIndex = 40;
            this.listViewTasks.UseCompatibleStateImageBehavior = false;
            this.listViewTasks.View = System.Windows.Forms.View.Details;
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
            this.colHeaderDestCity.Width = 68;
            // 
            // colHeaderDuration
            // 
            this.colHeaderDuration.Text = "到达时间";
            this.colHeaderDuration.Width = 96;
            // 
            // colHeaderETA
            // 
            this.colHeaderETA.Text = "完成时间";
            this.colHeaderETA.Width = 111;
            // 
            // columnHeaderCountingDown
            // 
            this.columnHeaderCountingDown.Text = "倒计时";
            this.columnHeaderCountingDown.Width = 104;
            // 
            // checkBoxSelectAllTasks
            // 
            this.checkBoxSelectAllTasks.AutoSize = true;
            this.checkBoxSelectAllTasks.Location = new System.Drawing.Point(15, 267);
            this.checkBoxSelectAllTasks.Name = "checkBoxSelectAllTasks";
            this.checkBoxSelectAllTasks.Size = new System.Drawing.Size(50, 17);
            this.checkBoxSelectAllTasks.TabIndex = 41;
            this.checkBoxSelectAllTasks.Text = "全选";
            this.checkBoxSelectAllTasks.UseVisualStyleBackColor = true;
            this.checkBoxSelectAllTasks.CheckedChanged += new System.EventHandler(this.checkBoxSelectAllTasks_CheckedChanged);
            // 
            // checkBoxDefend
            // 
            this.checkBoxDefend.AutoSize = true;
            this.checkBoxDefend.Location = new System.Drawing.Point(477, 267);
            this.checkBoxDefend.Name = "checkBoxDefend";
            this.checkBoxDefend.Size = new System.Drawing.Size(74, 17);
            this.checkBoxDefend.TabIndex = 42;
            this.checkBoxDefend.Text = "防御部队";
            this.checkBoxDefend.UseVisualStyleBackColor = true;
            // 
            // btnContribute
            // 
            this.btnContribute.Enabled = false;
            this.btnContribute.Location = new System.Drawing.Point(236, 140);
            this.btnContribute.Name = "btnContribute";
            this.btnContribute.Size = new System.Drawing.Size(75, 25);
            this.btnContribute.TabIndex = 43;
            this.btnContribute.Text = "捐粮";
            this.btnContribute.UseVisualStyleBackColor = true;
            this.btnContribute.Click += new System.EventHandler(this.btnContribute_Click);
            // 
            // listViewInfluence
            // 
            this.listViewInfluence.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAccount,
            this.columnHeaderWood,
            this.columnHeaderMud,
            this.columnHeaderIron,
            this.columnHeaderFood});
            this.listViewInfluence.FullRowSelect = true;
            this.listViewInfluence.GridLines = true;
            this.listViewInfluence.Location = new System.Drawing.Point(15, 527);
            this.listViewInfluence.Name = "listViewInfluence";
            this.listViewInfluence.Size = new System.Drawing.Size(617, 217);
            this.listViewInfluence.TabIndex = 44;
            this.listViewInfluence.UseCompatibleStateImageBehavior = false;
            this.listViewInfluence.View = System.Windows.Forms.View.Details;
            this.listViewInfluence.Visible = false;
            // 
            // columnHeaderAccount
            // 
            this.columnHeaderAccount.Text = "账号";
            this.columnHeaderAccount.Width = 80;
            // 
            // columnHeaderWood
            // 
            this.columnHeaderWood.Text = "木材";
            this.columnHeaderWood.Width = 100;
            // 
            // columnHeaderMud
            // 
            this.columnHeaderMud.Text = "泥土";
            this.columnHeaderMud.Width = 100;
            // 
            // columnHeaderIron
            // 
            this.columnHeaderIron.Text = "铁矿";
            this.columnHeaderIron.Width = 100;
            // 
            // columnHeaderFood
            // 
            this.columnHeaderFood.Text = "粮食";
            this.columnHeaderFood.Width = 100;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 510);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 45;
            this.label4.Text = "势力资源";
            this.label4.Visible = false;
            // 
            // btnGroupTroop
            // 
            this.btnGroupTroop.Enabled = false;
            this.btnGroupTroop.Location = new System.Drawing.Point(71, 262);
            this.btnGroupTroop.Name = "btnGroupTroop";
            this.btnGroupTroop.Size = new System.Drawing.Size(75, 25);
            this.btnGroupTroop.TabIndex = 47;
            this.btnGroupTroop.Text = "联合部队";
            this.btnGroupTroop.UseVisualStyleBackColor = true;
            this.btnGroupTroop.Click += new System.EventHandler(this.btnGroupTroop_Click);
            // 
            // radioButtonCenturion
            // 
            this.radioButtonCenturion.AutoSize = true;
            this.radioButtonCenturion.Location = new System.Drawing.Point(327, 266);
            this.radioButtonCenturion.Name = "radioButtonCenturion";
            this.radioButtonCenturion.Size = new System.Drawing.Size(49, 17);
            this.radioButtonCenturion.TabIndex = 48;
            this.radioButtonCenturion.Text = "单将";
            this.radioButtonCenturion.UseVisualStyleBackColor = true;
            // 
            // radioButtonSmallTroop
            // 
            this.radioButtonSmallTroop.AutoSize = true;
            this.radioButtonSmallTroop.Checked = true;
            this.radioButtonSmallTroop.Location = new System.Drawing.Point(376, 266);
            this.radioButtonSmallTroop.Name = "radioButtonSmallTroop";
            this.radioButtonSmallTroop.Size = new System.Drawing.Size(49, 17);
            this.radioButtonSmallTroop.TabIndex = 49;
            this.radioButtonSmallTroop.TabStop = true;
            this.radioButtonSmallTroop.Text = "千人";
            this.radioButtonSmallTroop.UseVisualStyleBackColor = true;
            // 
            // radioButtonFullTroop
            // 
            this.radioButtonFullTroop.AutoSize = true;
            this.radioButtonFullTroop.Location = new System.Drawing.Point(426, 266);
            this.radioButtonFullTroop.Name = "radioButtonFullTroop";
            this.radioButtonFullTroop.Size = new System.Drawing.Size(49, 17);
            this.radioButtonFullTroop.TabIndex = 50;
            this.radioButtonFullTroop.TabStop = true;
            this.radioButtonFullTroop.Text = "全力";
            this.radioButtonFullTroop.UseVisualStyleBackColor = true;
            // 
            // listViewCompletedTasks
            // 
            this.listViewCompletedTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader8,
            this.columnHeader12,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader13});
            this.listViewCompletedTasks.GridLines = true;
            this.listViewCompletedTasks.Location = new System.Drawing.Point(654, 408);
            this.listViewCompletedTasks.Name = "listViewCompletedTasks";
            this.listViewCompletedTasks.Size = new System.Drawing.Size(578, 336);
            this.listViewCompletedTasks.TabIndex = 51;
            this.listViewCompletedTasks.UseCompatibleStateImageBehavior = false;
            this.listViewCompletedTasks.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Account";
            this.columnHeader8.Width = 127;
            // 
            // columnHeader12
            // 
            this.columnHeader12.DisplayIndex = 4;
            this.columnHeader12.Text = "ID";
            this.columnHeader12.Width = 96;
            // 
            // columnHeader9
            // 
            this.columnHeader9.DisplayIndex = 1;
            this.columnHeader9.Text = "From";
            this.columnHeader9.Width = 86;
            // 
            // columnHeader10
            // 
            this.columnHeader10.DisplayIndex = 2;
            this.columnHeader10.Text = "To";
            this.columnHeader10.Width = 88;
            // 
            // columnHeader11
            // 
            this.columnHeader11.DisplayIndex = 3;
            this.columnHeader11.Text = "ETA";
            this.columnHeader11.Width = 135;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "Type";
            // 
            // btnCancelTasks
            // 
            this.btnCancelTasks.Location = new System.Drawing.Point(816, 24);
            this.btnCancelTasks.Name = "btnCancelTasks";
            this.btnCancelTasks.Size = new System.Drawing.Size(75, 23);
            this.btnCancelTasks.TabIndex = 52;
            this.btnCancelTasks.Text = "取消";
            this.btnCancelTasks.UseVisualStyleBackColor = true;
            this.btnCancelTasks.Click += new System.EventHandler(this.btnCancelTasks_Click);
            // 
            // btnReliveHeroTask
            // 
            this.btnReliveHeroTask.Location = new System.Drawing.Point(236, 111);
            this.btnReliveHeroTask.Name = "btnReliveHeroTask";
            this.btnReliveHeroTask.Size = new System.Drawing.Size(75, 23);
            this.btnReliveHeroTask.TabIndex = 53;
            this.btnReliveHeroTask.Text = "复活武将\r\n";
            this.btnReliveHeroTask.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1246, 782);
            this.Controls.Add(this.btnReliveHeroTask);
            this.Controls.Add(this.btnCancelTasks);
            this.Controls.Add(this.listViewCompletedTasks);
            this.Controls.Add(this.radioButtonFullTroop);
            this.Controls.Add(this.radioButtonSmallTroop);
            this.Controls.Add(this.radioButtonCenturion);
            this.Controls.Add(this.btnGroupTroop);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.listViewInfluence);
            this.Controls.Add(this.btnContribute);
            this.Controls.Add(this.checkBoxDefend);
            this.Controls.Add(this.checkBoxSelectAllTasks);
            this.Controls.Add(this.listViewTasks);
            this.Controls.Add(this.btnDismissTroop);
            this.Controls.Add(this.btnQuickCreateTroop);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.btnScanCity);
            this.Controls.Add(this.dateTimePickerArrival);
            this.Controls.Add(this.textBoxSysTime);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.listViewAccounts);
            this.Controls.Add(this.btnLoginAll);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listBoxDstCities);
            this.Controls.Add(this.listBoxSrcCities);
            this.Controls.Add(this.btnConfirmMainTroops);
            this.Controls.Add(this.listViewTroops);
            this.Controls.Add(this.btnAutoAttack);
            this.Controls.Add(this.btnLoadProfile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.webBrowserMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "TC Console (1.0)";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnLoadProfile;
		private System.Windows.Forms.Button btnAutoAttack;
		private System.Windows.Forms.ListView listViewTroops;
		private System.Windows.Forms.Button btnConfirmMainTroops;
		private System.Windows.Forms.ListBox listBoxSrcCities;
		private System.Windows.Forms.ListBox listBoxDstCities;
		private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button btnLoginAll;
		private System.Windows.Forms.ListView listViewAccounts;
        private System.Windows.Forms.WebBrowser webBrowserMain;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox textBoxSysTime;
        private System.Windows.Forms.DateTimePicker dateTimePickerArrival;
        private System.Windows.Forms.Button btnScanCity;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.Button btnQuickCreateTroop;
        private System.Windows.Forms.Button btnDismissTroop;
        private System.Windows.Forms.ListView listViewTasks;
        private System.Windows.Forms.ColumnHeader colHeaderTaskType;
        private System.Windows.Forms.ColumnHeader colHeaderAccount;
        private System.Windows.Forms.ColumnHeader colHeaderSourceCity;
        private System.Windows.Forms.ColumnHeader colHeaderDestCity;
        private System.Windows.Forms.ColumnHeader colHeaderDuration;
        private System.Windows.Forms.ColumnHeader colHeaderETA;
        private System.Windows.Forms.CheckBox checkBoxSelectAllTasks;
        private System.Windows.Forms.CheckBox checkBoxDefend;
        private System.Windows.Forms.Button btnContribute;
        private System.Windows.Forms.ListView listViewInfluence;
        private System.Windows.Forms.ColumnHeader columnHeaderAccount;
        private System.Windows.Forms.ColumnHeader columnHeaderWood;
        private System.Windows.Forms.ColumnHeader columnHeaderMud;
        private System.Windows.Forms.ColumnHeader columnHeaderIron;
        private System.Windows.Forms.ColumnHeader columnHeaderFood;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeaderAccountName;
        private System.Windows.Forms.ColumnHeader columnHeaderLoginStatus;
        private System.Windows.Forms.ColumnHeader columnHeaderCountingDown;
        private System.Windows.Forms.Button btnGroupTroop;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.RadioButton radioButtonCenturion;
        private System.Windows.Forms.RadioButton radioButtonSmallTroop;
        private System.Windows.Forms.RadioButton radioButtonFullTroop;
        private System.Windows.Forms.ListView listViewCompletedTasks;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.Button btnCancelTasks;
        private System.Windows.Forms.Button btnReliveHeroTask;
    }
}

