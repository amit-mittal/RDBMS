using System;
using System.Windows.Forms;

namespace RDBMS.Util
{
	class DisplayMessage
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
				form.OutputBox.Text = msg;
			}
		}
	}
}
