namespace HyperView.Forms
{
    partial class ManageVMGroupMembers
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
            labelThisViewProvidesOver = new Label();
            labelAvailable = new Label();
            labelMembers = new Label();
            listboxAvailable = new ListBox();
            listboxMembers = new ListBox();
            buttonAdd = new Button();
            buttonClose = new Button();
            buttonRemove = new Button();
            SuspendLayout();
            // 
            // labelThisViewProvidesOver
            // 
            labelThisViewProvidesOver.AutoSize = true;
            labelThisViewProvidesOver.Location = new Point(12, 9);
            labelThisViewProvidesOver.Name = "labelThisViewProvidesOver";
            labelThisViewProvidesOver.Size = new Size(700, 30);
            labelThisViewProvidesOver.TabIndex = 0;
            labelThisViewProvidesOver.Text = "This view provides overview and functionality within Hyper-V for management of VM Group membership for VMs that is tagged for\r\nVM Groups. You can add and remove members.";
            // 
            // labelAvailable
            // 
            labelAvailable.AutoSize = true;
            labelAvailable.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelAvailable.Location = new Point(149, 45);
            labelAvailable.Name = "labelAvailable";
            labelAvailable.Size = new Size(87, 15);
            labelAvailable.TabIndex = 1;
            labelAvailable.Text = "Available VMs:";
            // 
            // labelMembers
            // 
            labelMembers.AutoSize = true;
            labelMembers.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelMembers.Location = new Point(507, 45);
            labelMembers.Name = "labelMembers";
            labelMembers.Size = new Size(101, 15);
            labelMembers.TabIndex = 2;
            labelMembers.Text = "Group Members:";
            // 
            // listboxAvailable
            // 
            listboxAvailable.FormattingEnabled = true;
            listboxAvailable.Location = new Point(12, 67);
            listboxAvailable.Name = "listboxAvailable";
            listboxAvailable.Size = new Size(357, 304);
            listboxAvailable.TabIndex = 3;
            // 
            // listboxMembers
            // 
            listboxMembers.FormattingEnabled = true;
            listboxMembers.Location = new Point(375, 67);
            listboxMembers.Name = "listboxMembers";
            listboxMembers.Size = new Size(357, 304);
            listboxMembers.TabIndex = 4;
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(161, 377);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(75, 23);
            buttonAdd.TabIndex = 5;
            buttonAdd.Text = "Add >";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += ButtonAdd_Click;
            // 
            // buttonClose
            // 
            buttonClose.Location = new Point(333, 377);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(75, 23);
            buttonClose.TabIndex = 6;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += ButtonClose_Click;
            // 
            // buttonRemove
            // 
            buttonRemove.Location = new Point(507, 377);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new Size(75, 23);
            buttonRemove.TabIndex = 7;
            buttonRemove.Text = "< Remove";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += ButtonRemove_Click;
            // 
            // ManageVMGroupMembers
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(745, 410);
            Controls.Add(buttonRemove);
            Controls.Add(buttonClose);
            Controls.Add(buttonAdd);
            Controls.Add(listboxMembers);
            Controls.Add(listboxAvailable);
            Controls.Add(labelMembers);
            Controls.Add(labelAvailable);
            Controls.Add(labelThisViewProvidesOver);
            Name = "ManageVMGroupMembers";
            StartPosition = FormStartPosition.CenterParent;
            Text = "ManageVMGroupMembers";
            Load += ManageVMGroupMembers_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelThisViewProvidesOver;
        private Label labelAvailable;
        private Label labelMembers;
        private ListBox listboxAvailable;
        private ListBox listboxMembers;
        private Button buttonAdd;
        private Button buttonClose;
        private Button buttonRemove;
    }
}