using System;
using System.Windows.Forms;
using RDBMS.QueryManager;
using RDBMS.Testing;
using RDBMS.Util;

namespace RDBMS
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			
			//doing the testing
			InitTests();

			//do the work here
			SqlForm form = new SqlForm();
			Application.EnableVisualStyles();

			Application.Run(form);
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