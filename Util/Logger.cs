using System;
using System.IO;

namespace RDBMS.Util
{
	class Logger
	{
		private const String LogFile = "E:\\Log.txt";
		private static String _class;
		private static StreamWriter _sw;

		public Logger()
		{
		}

		public Logger(String className)
		{
			_class = className;
			_sw = new StreamWriter(LogFile, true);
		}

		public void CleanUp()
		{
			_sw = new StreamWriter(LogFile);
			_sw.Close();
		}

		public void Error(String msg)
		{
			_sw.WriteLine(_class + " ERROR: " + msg);
		}

		public void Message(String msg)
		{
			_sw.WriteLine(_class + " MESSAGE: " + msg);
		}

		public void Close()
		{
			_sw.Close();
		}
	}
}
