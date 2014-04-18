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
			Console.Write("merasql> ");
			String str = "UPDATE TABLE_1 SET " +
						"COL_1='hello' , " +
						"COL_2 = 5 " +
						"WHERE " +
						"COL_3 = 8 " +
						"AND " +
						"COL_4 = 'HEG'";
			qh.SetQuery(str);

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