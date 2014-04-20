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
			this.QueryBox = new System.Windows.Forms.TextBox();
			this.QueryButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.ErrorBox = new System.Windows.Forms.RichTextBox();
			this.OutputBox = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// QueryBox
			// 
			this.QueryBox.Location = new System.Drawing.Point(99, 46);
			this.QueryBox.Name = "QueryBox";
			this.QueryBox.Size = new System.Drawing.Size(228, 20);
			this.QueryBox.TabIndex = 0;
			this.QueryBox.TextChanged += new System.EventHandler(this.QueryBox_TextChanged);
			// 
			// QueryButton
			// 
			this.QueryButton.Location = new System.Drawing.Point(135, 304);
			this.QueryButton.Name = "QueryButton";
			this.QueryButton.Size = new System.Drawing.Size(114, 23);
			this.QueryButton.TabIndex = 3;
			this.QueryButton.Text = "Execute SQL Query";
			this.QueryButton.UseVisualStyleBackColor = true;
			this.QueryButton.Click += new System.EventHandler(this.queryButton_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 50);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(69, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "SQL QUERY";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 129);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(46, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "ERROR";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(15, 209);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(52, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "OUTPUT";
			// 
			// ErrorBox
			// 
			this.ErrorBox.Location = new System.Drawing.Point(99, 123);
			this.ErrorBox.Name = "ErrorBox";
			this.ErrorBox.Size = new System.Drawing.Size(228, 21);
			this.ErrorBox.TabIndex = 8;
			this.ErrorBox.Text = "";
			// 
			// OutputBox
			// 
			this.OutputBox.Location = new System.Drawing.Point(99, 201);
			this.OutputBox.Name = "OutputBox";
			this.OutputBox.Size = new System.Drawing.Size(228, 83);
			this.OutputBox.TabIndex = 9;
			this.OutputBox.Text = "";
			// 
			// SqlForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(388, 342);
			this.Controls.Add(this.OutputBox);
			this.Controls.Add(this.ErrorBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.QueryButton);
			this.Controls.Add(this.QueryBox);
			this.Name = "SqlForm";
			this.Text = "SQL Query Evaluator";
			this.Load += new System.EventHandler(this.SqlForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.TextBox QueryBox;
		public System.Windows.Forms.Button QueryButton;
		public System.Windows.Forms.Label label1;
		public System.Windows.Forms.Label label2;
		public System.Windows.Forms.Label label3;
		public System.Windows.Forms.RichTextBox ErrorBox;
		public System.Windows.Forms.RichTextBox OutputBox;
	}
}