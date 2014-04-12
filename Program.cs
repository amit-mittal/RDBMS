using System;
using System.Collections.Generic;
using RDBMS.DataStructure;
using RDBMS.Testing;
using RDBMS.Util;

namespace RDBMS
{
    class Program
    {
        static void Main(string[] args)
        {
			InitTests();
	        Console.ReadKey();
        }

		/**
		 * Calls test methods of the project
		 * Tests fail if some error is there in 
		 * the implementation
		 */
		static void InitTests()
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
