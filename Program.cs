using System;
using Kw.Data;
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

			/*Index<String> index = new Index<String>("jello", 1);
			Console.WriteLine(index.Key);
			Converter.ObjectToFile(index, "E:\\SaSa");

			Index<String> ii = (Index<String>)Converter.FileToObject("E:\\SaSa");
			Console.WriteLine(ii.Key);
			Console.WriteLine(ii.Address);

			BtreeDictionary<Index<int>, string > ff = new BtreeDictionary<Index<int>, string>();
			ff.Add(new Index<int>(22, 34), "hello");
			ff.Add(new Index<int>(23, 48), "hell");
			ff.Add(new Index<int>(90, 11), "hel");
			int iii;
			Leaf<Index<int>, string > leaf = ff.Find(new Index<int>(23, 48), out iii);
			Console.WriteLine(leaf.ValueCount);

			Converter.ObjectToFile(ff, "E:\\SaSa");
			BtreeDictionary<Index<int>, string> bbtree = (BtreeDictionary<Index<int>, string>) Converter.FileToObject("E:\\SaSa");
			Console.WriteLine(bbtree.Count);*/


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
