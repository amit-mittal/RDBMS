using System;
using System.IO;
using RDBMS.SpaceManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RDBMS.Testing
{
	[TestClass]
	class StorageManagerTest
	{
		private static readonly StorageManager Manager = new StorageManager();

		[TestMethod]
		private void TestCreateDropFile()
		{
			try
			{
				Manager.CreateFile("E:\\Sample", 10);
				Assert.IsTrue(File.Exists("E:\\Sample"));
				Assert.AreEqual(File.ReadAllBytes("E:\\Sample").Length, 8);
				//preferably check those written values also
				
				Manager.DropFile("E:\\Sample");
				Assert.IsTrue(!File.Exists("E:\\Sample"));
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.ReadKey();
				//TODO: instead of console use logger
			}
		}

		public void Init()
		{
			TestCreateDropFile();
		}
	}
}
