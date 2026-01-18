namespace HyperView.Forms
{
    partial class CreateVMGroupForm
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
            buttonOK = new Button();
            buttonCancel = new Button();
            buttonRecommended = new Button();
            labelGroupName = new Label();
            labelGroupType = new Label();
            textboxGroupName = new TextBox();
            comboboxGroupType = new ComboBox();
            groupboxRecommended = new GroupBox();
            treeviewTemplates = new TreeView();
            labelSelectARecommendedVM = new Label();
            labelSelected = new Label();
            textboxDescription = new TextBox();
            buttonUseSelectedTemplate = new Button();
            groupboxRecommended.SuspendLayout();
            SuspendLayout();
            // 
            // buttonOK
            // 
            buttonOK.Location = new Point(411, 12);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 23);
            buttonOK.TabIndex = 0;
            buttonOK.Text = "Create VM Group";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(330, 12);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonRecommended
            // 
            buttonRecommended.Location = new Point(330, 42);
            buttonRecommended.Name = "buttonRecommended";
            buttonRecommended.Size = new Size(124, 23);
            buttonRecommended.TabIndex = 2;
            buttonRecommended.Text = "Templates...";
            buttonRecommended.UseVisualStyleBackColor = true;
            // 
            // labelGroupName
            // 
            labelGroupName.AutoSize = true;
            labelGroupName.Location = new Point(12, 16);
            labelGroupName.Name = "labelGroupName";
            labelGroupName.Size = new Size(78, 15);
            labelGroupName.TabIndex = 3;
            labelGroupName.Text = "Group Name:";
            // 
            // labelGroupType
            // 
            labelGroupType.AutoSize = true;
            labelGroupType.Location = new Point(12, 45);
            labelGroupType.Name = "labelGroupType";
            labelGroupType.Size = new Size(71, 15);
            labelGroupType.TabIndex = 4;
            labelGroupType.Text = "Group Type:";
            // 
            // textboxGroupName
            // 
            textboxGroupName.Location = new Point(96, 13);
            textboxGroupName.Name = "textboxGroupName";
            textboxGroupName.Size = new Size(228, 23);
            textboxGroupName.TabIndex = 5;
            // 
            // comboboxGroupType
            // 
            comboboxGroupType.FormattingEnabled = true;
            comboboxGroupType.Location = new Point(96, 42);
            comboboxGroupType.Name = "comboboxGroupType";
            comboboxGroupType.Size = new Size(228, 23);
            comboboxGroupType.TabIndex = 6;
            // 
            // groupboxRecommended
            // 
            groupboxRecommended.Controls.Add(buttonUseSelectedTemplate);
            groupboxRecommended.Controls.Add(textboxDescription);
            groupboxRecommended.Controls.Add(labelSelected);
            groupboxRecommended.Controls.Add(labelSelectARecommendedVM);
            groupboxRecommended.Controls.Add(treeviewTemplates);
            groupboxRecommended.Location = new Point(12, 71);
            groupboxRecommended.Name = "groupboxRecommended";
            groupboxRecommended.Size = new Size(474, 289);
            groupboxRecommended.TabIndex = 7;
            groupboxRecommended.TabStop = false;
            groupboxRecommended.Text = "Recommended VM Groups";
            // 
            // treeviewTemplates
            // 
            treeviewTemplates.Location = new Point(6, 42);
            treeviewTemplates.Name = "treeviewTemplates";
            treeviewTemplates.Size = new Size(270, 241);
            treeviewTemplates.TabIndex = 0;
            // 
            // labelSelectARecommendedVM
            // 
            labelSelectARecommendedVM.AutoSize = true;
            labelSelectARecommendedVM.Location = new Point(6, 19);
            labelSelectARecommendedVM.Name = "labelSelectARecommendedVM";
            labelSelectARecommendedVM.Size = new Size(238, 15);
            labelSelectARecommendedVM.TabIndex = 1;
            labelSelectARecommendedVM.Text = "Select a recommended VM Group template:";
            // 
            // labelSelected
            // 
            labelSelected.AutoSize = true;
            labelSelected.Location = new Point(287, 19);
            labelSelected.Name = "labelSelected";
            labelSelected.Size = new Size(87, 15);
            labelSelected.TabIndex = 2;
            labelSelected.Text = "Selected Group";
            // 
            // textboxDescription
            // 
            textboxDescription.Location = new Point(282, 42);
            textboxDescription.Multiline = true;
            textboxDescription.Name = "textboxDescription";
            textboxDescription.ReadOnly = true;
            textboxDescription.Size = new Size(186, 215);
            textboxDescription.TabIndex = 3;
            // 
            // buttonUseSelectedTemplate
            // 
            buttonUseSelectedTemplate.Location = new Point(282, 260);
            buttonUseSelectedTemplate.Name = "buttonUseSelectedTemplate";
            buttonUseSelectedTemplate.Size = new Size(186, 23);
            buttonUseSelectedTemplate.TabIndex = 4;
            buttonUseSelectedTemplate.Text = "Use selected Template";
            buttonUseSelectedTemplate.UseVisualStyleBackColor = true;
            // 
            // CreateVMGroupForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(498, 372);
            Controls.Add(groupboxRecommended);
            Controls.Add(comboboxGroupType);
            Controls.Add(textboxGroupName);
            Controls.Add(labelGroupType);
            Controls.Add(labelGroupName);
            Controls.Add(buttonRecommended);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Name = "CreateVMGroupForm";
            Text = "CreateVMGroupForm";
            groupboxRecommended.ResumeLayout(false);
            groupboxRecommended.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonRecommended;
        private Label labelGroupName;
        private Label labelGroupType;
        private TextBox textboxGroupName;
        private ComboBox comboboxGroupType;
        private GroupBox groupboxRecommended;
        private TextBox textboxDescription;
        private Label labelSelected;
        private Label labelSelectARecommendedVM;
        private TreeView treeviewTemplates;
        private Button buttonUseSelectedTemplate;
    }
}