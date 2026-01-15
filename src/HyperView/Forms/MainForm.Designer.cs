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
            ((System.ComponentModel.ISupportInitialize)datagridviewVMOverView).BeginInit();
            SuspendLayout();
            // 
            // datagridviewVMOverView
            // 
            datagridviewVMOverView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridviewVMOverView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datagridviewVMOverView.Location = new Point(467, 524);
            datagridviewVMOverView.Name = "datagridviewVMOverView";
            datagridviewVMOverView.Size = new Size(1034, 356);
            datagridviewVMOverView.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1513, 892);
            Controls.Add(datagridviewVMOverView);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            ((System.ComponentModel.ISupportInitialize)datagridviewVMOverView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView datagridviewVMOverView;
    }
}
