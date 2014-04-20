using System;
using System.Windows.Forms;
using System.Drawing;
using RDBMS.QueryManager;
using RDBMS.Util;

namespace RDBMS
{
	public partial class SqlForm : Form
	{
		private QueryHandler qh;
		private ToolTip tip;

		public SqlForm()
		{
			qh = new QueryHandler(this);
			tip = new ToolTip();
			InitializeComponent();
		}
		
		private void ToolTip()
		{
			this.tip.SetToolTip(QueryBox, "Enter SQL Query here");
			this.tip.SetToolTip(ErrorBox, "Error Messages are displayed here");
			this.tip.SetToolTip(OutputBox, "Output is displayed here");
			this.tip.SetToolTip(QueryButton, "Click for evaluating query");
			this.tip.SetToolTip(cancelButton, "Click to exit the GUI");
		}

		public void QueryButton_Click(object sender, EventArgs args)
		{
			try
			{
				ErrorBox.Clear();
				OutputBox.Clear();
				qh.SetQuery(QueryBox.Text);
			}
			
			catch (Exception e)
			{
				ErrorBox.Text = e.Message;
			}
		}

		public void SqlForm_Load(object sender, EventArgs e)
		{
			ToolTip();
			QueryBox.Text = "Enter Query Here";
			QueryBox.ForeColor = Color.Gray;
			this.AcceptButton = QueryButton;
			var _message = new DisplayMessage();
			_message.Message("This is the form. Do your thing.");
		}

		private void QueryBox_Leave_1(object sender, EventArgs e)
		{
			if (String.IsNullOrWhiteSpace(QueryBox.Text))
			{
				QueryBox.Text = "Enter Query Here";
				QueryBox.ForeColor = Color.Gray;
			}
		}

		private void QueryBox_Enter(object sender, EventArgs e)
		{
			if (QueryBox.Text.Equals("Enter Query Here") == true && QueryBox.ForeColor == Color.Gray)
			{
				QueryBox.Clear();
				QueryBox.ForeColor = Color.Black;
			}
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

	}
}
