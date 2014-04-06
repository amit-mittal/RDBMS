using System;
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

		static void InitTests()
		{
			new Logger().CleanUp();
			var storageManagerTest = new StorageManagerTest();
			storageManagerTest.Init();

			var converterTest = new ConverterTest();
			converterTest.Init();
		}
    }
}
