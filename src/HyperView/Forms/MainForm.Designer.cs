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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            datagridviewVMOverView = new DataGridView();
            tabcontrolMainForm = new TabControl();
            tabpagehvOverview = new TabPage();
            buttonSummaryhvOverviewView = new Button();
            buttonLoadVMsrefresh = new Button();
            labelOverviewHelpText = new Label();
            tabpageVMGroups = new TabPage();
            groupBox2 = new GroupBox();
            buttonManageServerMembers = new Button();
            groupBox1 = new GroupBox();
            buttonRenameSelectedVMGrou = new Button();
            buttonDeleteSelectedVMGrou = new Button();
            buttonCreateANewVMGroup = new Button();
            buttonLoadGroupsrefresh = new Button();
            labelThisViewProvidesOver = new Label();
            datagridviewVMGroups = new DataGridView();
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
            disconnectToolStripMenuItem = new ToolStripMenuItem();
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
            allVMDataToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            pictureboxSupportMe = new PictureBox();
            statusStripMainForm = new StatusStrip();
            toolStripStatusLabelMainForm = new ToolStripStatusLabel();
            toolStripStatusLabelTextMainForm = new ToolStripStatusLabel();
            groupBoxMainFormServerDetails = new GroupBox();
            toolstripstatuslabelMain_CreatedBy = new Label();
            ((System.ComponentModel.ISupportInitialize)datagridviewVMOverView).BeginInit();
            tabcontrolMainForm.SuspendLayout();
            tabpagehvOverview.SuspendLayout();
            tabpageVMGroups.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datagridviewVMGroups).BeginInit();
            menuStripTopMainForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureboxSupportMe).BeginInit();
            statusStripMainForm.SuspendLayout();
            SuspendLayout();
            // 
            // datagridviewVMOverView
            // 
            datagridviewVMOverView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewVMOverView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewVMOverView.Location = new Point(6, 35);
            datagridviewVMOverView.Name = "datagridviewVMOverView";
            datagridviewVMOverView.Size = new Size(1601, 803);
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
            tabpagehvOverview.Controls.Add(buttonSummaryhvOverviewView);
            tabpagehvOverview.Controls.Add(buttonLoadVMsrefresh);
            tabpagehvOverview.Controls.Add(labelOverviewHelpText);
            tabpagehvOverview.Controls.Add(datagridviewVMOverView);
            tabpagehvOverview.Location = new Point(4, 24);
            tabpagehvOverview.Name = "tabpagehvOverview";
            tabpagehvOverview.Padding = new Padding(3);
            tabpagehvOverview.Size = new Size(1613, 844);
            tabpagehvOverview.TabIndex = 0;
            tabpagehvOverview.Text = "hvOverview";
            tabpagehvOverview.UseVisualStyleBackColor = true;
            // 
            // buttonSummaryhvOverviewView
            // 
            buttonSummaryhvOverviewView.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSummaryhvOverviewView.Location = new Point(1416, 6);
            buttonSummaryhvOverviewView.Name = "buttonSummaryhvOverviewView";
            buttonSummaryhvOverviewView.Size = new Size(75, 23);
            buttonSummaryhvOverviewView.TabIndex = 3;
            buttonSummaryhvOverviewView.Text = "Summary";
            buttonSummaryhvOverviewView.UseVisualStyleBackColor = true;
            // 
            // buttonLoadVMsrefresh
            // 
            buttonLoadVMsrefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonLoadVMsrefresh.Location = new Point(1497, 6);
            buttonLoadVMsrefresh.Name = "buttonLoadVMsrefresh";
            buttonLoadVMsrefresh.Size = new Size(110, 23);
            buttonLoadVMsrefresh.TabIndex = 2;
            buttonLoadVMsrefresh.Text = "&Load VMs/refresh";
            buttonLoadVMsrefresh.UseVisualStyleBackColor = true;
            // 
            // labelOverviewHelpText
            // 
            labelOverviewHelpText.AutoSize = true;
            labelOverviewHelpText.Location = new Point(6, 3);
            labelOverviewHelpText.Name = "labelOverviewHelpText";
            labelOverviewHelpText.Size = new Size(705, 30);
            labelOverviewHelpText.TabIndex = 1;
            labelOverviewHelpText.Text = "This view provides provides overview and core functionality within the Hyper-V space for information about VMs and other data that\r\nextends that functionality over multiple servers.";
            // 
            // tabpageVMGroups
            // 
            tabpageVMGroups.Controls.Add(groupBox2);
            tabpageVMGroups.Controls.Add(groupBox1);
            tabpageVMGroups.Controls.Add(buttonLoadGroupsrefresh);
            tabpageVMGroups.Controls.Add(labelThisViewProvidesOver);
            tabpageVMGroups.Controls.Add(datagridviewVMGroups);
            tabpageVMGroups.Location = new Point(4, 24);
            tabpageVMGroups.Name = "tabpageVMGroups";
            tabpageVMGroups.Padding = new Padding(3);
            tabpageVMGroups.Size = new Size(1613, 844);
            tabpageVMGroups.TabIndex = 1;
            tabpageVMGroups.Text = "Manage VM Groups";
            tabpageVMGroups.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(buttonManageServerMembers);
            groupBox2.Location = new Point(1407, 211);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(200, 73);
            groupBox2.TabIndex = 5;
            groupBox2.TabStop = false;
            groupBox2.Text = "Manage VM Group";
            // 
            // buttonManageServerMembers
            // 
            buttonManageServerMembers.Location = new Point(6, 22);
            buttonManageServerMembers.Name = "buttonManageServerMembers";
            buttonManageServerMembers.Size = new Size(188, 43);
            buttonManageServerMembers.TabIndex = 2;
            buttonManageServerMembers.Text = "Manage server members";
            buttonManageServerMembers.UseVisualStyleBackColor = true;
            buttonManageServerMembers.Click += buttonManageServerMembers_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(buttonRenameSelectedVMGrou);
            groupBox1.Controls.Add(buttonDeleteSelectedVMGrou);
            groupBox1.Controls.Add(buttonCreateANewVMGroup);
            groupBox1.Location = new Point(1407, 35);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(200, 170);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Group management";
            // 
            // buttonRenameSelectedVMGrou
            // 
            buttonRenameSelectedVMGrou.Location = new Point(6, 120);
            buttonRenameSelectedVMGrou.Name = "buttonRenameSelectedVMGrou";
            buttonRenameSelectedVMGrou.Size = new Size(188, 43);
            buttonRenameSelectedVMGrou.TabIndex = 2;
            buttonRenameSelectedVMGrou.Text = "Rename selected VM group";
            buttonRenameSelectedVMGrou.UseVisualStyleBackColor = true;
            buttonRenameSelectedVMGrou.Click += buttonRenameSelectedVMGrou_Click;
            // 
            // buttonDeleteSelectedVMGrou
            // 
            buttonDeleteSelectedVMGrou.BackColor = Color.LightCoral;
            buttonDeleteSelectedVMGrou.Location = new Point(6, 71);
            buttonDeleteSelectedVMGrou.Name = "buttonDeleteSelectedVMGrou";
            buttonDeleteSelectedVMGrou.Size = new Size(188, 43);
            buttonDeleteSelectedVMGrou.TabIndex = 1;
            buttonDeleteSelectedVMGrou.Text = "Delete selected VM group";
            buttonDeleteSelectedVMGrou.UseVisualStyleBackColor = false;
            buttonDeleteSelectedVMGrou.Click += buttonDeleteSelectedVMGrou_Click;
            // 
            // buttonCreateANewVMGroup
            // 
            buttonCreateANewVMGroup.BackColor = Color.LightGreen;
            buttonCreateANewVMGroup.Location = new Point(6, 22);
            buttonCreateANewVMGroup.Name = "buttonCreateANewVMGroup";
            buttonCreateANewVMGroup.Size = new Size(188, 43);
            buttonCreateANewVMGroup.TabIndex = 0;
            buttonCreateANewVMGroup.Text = "Create a new VM Group";
            buttonCreateANewVMGroup.UseVisualStyleBackColor = false;
            buttonCreateANewVMGroup.Click += buttonCreateANewVMGroup_Click;
            // 
            // buttonLoadGroupsrefresh
            // 
            buttonLoadGroupsrefresh.Location = new Point(1475, 6);
            buttonLoadGroupsrefresh.Name = "buttonLoadGroupsrefresh";
            buttonLoadGroupsrefresh.Size = new Size(132, 23);
            buttonLoadGroupsrefresh.TabIndex = 3;
            buttonLoadGroupsrefresh.Text = "&Load Groups/refresh";
            buttonLoadGroupsrefresh.UseVisualStyleBackColor = true;
            buttonLoadGroupsrefresh.Click += buttonLoadGroupsrefresh_Click;
            // 
            // labelThisViewProvidesOver
            // 
            labelThisViewProvidesOver.AutoSize = true;
            labelThisViewProvidesOver.Location = new Point(6, 3);
            labelThisViewProvidesOver.Name = "labelThisViewProvidesOver";
            labelThisViewProvidesOver.Size = new Size(712, 30);
            labelThisViewProvidesOver.TabIndex = 2;
            labelThisViewProvidesOver.Text = "This view provides overview and core functionality within the Hyper-V space for management of VM Groups for VMs that extends that\r\nfunctionality over multiple servers.";
            // 
            // datagridviewVMGroups
            // 
            datagridviewVMGroups.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewVMGroups.Location = new Point(6, 35);
            datagridviewVMGroups.Name = "datagridviewVMGroups";
            datagridviewVMGroups.Size = new Size(1395, 803);
            datagridviewVMGroups.TabIndex = 0;
            // 
            // tabpageManageNetwork
            // 
            tabpageManageNetwork.Location = new Point(4, 24);
            tabpageManageNetwork.Name = "tabpageManageNetwork";
            tabpageManageNetwork.Size = new Size(1613, 844);
            tabpageManageNetwork.TabIndex = 2;
            tabpageManageNetwork.Text = "Manage VM Networks";
            tabpageManageNetwork.UseVisualStyleBackColor = true;
            // 
            // tabpagehvHosts
            // 
            tabpagehvHosts.Location = new Point(4, 24);
            tabpagehvHosts.Name = "tabpagehvHosts";
            tabpagehvHosts.Size = new Size(1613, 844);
            tabpagehvHosts.TabIndex = 3;
            tabpagehvHosts.Text = "hvHosts";
            tabpagehvHosts.UseVisualStyleBackColor = true;
            // 
            // tabpagehvClusters
            // 
            tabpagehvClusters.Location = new Point(4, 24);
            tabpagehvClusters.Name = "tabpagehvClusters";
            tabpagehvClusters.Size = new Size(1613, 844);
            tabpagehvClusters.TabIndex = 4;
            tabpagehvClusters.Text = "hvClusters";
            tabpagehvClusters.UseVisualStyleBackColor = true;
            // 
            // tabpagehvStorage
            // 
            tabpagehvStorage.Location = new Point(4, 24);
            tabpagehvStorage.Name = "tabpagehvStorage";
            tabpagehvStorage.Size = new Size(1613, 844);
            tabpagehvStorage.TabIndex = 5;
            tabpagehvStorage.Text = "hvStorage";
            tabpagehvStorage.UseVisualStyleBackColor = true;
            // 
            // tabpagehvNetworking
            // 
            tabpagehvNetworking.Location = new Point(4, 24);
            tabpagehvNetworking.Name = "tabpagehvNetworking";
            tabpagehvNetworking.Size = new Size(1613, 844);
            tabpagehvNetworking.TabIndex = 6;
            tabpagehvNetworking.Text = "hvNetworking";
            tabpagehvNetworking.UseVisualStyleBackColor = true;
            // 
            // tabpagehvCheckpoints
            // 
            tabpagehvCheckpoints.Location = new Point(4, 24);
            tabpagehvCheckpoints.Name = "tabpagehvCheckpoints";
            tabpagehvCheckpoints.Size = new Size(1613, 844);
            tabpagehvCheckpoints.TabIndex = 7;
            tabpagehvCheckpoints.Text = "hvCheckpoints";
            tabpagehvCheckpoints.UseVisualStyleBackColor = true;
            // 
            // tabpagehvReplica
            // 
            tabpagehvReplica.Location = new Point(4, 24);
            tabpagehvReplica.Name = "tabpagehvReplica";
            tabpagehvReplica.Size = new Size(1613, 844);
            tabpagehvReplica.TabIndex = 8;
            tabpagehvReplica.Text = "hvReplica";
            tabpagehvReplica.UseVisualStyleBackColor = true;
            // 
            // tabpagehvResources
            // 
            tabpagehvResources.Location = new Point(4, 24);
            tabpagehvResources.Name = "tabpagehvResources";
            tabpagehvResources.Size = new Size(1613, 844);
            tabpagehvResources.TabIndex = 9;
            tabpagehvResources.Text = "hvResources";
            tabpagehvResources.UseVisualStyleBackColor = true;
            // 
            // tabpagehvSecurity
            // 
            tabpagehvSecurity.Location = new Point(4, 24);
            tabpagehvSecurity.Name = "tabpagehvSecurity";
            tabpagehvSecurity.Size = new Size(1613, 844);
            tabpagehvSecurity.TabIndex = 10;
            tabpagehvSecurity.Text = "hvSecurity";
            tabpagehvSecurity.UseVisualStyleBackColor = true;
            // 
            // tabpagehvPerformance
            // 
            tabpagehvPerformance.Location = new Point(4, 24);
            tabpagehvPerformance.Name = "tabpagehvPerformance";
            tabpagehvPerformance.Size = new Size(1613, 844);
            tabpagehvPerformance.TabIndex = 11;
            tabpagehvPerformance.Text = "hvPerformance";
            tabpagehvPerformance.UseVisualStyleBackColor = true;
            // 
            // tabpagehvCompliance
            // 
            tabpagehvCompliance.Location = new Point(4, 24);
            tabpagehvCompliance.Name = "tabpagehvCompliance";
            tabpagehvCompliance.Size = new Size(1613, 844);
            tabpagehvCompliance.TabIndex = 12;
            tabpagehvCompliance.Text = "hvCompliance";
            tabpagehvCompliance.UseVisualStyleBackColor = true;
            // 
            // tabpagehvInventory
            // 
            tabpagehvInventory.Location = new Point(4, 24);
            tabpagehvInventory.Name = "tabpagehvInventory";
            tabpagehvInventory.Size = new Size(1613, 844);
            tabpagehvInventory.TabIndex = 13;
            tabpagehvInventory.Text = "hvInventory";
            tabpagehvInventory.UseVisualStyleBackColor = true;
            // 
            // tabpageCreateVM
            // 
            tabpageCreateVM.Location = new Point(4, 24);
            tabpageCreateVM.Name = "tabpageCreateVM";
            tabpageCreateVM.Size = new Size(1613, 844);
            tabpageCreateVM.TabIndex = 14;
            tabpageCreateVM.Text = "Create VM´s";
            tabpageCreateVM.UseVisualStyleBackColor = true;
            // 
            // tabpageHealthOverview
            // 
            tabpageHealthOverview.Location = new Point(4, 24);
            tabpageHealthOverview.Name = "tabpageHealthOverview";
            tabpageHealthOverview.Size = new Size(1613, 844);
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
            menuToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { disconnectToolStripMenuItem, onlineToolStripMenuItem, logsToolStripMenuItem, downloadLastestReleaseFromGitHubToolStripMenuItem, changelogToolStripMenuItem, exportDataToolStripMenuItem, aboutToolStripMenuItem, exitToolStripMenuItem });
            menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            menuToolStripMenuItem.Size = new Size(50, 20);
            menuToolStripMenuItem.Text = "Menu";
            // 
            // disconnectToolStripMenuItem
            // 
            disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            disconnectToolStripMenuItem.Size = new Size(273, 22);
            disconnectToolStripMenuItem.Text = "Disconnect";
            disconnectToolStripMenuItem.Click += disconnectToolStripMenuItem_Click;
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
            myWebpageToolStripMenuItem.Size = new Size(180, 22);
            myWebpageToolStripMenuItem.Text = "My webpage";
            myWebpageToolStripMenuItem.Click += myWebpageToolStripMenuItem_Click;
            // 
            // myBlogToolStripMenuItem
            // 
            myBlogToolStripMenuItem.Name = "myBlogToolStripMenuItem";
            myBlogToolStripMenuItem.Size = new Size(180, 22);
            myBlogToolStripMenuItem.Text = "My blog";
            myBlogToolStripMenuItem.Click += myBlogToolStripMenuItem_Click;
            // 
            // guideToolStripMenuItem
            // 
            guideToolStripMenuItem.Name = "guideToolStripMenuItem";
            guideToolStripMenuItem.Size = new Size(180, 22);
            guideToolStripMenuItem.Text = "Guide";
            guideToolStripMenuItem.Click += guideToolStripMenuItem_Click;
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
            openLogFolderToolStripMenuItem.Size = new Size(180, 22);
            openLogFolderToolStripMenuItem.Text = "Open log folder";
            openLogFolderToolStripMenuItem.Click += openLogFolderToolStripMenuItem_Click;
            // 
            // openLogForTodayToolStripMenuItem
            // 
            openLogForTodayToolStripMenuItem.Name = "openLogForTodayToolStripMenuItem";
            openLogForTodayToolStripMenuItem.Size = new Size(180, 22);
            openLogForTodayToolStripMenuItem.Text = "Open log for today";
            openLogForTodayToolStripMenuItem.Click += openLogForTodayToolStripMenuItem_Click;
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
            changelogToolStripMenuItem.Click += changelogToolStripMenuItem_Click;
            // 
            // exportDataToolStripMenuItem
            // 
            exportDataToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { allVMDataToolStripMenuItem });
            exportDataToolStripMenuItem.Name = "exportDataToolStripMenuItem";
            exportDataToolStripMenuItem.Size = new Size(273, 22);
            exportDataToolStripMenuItem.Text = "Export data";
            // 
            // allVMDataToolStripMenuItem
            // 
            allVMDataToolStripMenuItem.Name = "allVMDataToolStripMenuItem";
            allVMDataToolStripMenuItem.Size = new Size(135, 22);
            allVMDataToolStripMenuItem.Text = "All VM data";
            allVMDataToolStripMenuItem.Click += allVMDataToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(273, 22);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(273, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // pictureboxSupportMe
            // 
            pictureboxSupportMe.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictureboxSupportMe.BackColor = Color.Transparent;
            pictureboxSupportMe.Image = (Image)resources.GetObject("pictureboxSupportMe.Image");
            pictureboxSupportMe.Location = new Point(1520, 5);
            pictureboxSupportMe.Name = "pictureboxSupportMe";
            pictureboxSupportMe.Size = new Size(119, 36);
            pictureboxSupportMe.SizeMode = PictureBoxSizeMode.Zoom;
            pictureboxSupportMe.TabIndex = 1;
            pictureboxSupportMe.TabStop = false;
            // 
            // statusStripMainForm
            // 
            statusStripMainForm.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelMainForm, toolStripStatusLabelTextMainForm });
            statusStripMainForm.Location = new Point(0, 977);
            statusStripMainForm.Name = "statusStripMainForm";
            statusStripMainForm.Size = new Size(1645, 22);
            statusStripMainForm.SizingGrip = false;
            statusStripMainForm.TabIndex = 3;
            statusStripMainForm.Text = "statusStrip1";
            // 
            // toolStripStatusLabelMainForm
            // 
            toolStripStatusLabelMainForm.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            toolStripStatusLabelMainForm.Name = "toolStripStatusLabelMainForm";
            toolStripStatusLabelMainForm.Size = new Size(45, 17);
            toolStripStatusLabelMainForm.Text = "Status:";
            // 
            // toolStripStatusLabelTextMainForm
            // 
            toolStripStatusLabelTextMainForm.Name = "toolStripStatusLabelTextMainForm";
            toolStripStatusLabelTextMainForm.Size = new Size(67, 17);
            toolStripStatusLabelTextMainForm.Text = "%STATUS%";
            // 
            // groupBoxMainFormServerDetails
            // 
            groupBoxMainFormServerDetails.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            groupBoxMainFormServerDetails.Location = new Point(12, 905);
            groupBoxMainFormServerDetails.Name = "groupBoxMainFormServerDetails";
            groupBoxMainFormServerDetails.Size = new Size(449, 58);
            groupBoxMainFormServerDetails.TabIndex = 4;
            groupBoxMainFormServerDetails.TabStop = false;
            groupBoxMainFormServerDetails.Text = "Details";
            // 
            // toolstripstatuslabelMain_CreatedBy
            // 
            toolstripstatuslabelMain_CreatedBy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            toolstripstatuslabelMain_CreatedBy.AutoSize = true;
            toolstripstatuslabelMain_CreatedBy.BackColor = Color.White;
            toolstripstatuslabelMain_CreatedBy.Location = new Point(1327, 4);
            toolstripstatuslabelMain_CreatedBy.Name = "toolstripstatuslabelMain_CreatedBy";
            toolstripstatuslabelMain_CreatedBy.Size = new Size(190, 15);
            toolstripstatuslabelMain_CreatedBy.TabIndex = 4;
            toolstripstatuslabelMain_CreatedBy.Text = "Created by: Michael Morten Sonne";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1645, 999);
            Controls.Add(toolstripstatuslabelMain_CreatedBy);
            Controls.Add(groupBoxMainFormServerDetails);
            Controls.Add(statusStripMainForm);
            Controls.Add(pictureboxSupportMe);
            Controls.Add(menuStripTopMainForm);
            Controls.Add(tabcontrolMainForm);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripTopMainForm;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            FormClosing += MainForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)datagridviewVMOverView).EndInit();
            tabcontrolMainForm.ResumeLayout(false);
            tabpagehvOverview.ResumeLayout(false);
            tabpagehvOverview.PerformLayout();
            tabpageVMGroups.ResumeLayout(false);
            tabpageVMGroups.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)datagridviewVMGroups).EndInit();
            menuStripTopMainForm.ResumeLayout(false);
            menuStripTopMainForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureboxSupportMe).EndInit();
            statusStripMainForm.ResumeLayout(false);
            statusStripMainForm.PerformLayout();
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
        private PictureBox pictureboxSupportMe;
        private StatusStrip statusStripMainForm;
        private ToolStripStatusLabel toolStripStatusLabelMainForm;
        private ToolStripStatusLabel toolStripStatusLabelTextMainForm;
        private GroupBox groupBoxMainFormServerDetails;
        private Label labelOverviewHelpText;
        private Button buttonSummaryhvOverviewView;
        private Button buttonLoadVMsrefresh;
        private ToolStripMenuItem disconnectToolStripMenuItem;
        private Label labelThisViewProvidesOver;
        private DataGridView datagridviewVMGroups;
        private GroupBox groupBox1;
        private Button buttonRenameSelectedVMGrou;
        private Button buttonDeleteSelectedVMGrou;
        private Button buttonCreateANewVMGroup;
        private Button buttonLoadGroupsrefresh;
        private GroupBox groupBox2;
        private Button buttonManageServerMembers;
        private Label toolstripstatuslabelMain_CreatedBy;
        private ToolStripMenuItem allVMDataToolStripMenuItem;
    }
}
