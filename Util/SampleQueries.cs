using System;

namespace RDBMS.Util
{
	static class SampleQueries
	{
		//GENERAL QUERY
		public static String ShowDatabases = "SHOW DATABASES";


		//DATABASE QUERIES
		public static String CreateDatabase = "CREATE DATABASE sample_data";

		public static String DropDatabase = "DROP DATABASE sample_data";

		public static String UseDatabase = "USE sample_data";

		public static String ShowTables = "SHOW TABLES";


		//TABLE QUERIES
		public static String DescribeTable = "DESCRIBE table_1";

		public static String CreateTable = "CREATE TABLE table_1 ( " +
											"col_1 INT (10), " +
											"col_2 CHAR (100)" +
											")";

		public static String DropTable = "DROP TABLE table_1";

		public static String InsertRecord = "INSERT INTO table_1 " +
											"(col_1, col_2) " +
											"VALUES(10, 'hello world')";

		public static String UpdateRecord= "UPDATE table_1 " +
											"SET col_1 = 13 " +
											"WHERE col_2 = 'hello world'";

		public static String DeleteRecord = "DELETE FROM table_1 " +
											"WHERE col_1 = 13";

		public static String SelectRecord = "SELECT * FROM table_1 " +
											"WHERE ((col_2 = 'hello world') AND (col_1 = 13))"


		//INDEX QUERIES
		public static String CreateIndex = "CREATE INDEX col_1 ON table_1";

		public static String DropIndex = "DROP INDEX column_1 ON table_1";
	}
}
