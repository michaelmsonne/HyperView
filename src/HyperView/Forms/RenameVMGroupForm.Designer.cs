namespace HyperView.Forms
{
    partial class RenameVMGroupForm
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
            labelCurrentName = new Label();
            labelCurrentNameValue = new TextBox();
            labelNewName = new Label();
            textboxNewName = new TextBox();
            buttonOK = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // labelCurrentName
            // 
            labelCurrentName.AutoSize = true;
            labelCurrentName.Location = new Point(12, 9);
            labelCurrentName.Name = "labelCurrentName";
            labelCurrentName.Size = new Size(83, 15);
            labelCurrentName.TabIndex = 0;
            labelCurrentName.Text = "Current name:";
            // 
            // labelCurrentNameValue
            // 
            labelCurrentNameValue.Location = new Point(101, 6);
            labelCurrentNameValue.Name = "labelCurrentNameValue";
            labelCurrentNameValue.ReadOnly = true;
            labelCurrentNameValue.Size = new Size(271, 23);
            labelCurrentNameValue.TabIndex = 1;
            // 
            // labelNewName
            // 
            labelNewName.AutoSize = true;
            labelNewName.Location = new Point(12, 38);
            labelNewName.Name = "labelNewName";
            labelNewName.Size = new Size(67, 15);
            labelNewName.TabIndex = 2;
            labelNewName.Text = "New name:";
            // 
            // textboxNewName
            // 
            textboxNewName.Location = new Point(101, 35);
            textboxNewName.Name = "textboxNewName";
            textboxNewName.Size = new Size(271, 23);
            textboxNewName.TabIndex = 3;
            // 
            // buttonOK
            // 
            buttonOK.Location = new Point(196, 64);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 23);
            buttonOK.TabIndex = 4;
            buttonOK.Text = "Rename";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(115, 64);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // RenameVMGroupForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 97);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(textboxNewName);
            Controls.Add(labelNewName);
            Controls.Add(labelCurrentNameValue);
            Controls.Add(labelCurrentName);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RenameVMGroupForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "RenameVMGroupForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelCurrentName;
        private TextBox labelCurrentNameValue;
        private Label labelNewName;
        private TextBox textboxNewName;
        private Button buttonOK;
        private Button buttonCancel;
    }
}