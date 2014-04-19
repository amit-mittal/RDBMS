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
			String str = "SELECT col_1 OF t1, col_2 OF t2 FROM t1 OF table_1, t2 OF table_2 " +
						"WHERE ((col_3 OF t1 = 3) AND (col_4 OF t1 = val_4 OF t2))";
			qh.SetQuery(str);
			//do the work here

			InitTests();
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