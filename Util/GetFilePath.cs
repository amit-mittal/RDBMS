using System;

namespace RDBMS.Util
{
	static class GetFilePath
	{
		private const String Root = "E:\\RDBMS\\";

		public static String GetDatabasePath(String dbName)
		{
			return Root + dbName;
		}

		public static String GetTablePath(String dbName, String tableName)
		{
			return Root + dbName + "\\" + tableName;
		}

		public static String GetTableRecordsPath(String dbName, String tableName)
		{
			return Root + dbName + "\\" + tableName + "\\records";
		}
	}
}
