namespace HyperView.Forms
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            labelLoginFormToolName = new Label();
            pictureBox1 = new PictureBox();
            labelServer = new Label();
            textboxServer = new TextBox();
            groupAuth = new GroupBox();
            buttonCancel = new Button();
            ButtonLogin = new Button();
            checkboxRemember = new CheckBox();
            labelPassword = new Label();
            labelUsername = new Label();
            textboxPassword = new TextBox();
            textboxUsername = new TextBox();
            radioCustom = new RadioButton();
            radioWindows = new RadioButton();
            buttonHelpConnectGuide = new Button();
            statusStripLoginForm = new StatusStrip();
            toolStripStatusLabelLoginForm = new ToolStripStatusLabel();
            toolStripStatusLabelTextLoginForm = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupAuth.SuspendLayout();
            statusStripLoginForm.SuspendLayout();
            SuspendLayout();
            // 
            // labelLoginFormToolName
            // 
            labelLoginFormToolName.AutoSize = true;
            labelLoginFormToolName.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelLoginFormToolName.Location = new Point(147, 68);
            labelLoginFormToolName.Name = "labelLoginFormToolName";
            labelLoginFormToolName.Size = new Size(135, 30);
            labelLoginFormToolName.TabIndex = 0;
            labelLoginFormToolName.Text = "%toolname%";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.hyper_v;
            pictureBox1.Location = new Point(36, 30);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(105, 106);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // labelServer
            // 
            labelServer.AutoSize = true;
            labelServer.Location = new Point(21, 170);
            labelServer.Name = "labelServer";
            labelServer.Size = new Size(83, 15);
            labelServer.TabIndex = 2;
            labelServer.Text = "Server IP/DNS:";
            // 
            // textboxServer
            // 
            textboxServer.Location = new Point(110, 167);
            textboxServer.Name = "textboxServer";
            textboxServer.Size = new Size(271, 23);
            textboxServer.TabIndex = 3;
            textboxServer.TextChanged += textboxServer_TextChanged;
            textboxServer.KeyDown += TextboxServer_KeyDown;
            // 
            // groupAuth
            // 
            groupAuth.Controls.Add(buttonCancel);
            groupAuth.Controls.Add(ButtonLogin);
            groupAuth.Controls.Add(checkboxRemember);
            groupAuth.Controls.Add(labelPassword);
            groupAuth.Controls.Add(labelUsername);
            groupAuth.Controls.Add(textboxPassword);
            groupAuth.Controls.Add(textboxUsername);
            groupAuth.Controls.Add(radioCustom);
            groupAuth.Controls.Add(radioWindows);
            groupAuth.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupAuth.Location = new Point(12, 205);
            groupAuth.Name = "groupAuth";
            groupAuth.Size = new Size(407, 209);
            groupAuth.TabIndex = 4;
            groupAuth.TabStop = false;
            groupAuth.Text = "Authentication Method";
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(98, 176);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 8;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += ButtonCancel_Click;
            // 
            // ButtonLogin
            // 
            ButtonLogin.Location = new Point(236, 176);
            ButtonLogin.Name = "ButtonLogin";
            ButtonLogin.Size = new Size(75, 23);
            ButtonLogin.TabIndex = 7;
            ButtonLogin.Text = "Login";
            ButtonLogin.UseVisualStyleBackColor = true;
            ButtonLogin.Click += ButtonLogin_Click;
            // 
            // checkboxRemember
            // 
            checkboxRemember.AutoSize = true;
            checkboxRemember.Font = new Font("Segoe UI", 9F);
            checkboxRemember.Location = new Point(122, 143);
            checkboxRemember.Name = "checkboxRemember";
            checkboxRemember.Size = new Size(208, 19);
            checkboxRemember.TabIndex = 6;
            checkboxRemember.Text = "Remember credentials (encrypted)";
            checkboxRemember.UseVisualStyleBackColor = true;
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Font = new Font("Segoe UI", 9F);
            labelPassword.Location = new Point(37, 111);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(60, 15);
            labelPassword.TabIndex = 5;
            labelPassword.Text = "Password:";
            // 
            // labelUsername
            // 
            labelUsername.AutoSize = true;
            labelUsername.Font = new Font("Segoe UI", 9F);
            labelUsername.Location = new Point(37, 79);
            labelUsername.Name = "labelUsername";
            labelUsername.Size = new Size(63, 15);
            labelUsername.TabIndex = 4;
            labelUsername.Text = "Username:";
            // 
            // textboxPassword
            // 
            textboxPassword.Font = new Font("Segoe UI", 9F);
            textboxPassword.Location = new Point(106, 108);
            textboxPassword.Name = "textboxPassword";
            textboxPassword.Size = new Size(284, 23);
            textboxPassword.TabIndex = 3;
            textboxPassword.KeyDown += TextboxPassword_KeyDown;
            // 
            // textboxUsername
            // 
            textboxUsername.Font = new Font("Segoe UI", 9F);
            textboxUsername.Location = new Point(106, 79);
            textboxUsername.Name = "textboxUsername";
            textboxUsername.Size = new Size(284, 23);
            textboxUsername.TabIndex = 2;
            textboxUsername.KeyDown += TextboxUsername_KeyDown;
            // 
            // radioCustom
            // 
            radioCustom.AutoSize = true;
            radioCustom.Font = new Font("Segoe UI", 9F);
            radioCustom.Location = new Point(23, 50);
            radioCustom.Name = "radioCustom";
            radioCustom.Size = new Size(147, 19);
            radioCustom.TabIndex = 1;
            radioCustom.TabStop = true;
            radioCustom.Text = "Use specific credentials";
            radioCustom.UseVisualStyleBackColor = true;
            radioCustom.CheckedChanged += RadioAuth_CheckedChanged;
            // 
            // radioWindows
            // 
            radioWindows.AutoSize = true;
            radioWindows.Checked = true;
            radioWindows.Font = new Font("Segoe UI", 9F);
            radioWindows.Location = new Point(23, 25);
            radioWindows.Name = "radioWindows";
            radioWindows.Size = new Size(238, 19);
            radioWindows.TabIndex = 0;
            radioWindows.TabStop = true;
            radioWindows.Text = "Use current Windows session credentials";
            radioWindows.UseVisualStyleBackColor = true;
            radioWindows.CheckedChanged += RadioAuth_CheckedChanged;
            // 
            // buttonHelpConnectGuide
            // 
            buttonHelpConnectGuide.Location = new Point(387, 167);
            buttonHelpConnectGuide.Name = "buttonHelpConnectGuide";
            buttonHelpConnectGuide.Size = new Size(36, 23);
            buttonHelpConnectGuide.TabIndex = 5;
            buttonHelpConnectGuide.UseVisualStyleBackColor = true;
            buttonHelpConnectGuide.Click += buttonHelpConnectGuide_Click;
            // 
            // statusStripLoginForm
            // 
            statusStripLoginForm.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelLoginForm, toolStripStatusLabelTextLoginForm });
            statusStripLoginForm.Location = new Point(0, 428);
            statusStripLoginForm.Name = "statusStripLoginForm";
            statusStripLoginForm.Size = new Size(432, 22);
            statusStripLoginForm.SizingGrip = false;
            statusStripLoginForm.TabIndex = 6;
            statusStripLoginForm.Text = "statusStrip1";
            // 
            // toolStripStatusLabelLoginForm
            // 
            toolStripStatusLabelLoginForm.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            toolStripStatusLabelLoginForm.Name = "toolStripStatusLabelLoginForm";
            toolStripStatusLabelLoginForm.Size = new Size(45, 17);
            toolStripStatusLabelLoginForm.Text = "Status:";
            // 
            // toolStripStatusLabelTextLoginForm
            // 
            toolStripStatusLabelTextLoginForm.Name = "toolStripStatusLabelTextLoginForm";
            toolStripStatusLabelTextLoginForm.Size = new Size(67, 17);
            toolStripStatusLabelTextLoginForm.Text = "%STATUS%";
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(432, 450);
            Controls.Add(statusStripLoginForm);
            Controls.Add(buttonHelpConnectGuide);
            Controls.Add(groupAuth);
            Controls.Add(textboxServer);
            Controls.Add(labelServer);
            Controls.Add(pictureBox1);
            Controls.Add(labelLoginFormToolName);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "LoginForm";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            groupAuth.ResumeLayout(false);
            groupAuth.PerformLayout();
            statusStripLoginForm.ResumeLayout(false);
            statusStripLoginForm.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelLoginFormToolName;
        private PictureBox pictureBox1;
        private Label labelServer;
        private TextBox textboxServer;
        private GroupBox groupAuth;
        private RadioButton radioCustom;
        private RadioButton radioWindows;
        private Label labelPassword;
        private Label labelUsername;
        private TextBox textboxPassword;
        private TextBox textboxUsername;
        private Button buttonCancel;
        private Button ButtonLogin;
        private CheckBox checkboxRemember;
        private Button buttonHelpConnectGuide;
        private StatusStrip statusStripLoginForm;
        private ToolStripStatusLabel toolStripStatusLabelLoginForm;
        private ToolStripStatusLabel toolStripStatusLabelTextLoginForm;
    }
}