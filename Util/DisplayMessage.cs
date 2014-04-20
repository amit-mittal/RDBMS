using System;
using System.Windows.Forms;

namespace RDBMS.Util
{
	internal class DisplayMessage
	{
		public SqlForm form;

		public DisplayMessage()
		{
		}

		public DisplayMessage(SqlForm form)
		{
			this.form = form;
		}

		public void Message(String msg)
		{
			if (form == null)
			{
				Console.WriteLine(msg);
			}
			else
			{
				form.OutputBox.AppendText(msg);
				form.OutputBox.AppendText(Environment.NewLine);
			}
		}

		public void Error(String msg)
		{
			if (form == null)
			{
				Console.WriteLine(msg);
			}
			else
			{
				form.ErrorBox.Text = msg;
			}
		}
	}
}