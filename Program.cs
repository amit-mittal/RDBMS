using System;
using RDBMS.QueryManager;
using RDBMS.Testing;
using RDBMS.Util;

namespace RDBMS
{
	internal class Program
	{
		//TODO Change to form application
		private static void Main(string[] args)
		{
			var qh = new QueryHandler();

			/*String str = "create database sample_data";
			qh.SetQuery(str);

			str = "use sample_data";
			qh.SetQuery(str);

			str = "create table table1(" +
				"col1 char (100)," +
				"col2 int (5)" +
				")";
			qh.SetQuery(str);*/

			String str = "create index name_1 on table_1";
			qh.SetQuery(str);
/*
			str = "insert into table1 (col1, col2) values ('first', 22) ";
			qh.SetQuery(str);

			str = "SELECT col2 FROM table1 " +
						"WHERE " +
						"COL1 = 'first' ";
			qh.SetQuery(str);*/

			//InitTests();
			Console.ReadKey();
		}

		/**
		 * Calls test methods of the project
		 * Tests fail if some error is there in 
		 * the implementation
		 */

		private static void InitTests()
		{
			new Logger().CleanUp();
			var storageManagerTest = new StorageManagerTest();
			storageManagerTest.Init();

			var converterTest = new ConverterTest();
			converterTest.Init();

			var databaseManagerTest = new DatabaseManagerTest();
			databaseManagerTest.Init();

			var tableManagerTest = new TableManagerTest();
			tableManagerTest.Init();
		}
	}
}