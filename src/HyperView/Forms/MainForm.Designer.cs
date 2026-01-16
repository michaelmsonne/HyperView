namespace HyperView
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            datagridviewVMOverView = new DataGridView();
            tabcontrolMainForm = new TabControl();
            tabpagehvOverview = new TabPage();
            tabpageVMGroups = new TabPage();
            tabpageManageNetwork = new TabPage();
            tabpagehvHosts = new TabPage();
            tabpagehvClusters = new TabPage();
            tabpagehvStorage = new TabPage();
            tabpagehvNetworking = new TabPage();
            tabpagehvCheckpoints = new TabPage();
            tabpagehvReplica = new TabPage();
            tabpagehvResources = new TabPage();
            tabpagehvSecurity = new TabPage();
            tabpagehvPerformance = new TabPage();
            tabpagehvCompliance = new TabPage();
            tabpagehvInventory = new TabPage();
            tabpageCreateVM = new TabPage();
            tabpageHealthOverview = new TabPage();
            menuStripTopMainForm = new MenuStrip();
            menuToolStripMenuItem = new ToolStripMenuItem();
            onlineToolStripMenuItem = new ToolStripMenuItem();
            myWebpageToolStripMenuItem = new ToolStripMenuItem();
            myBlogToolStripMenuItem = new ToolStripMenuItem();
            guideToolStripMenuItem = new ToolStripMenuItem();
            logsToolStripMenuItem = new ToolStripMenuItem();
            openLogFolderToolStripMenuItem = new ToolStripMenuItem();
            openLogForTodayToolStripMenuItem = new ToolStripMenuItem();
            downloadLastestReleaseFromGitHubToolStripMenuItem = new ToolStripMenuItem();
            changelogToolStripMenuItem = new ToolStripMenuItem();
            exportDataToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)datagridviewVMOverView).BeginInit();
            tabcontrolMainForm.SuspendLayout();
            tabpagehvOverview.SuspendLayout();
            menuStripTopMainForm.SuspendLayout();
            SuspendLayout();
            // 
            // datagridviewVMOverView
            // 
            datagridviewVMOverView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewVMOverView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewVMOverView.Location = new Point(6, 38);
            datagridviewVMOverView.Name = "datagridviewVMOverView";
            datagridviewVMOverView.Size = new Size(1601, 800);
            datagridviewVMOverView.TabIndex = 0;
            // 
            // tabcontrolMainForm
            // 
            tabcontrolMainForm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabcontrolMainForm.Controls.Add(tabpagehvOverview);
            tabcontrolMainForm.Controls.Add(tabpageVMGroups);
            tabcontrolMainForm.Controls.Add(tabpageManageNetwork);
            tabcontrolMainForm.Controls.Add(tabpagehvHosts);
            tabcontrolMainForm.Controls.Add(tabpagehvClusters);
            tabcontrolMainForm.Controls.Add(tabpagehvStorage);
            tabcontrolMainForm.Controls.Add(tabpagehvNetworking);
            tabcontrolMainForm.Controls.Add(tabpagehvCheckpoints);
            tabcontrolMainForm.Controls.Add(tabpagehvReplica);
            tabcontrolMainForm.Controls.Add(tabpagehvResources);
            tabcontrolMainForm.Controls.Add(tabpagehvSecurity);
            tabcontrolMainForm.Controls.Add(tabpagehvPerformance);
            tabcontrolMainForm.Controls.Add(tabpagehvCompliance);
            tabcontrolMainForm.Controls.Add(tabpagehvInventory);
            tabcontrolMainForm.Controls.Add(tabpageCreateVM);
            tabcontrolMainForm.Controls.Add(tabpageHealthOverview);
            tabcontrolMainForm.Location = new Point(12, 27);
            tabcontrolMainForm.Name = "tabcontrolMainForm";
            tabcontrolMainForm.SelectedIndex = 0;
            tabcontrolMainForm.Size = new Size(1621, 872);
            tabcontrolMainForm.TabIndex = 1;
            // 
            // tabpagehvOverview
            // 
            tabpagehvOverview.Controls.Add(datagridviewVMOverView);
            tabpagehvOverview.Location = new Point(4, 24);
            tabpagehvOverview.Name = "tabpagehvOverview";
            tabpagehvOverview.Padding = new Padding(3);
            tabpagehvOverview.Size = new Size(1613, 844);
            tabpagehvOverview.TabIndex = 0;
            tabpagehvOverview.Text = "hvOverview";
            tabpagehvOverview.UseVisualStyleBackColor = true;
            // 
            // tabpageVMGroups
            // 
            tabpageVMGroups.Location = new Point(4, 24);
            tabpageVMGroups.Name = "tabpageVMGroups";
            tabpageVMGroups.Padding = new Padding(3);
            tabpageVMGroups.Size = new Size(1613, 769);
            tabpageVMGroups.TabIndex = 1;
            tabpageVMGroups.Text = "Manage VM Groups";
            tabpageVMGroups.UseVisualStyleBackColor = true;
            // 
            // tabpageManageNetwork
            // 
            tabpageManageNetwork.Location = new Point(4, 24);
            tabpageManageNetwork.Name = "tabpageManageNetwork";
            tabpageManageNetwork.Size = new Size(1613, 769);
            tabpageManageNetwork.TabIndex = 2;
            tabpageManageNetwork.Text = "Manage VM Networks";
            tabpageManageNetwork.UseVisualStyleBackColor = true;
            // 
            // tabpagehvHosts
            // 
            tabpagehvHosts.Location = new Point(4, 24);
            tabpagehvHosts.Name = "tabpagehvHosts";
            tabpagehvHosts.Size = new Size(1613, 769);
            tabpagehvHosts.TabIndex = 3;
            tabpagehvHosts.Text = "hvHosts";
            tabpagehvHosts.UseVisualStyleBackColor = true;
            // 
            // tabpagehvClusters
            // 
            tabpagehvClusters.Location = new Point(4, 24);
            tabpagehvClusters.Name = "tabpagehvClusters";
            tabpagehvClusters.Size = new Size(1613, 769);
            tabpagehvClusters.TabIndex = 4;
            tabpagehvClusters.Text = "hvClusters";
            tabpagehvClusters.UseVisualStyleBackColor = true;
            // 
            // tabpagehvStorage
            // 
            tabpagehvStorage.Location = new Point(4, 24);
            tabpagehvStorage.Name = "tabpagehvStorage";
            tabpagehvStorage.Size = new Size(1613, 769);
            tabpagehvStorage.TabIndex = 5;
            tabpagehvStorage.Text = "hvStorage";
            tabpagehvStorage.UseVisualStyleBackColor = true;
            // 
            // tabpagehvNetworking
            // 
            tabpagehvNetworking.Location = new Point(4, 24);
            tabpagehvNetworking.Name = "tabpagehvNetworking";
            tabpagehvNetworking.Size = new Size(1613, 769);
            tabpagehvNetworking.TabIndex = 6;
            tabpagehvNetworking.Text = "hvNetworking";
            tabpagehvNetworking.UseVisualStyleBackColor = true;
            // 
            // tabpagehvCheckpoints
            // 
            tabpagehvCheckpoints.Location = new Point(4, 24);
            tabpagehvCheckpoints.Name = "tabpagehvCheckpoints";
            tabpagehvCheckpoints.Size = new Size(1613, 769);
            tabpagehvCheckpoints.TabIndex = 7;
            tabpagehvCheckpoints.Text = "hvCheckpoints";
            tabpagehvCheckpoints.UseVisualStyleBackColor = true;
            // 
            // tabpagehvReplica
            // 
            tabpagehvReplica.Location = new Point(4, 24);
            tabpagehvReplica.Name = "tabpagehvReplica";
            tabpagehvReplica.Size = new Size(1613, 769);
            tabpagehvReplica.TabIndex = 8;
            tabpagehvReplica.Text = "hvReplica";
            tabpagehvReplica.UseVisualStyleBackColor = true;
            // 
            // tabpagehvResources
            // 
            tabpagehvResources.Location = new Point(4, 24);
            tabpagehvResources.Name = "tabpagehvResources";
            tabpagehvResources.Size = new Size(1613, 769);
            tabpagehvResources.TabIndex = 9;
            tabpagehvResources.Text = "hvResources";
            tabpagehvResources.UseVisualStyleBackColor = true;
            // 
            // tabpagehvSecurity
            // 
            tabpagehvSecurity.Location = new Point(4, 24);
            tabpagehvSecurity.Name = "tabpagehvSecurity";
            tabpagehvSecurity.Size = new Size(1613, 769);
            tabpagehvSecurity.TabIndex = 10;
            tabpagehvSecurity.Text = "hvSecurity";
            tabpagehvSecurity.UseVisualStyleBackColor = true;
            // 
            // tabpagehvPerformance
            // 
            tabpagehvPerformance.Location = new Point(4, 24);
            tabpagehvPerformance.Name = "tabpagehvPerformance";
            tabpagehvPerformance.Size = new Size(1613, 769);
            tabpagehvPerformance.TabIndex = 11;
            tabpagehvPerformance.Text = "hvPerformance";
            tabpagehvPerformance.UseVisualStyleBackColor = true;
            // 
            // tabpagehvCompliance
            // 
            tabpagehvCompliance.Location = new Point(4, 24);
            tabpagehvCompliance.Name = "tabpagehvCompliance";
            tabpagehvCompliance.Size = new Size(1613, 769);
            tabpagehvCompliance.TabIndex = 12;
            tabpagehvCompliance.Text = "hvCompliance";
            tabpagehvCompliance.UseVisualStyleBackColor = true;
            // 
            // tabpagehvInventory
            // 
            tabpagehvInventory.Location = new Point(4, 24);
            tabpagehvInventory.Name = "tabpagehvInventory";
            tabpagehvInventory.Size = new Size(1613, 769);
            tabpagehvInventory.TabIndex = 13;
            tabpagehvInventory.Text = "hvInventory";
            tabpagehvInventory.UseVisualStyleBackColor = true;
            // 
            // tabpageCreateVM
            // 
            tabpageCreateVM.Location = new Point(4, 24);
            tabpageCreateVM.Name = "tabpageCreateVM";
            tabpageCreateVM.Size = new Size(1613, 769);
            tabpageCreateVM.TabIndex = 14;
            tabpageCreateVM.Text = "Create VM´s";
            tabpageCreateVM.UseVisualStyleBackColor = true;
            // 
            // tabpageHealthOverview
            // 
            tabpageHealthOverview.Location = new Point(4, 24);
            tabpageHealthOverview.Name = "tabpageHealthOverview";
            tabpageHealthOverview.Size = new Size(1613, 769);
            tabpageHealthOverview.TabIndex = 15;
            tabpageHealthOverview.Text = "Health Overview";
            tabpageHealthOverview.UseVisualStyleBackColor = true;
            // 
            // menuStripTopMainForm
            // 
            menuStripTopMainForm.Items.AddRange(new ToolStripItem[] { menuToolStripMenuItem });
            menuStripTopMainForm.Location = new Point(0, 0);
            menuStripTopMainForm.Name = "menuStripTopMainForm";
            menuStripTopMainForm.Size = new Size(1645, 24);
            menuStripTopMainForm.TabIndex = 2;
            menuStripTopMainForm.Text = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            menuToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { onlineToolStripMenuItem, logsToolStripMenuItem, downloadLastestReleaseFromGitHubToolStripMenuItem, changelogToolStripMenuItem, exportDataToolStripMenuItem, aboutToolStripMenuItem, exitToolStripMenuItem });
            menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            menuToolStripMenuItem.Size = new Size(50, 20);
            menuToolStripMenuItem.Text = "Menu";
            // 
            // onlineToolStripMenuItem
            // 
            onlineToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { myWebpageToolStripMenuItem, myBlogToolStripMenuItem, guideToolStripMenuItem });
            onlineToolStripMenuItem.Name = "onlineToolStripMenuItem";
            onlineToolStripMenuItem.Size = new Size(273, 22);
            onlineToolStripMenuItem.Text = "Online";
            // 
            // myWebpageToolStripMenuItem
            // 
            myWebpageToolStripMenuItem.Name = "myWebpageToolStripMenuItem";
            myWebpageToolStripMenuItem.Size = new Size(142, 22);
            myWebpageToolStripMenuItem.Text = "My webpage";
            // 
            // myBlogToolStripMenuItem
            // 
            myBlogToolStripMenuItem.Name = "myBlogToolStripMenuItem";
            myBlogToolStripMenuItem.Size = new Size(142, 22);
            myBlogToolStripMenuItem.Text = "My blog";
            // 
            // guideToolStripMenuItem
            // 
            guideToolStripMenuItem.Name = "guideToolStripMenuItem";
            guideToolStripMenuItem.Size = new Size(142, 22);
            guideToolStripMenuItem.Text = "Guide";
            // 
            // logsToolStripMenuItem
            // 
            logsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openLogFolderToolStripMenuItem, openLogForTodayToolStripMenuItem });
            logsToolStripMenuItem.Name = "logsToolStripMenuItem";
            logsToolStripMenuItem.Size = new Size(273, 22);
            logsToolStripMenuItem.Text = "Logs";
            // 
            // openLogFolderToolStripMenuItem
            // 
            openLogFolderToolStripMenuItem.Name = "openLogFolderToolStripMenuItem";
            openLogFolderToolStripMenuItem.Size = new Size(174, 22);
            openLogFolderToolStripMenuItem.Text = "Open log folder";
            // 
            // openLogForTodayToolStripMenuItem
            // 
            openLogForTodayToolStripMenuItem.Name = "openLogForTodayToolStripMenuItem";
            openLogForTodayToolStripMenuItem.Size = new Size(174, 22);
            openLogForTodayToolStripMenuItem.Text = "Open log for today";
            // 
            // downloadLastestReleaseFromGitHubToolStripMenuItem
            // 
            downloadLastestReleaseFromGitHubToolStripMenuItem.Name = "downloadLastestReleaseFromGitHubToolStripMenuItem";
            downloadLastestReleaseFromGitHubToolStripMenuItem.Size = new Size(273, 22);
            downloadLastestReleaseFromGitHubToolStripMenuItem.Text = "Download lastest release from GitHub";
            // 
            // changelogToolStripMenuItem
            // 
            changelogToolStripMenuItem.Name = "changelogToolStripMenuItem";
            changelogToolStripMenuItem.Size = new Size(273, 22);
            changelogToolStripMenuItem.Text = "Changelog";
            // 
            // exportDataToolStripMenuItem
            // 
            exportDataToolStripMenuItem.Name = "exportDataToolStripMenuItem";
            exportDataToolStripMenuItem.Size = new Size(273, 22);
            exportDataToolStripMenuItem.Text = "Export data";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(273, 22);
            aboutToolStripMenuItem.Text = "About";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(273, 22);
            exitToolStripMenuItem.Text = "Exit";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1645, 999);
            Controls.Add(tabcontrolMainForm);
            Controls.Add(menuStripTopMainForm);
            MainMenuStrip = menuStripTopMainForm;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            ((System.ComponentModel.ISupportInitialize)datagridviewVMOverView).EndInit();
            tabcontrolMainForm.ResumeLayout(false);
            tabpagehvOverview.ResumeLayout(false);
            menuStripTopMainForm.ResumeLayout(false);
            menuStripTopMainForm.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView datagridviewVMOverView;
        private TabControl tabcontrolMainForm;
        private TabPage tabpagehvOverview;
        private TabPage tabpageVMGroups;
        private MenuStrip menuStripTopMainForm;
        private ToolStripMenuItem menuToolStripMenuItem;
        private ToolStripMenuItem onlineToolStripMenuItem;
        private ToolStripMenuItem myWebpageToolStripMenuItem;
        private ToolStripMenuItem myBlogToolStripMenuItem;
        private ToolStripMenuItem guideToolStripMenuItem;
        private ToolStripMenuItem logsToolStripMenuItem;
        private ToolStripMenuItem openLogFolderToolStripMenuItem;
        private ToolStripMenuItem openLogForTodayToolStripMenuItem;
        private ToolStripMenuItem downloadLastestReleaseFromGitHubToolStripMenuItem;
        private ToolStripMenuItem changelogToolStripMenuItem;
        private ToolStripMenuItem exportDataToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private TabPage tabpageManageNetwork;
        private TabPage tabpagehvHosts;
        private TabPage tabpagehvClusters;
        private TabPage tabpagehvStorage;
        private TabPage tabpagehvNetworking;
        private TabPage tabpagehvCheckpoints;
        private TabPage tabpagehvReplica;
        private TabPage tabpagehvResources;
        private TabPage tabpagehvSecurity;
        private TabPage tabpagehvPerformance;
        private TabPage tabpagehvCompliance;
        private TabPage tabpagehvInventory;
        private TabPage tabpageCreateVM;
        private TabPage tabpageHealthOverview;
    }
}
