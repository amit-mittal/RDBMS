using System;

namespace RDBMS.Util
{
	/**
	 * Returns the paths of the directory
	 * or file according to if database or table
	 */

	internal static class GetFilePath
	{
		public const String Root = "E:\\RDBMS\\";

		public static string Database(string dbName)
		{
			return Root + dbName;
		}

		public static string DatabaseConf(string dbName)
		{
			return Root + dbName + "\\conf";
		}

		public static String Table(String dbName, String tableName)
		{
			return Root + dbName + "\\" + tableName;
		}

		public static String TableConf(String dbName, String tableName)
		{
			return Root + dbName + "\\" + tableName + "\\conf";
		}

		public static String TableRecords(String dbName, String tableName)
		{
			return Root + dbName + "\\" + tableName + "\\records";
		}

		public static String TableRecordsBitmap(String dbName, String tableName)
		{
			return Root + dbName + "\\" + tableName + "\\records - BitMap";
		}

		public static String TableColumnIndex(String dbName, String tableName, String columnName)
		{
			return Root + dbName + "\\" + tableName + "\\" + columnName + " -index";
		}
	}
}