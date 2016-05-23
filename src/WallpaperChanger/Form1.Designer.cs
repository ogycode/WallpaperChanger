namespace WallpaperChanger
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.cbPeriod = new System.Windows.Forms.ComboBox();
            this.cbSource = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbHideTray = new System.Windows.Forms.CheckBox();
            this.cbAutorun = new System.Windows.Forms.CheckBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cbWallpaperSet = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbLang = new System.Windows.Forms.ComboBox();
            this.timerPeriod = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ncpStart = new System.Windows.Forms.ToolStripMenuItem();
            this.ncpApplyNow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ncmAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.ncmExit = new System.Windows.Forms.ToolStripMenuItem();
            this.btnStart = new System.Windows.Forms.Button();
            this.notifyContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbPeriod
            // 
            this.cbPeriod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbPeriod, "cbPeriod");
            this.cbPeriod.FormattingEnabled = true;
            this.cbPeriod.Items.AddRange(new object[] {
            resources.GetString("cbPeriod.Items"),
            resources.GetString("cbPeriod.Items1"),
            resources.GetString("cbPeriod.Items2"),
            resources.GetString("cbPeriod.Items3"),
            resources.GetString("cbPeriod.Items4"),
            resources.GetString("cbPeriod.Items5"),
            resources.GetString("cbPeriod.Items6")});
            this.cbPeriod.Name = "cbPeriod";
            this.cbPeriod.SelectedIndexChanged += new System.EventHandler(this.cbPeriodSelectedChanged);
            // 
            // cbSource
            // 
            this.cbSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbSource, "cbSource");
            this.cbSource.FormattingEnabled = true;
            this.cbSource.Items.AddRange(new object[] {
            resources.GetString("cbSource.Items")});
            this.cbSource.Name = "cbSource";
            this.cbSource.SelectedIndexChanged += new System.EventHandler(this.cbSourceSelectedChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // cbHideTray
            // 
            resources.ApplyResources(this.cbHideTray, "cbHideTray");
            this.cbHideTray.Name = "cbHideTray";
            this.cbHideTray.UseVisualStyleBackColor = true;
            this.cbHideTray.Click += new System.EventHandler(this.cbHideTrayClick);
            // 
            // cbAutorun
            // 
            resources.ApplyResources(this.cbAutorun, "cbAutorun");
            this.cbAutorun.Name = "cbAutorun";
            this.cbAutorun.UseVisualStyleBackColor = true;
            this.cbAutorun.Click += new System.EventHandler(this.cbAutorunClick);
            // 
            // btnApply
            // 
            resources.ApplyResources(this.btnApply, "btnApply");
            this.btnApply.Name = "btnApply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnAbout
            // 
            resources.ApplyResources(this.btnAbout, "btnAbout");
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cbWallpaperSet
            // 
            this.cbWallpaperSet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbWallpaperSet, "cbWallpaperSet");
            this.cbWallpaperSet.FormattingEnabled = true;
            this.cbWallpaperSet.Items.AddRange(new object[] {
            resources.GetString("cbWallpaperSet.Items"),
            resources.GetString("cbWallpaperSet.Items1"),
            resources.GetString("cbWallpaperSet.Items2")});
            this.cbWallpaperSet.Name = "cbWallpaperSet";
            this.cbWallpaperSet.SelectedIndexChanged += new System.EventHandler(this.cbWallpaperSetSelectedChanged);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // cbLang
            // 
            this.cbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbLang, "cbLang");
            this.cbLang.FormattingEnabled = true;
            this.cbLang.Items.AddRange(new object[] {
            resources.GetString("cbLang.Items"),
            resources.GetString("cbLang.Items1")});
            this.cbLang.Name = "cbLang";
            this.cbLang.SelectedIndexChanged += new System.EventHandler(this.cbLangSelectedChanged);
            // 
            // timerPeriod
            // 
            this.timerPeriod.Interval = 1500000;
            this.timerPeriod.Tick += new System.EventHandler(this.timerPeriodTick);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.notifyContextMenu;
            resources.ApplyResources(this.notifyIcon, "notifyIcon");
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIconDoubleClick);
            // 
            // notifyContextMenu
            // 
            this.notifyContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ncpStart,
            this.ncpApplyNow,
            this.toolStripSeparator1,
            this.ncmAbout,
            this.ncmExit});
            this.notifyContextMenu.Name = "notifyContextMenu";
            resources.ApplyResources(this.notifyContextMenu, "notifyContextMenu");
            // 
            // ncpStart
            // 
            this.ncpStart.Name = "ncpStart";
            resources.ApplyResources(this.ncpStart, "ncpStart");
            this.ncpStart.Click += new System.EventHandler(this.ncpStartClick);
            // 
            // ncpApplyNow
            // 
            this.ncpApplyNow.Name = "ncpApplyNow";
            resources.ApplyResources(this.ncpApplyNow, "ncpApplyNow");
            this.ncpApplyNow.Click += new System.EventHandler(this.ncpApplyNowClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // ncmAbout
            // 
            this.ncmAbout.Name = "ncmAbout";
            resources.ApplyResources(this.ncmAbout, "ncmAbout");
            // 
            // ncmExit
            // 
            this.ncmExit.Name = "ncmExit";
            resources.ApplyResources(this.ncmExit, "ncmExit");
            this.ncmExit.Click += new System.EventHandler(this.ncpExitClick);
            // 
            // btnStart
            // 
            resources.ApplyResources(this.btnStart, "btnStart");
            this.btnStart.Name = "btnStart";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbLang);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbWallpaperSet);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.cbAutorun);
            this.Controls.Add(this.cbHideTray);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbSource);
            this.Controls.Add(this.cbPeriod);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formClosing);
            this.Load += new System.EventHandler(this.formLoad);
            this.notifyContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbPeriod;
        private System.Windows.Forms.ComboBox cbSource;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbHideTray;
        private System.Windows.Forms.CheckBox cbAutorun;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbWallpaperSet;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbLang;
        private System.Windows.Forms.Timer timerPeriod;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip notifyContextMenu;
        private System.Windows.Forms.ToolStripMenuItem ncmAbout;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ncmExit;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ToolStripMenuItem ncpStart;
        private System.Windows.Forms.ToolStripMenuItem ncpApplyNow;
    }
}

