using System;
using System.Windows.Forms;
using RDBMS.QueryManager;
using RDBMS.Util;

namespace RDBMS
{
	public partial class SqlForm : Form
	{
		private QueryHandler qh;

		public SqlForm()
		{
			qh = new QueryHandler(this);
			InitializeComponent();
		}
		
		public void queryButton_Click(object sender, EventArgs args)
		{
			try
			{
				qh.SetQuery(QueryBox.Text);
			}
			
			catch (Exception e)
			{
				ErrorBox.AppendText(e.Message);
			}
		}

		public void SqlForm_Load(object sender, EventArgs e)
		{
			var _message = new DisplayMessage();
			_message.Message("This is the form. Do your thing.");
		}

		private void ErrorBox_TextChanged(object sender, EventArgs e)
		{

		}

		private void OutputBox_TextChanged(object sender, EventArgs e)
		{

		}

		private void QueryBox_TextChanged(object sender, EventArgs e)
		{

		}

	}
}
