namespace RDBMS
{
	partial class SqlForm
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
			this.QueryButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.ErrorBox = new System.Windows.Forms.RichTextBox();
			this.OutputBox = new System.Windows.Forms.RichTextBox();
			this.QueryBox = new System.Windows.Forms.RichTextBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// QueryButton
			// 
			this.QueryButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QueryButton.Font = new System.Drawing.Font("Century Schoolbook", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.QueryButton.Location = new System.Drawing.Point(470, 567);
			this.QueryButton.Name = "QueryButton";
			this.QueryButton.Size = new System.Drawing.Size(188, 29);
			this.QueryButton.TabIndex = 3;
			this.QueryButton.Text = "Execute SQL Query";
			this.QueryButton.UseVisualStyleBackColor = true;
			this.QueryButton.Click += new System.EventHandler(this.QueryButton_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Century Schoolbook", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(14, 58);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(85, 15);
			this.label1.TabIndex = 5;
			this.label1.Text = "SQL QUERY";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Century Schoolbook", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(14, 155);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 15);
			this.label2.TabIndex = 6;
			this.label2.Text = "ERROR";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Century Schoolbook", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(14, 259);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 15);
			this.label3.TabIndex = 7;
			this.label3.Text = "OUTPUT";
			// 
			// ErrorBox
			// 
			this.ErrorBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ErrorBox.Font = new System.Drawing.Font("Century Schoolbook", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ErrorBox.ForeColor = System.Drawing.Color.Red;
			this.ErrorBox.Location = new System.Drawing.Point(112, 150);
			this.ErrorBox.Name = "ErrorBox";
			this.ErrorBox.ReadOnly = true;
			this.ErrorBox.Size = new System.Drawing.Size(546, 74);
			this.ErrorBox.TabIndex = 8;
			this.ErrorBox.Text = "";
			// 
			// OutputBox
			// 
			this.OutputBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputBox.Font = new System.Drawing.Font("Century Schoolbook", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OutputBox.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.OutputBox.Location = new System.Drawing.Point(112, 255);
			this.OutputBox.Name = "OutputBox";
			this.OutputBox.ReadOnly = true;
			this.OutputBox.Size = new System.Drawing.Size(546, 265);
			this.OutputBox.TabIndex = 9;
			this.OutputBox.Text = "";
			// 
			// QueryBox
			// 
			this.QueryBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.QueryBox.Location = new System.Drawing.Point(112, 54);
			this.QueryBox.Name = "QueryBox";
			this.QueryBox.Size = new System.Drawing.Size(546, 67);
			this.QueryBox.TabIndex = 10;
			this.QueryBox.Text = "";
			this.QueryBox.Enter += new System.EventHandler(this.QueryBox_Enter);
			this.QueryBox.Leave += new System.EventHandler(this.QueryBox_Leave_1);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.cancelButton.Font = new System.Drawing.Font("Century Schoolbook", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cancelButton.Location = new System.Drawing.Point(112, 567);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(188, 29);
			this.cancelButton.TabIndex = 11;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// SqlForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(702, 658);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.QueryBox);
			this.Controls.Add(this.OutputBox);
			this.Controls.Add(this.ErrorBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.QueryButton);
			this.Font = new System.Drawing.Font("Century Schoolbook", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SqlForm";
			this.Text = "SQL Query Evaluator";
			this.Load += new System.EventHandler(this.SqlForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.Button QueryButton;
		public System.Windows.Forms.Label label1;
		public System.Windows.Forms.Label label2;
		public System.Windows.Forms.Label label3;
		public System.Windows.Forms.RichTextBox ErrorBox;
		public System.Windows.Forms.RichTextBox OutputBox;
		public System.Windows.Forms.RichTextBox QueryBox;
		private System.Windows.Forms.Button cancelButton;
	}
}