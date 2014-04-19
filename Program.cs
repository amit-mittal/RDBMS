using System;
using RDBMS.QueryManager;
using RDBMS.Testing;
using RDBMS.Util;

namespace RDBMS
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var qh = new QueryHandler();
			String str = "SELECT * FROM table_1 " +
						"WHERE ((col_2 = 'hello world') AND (col_1 = 13)) " +
						"ORDER BY col_1 asc";
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