using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDBMS.FileManager;
using RDBMS.Util;

namespace RDBMS.Testing
{
	[TestClass]
	internal class DatabaseManagerTest
	{
		private Logger _logger;
		private static DatabaseManager manager = new DatabaseManager();
		private const String dbName = "Sample";

		[TestMethod]
		private void TestCreateDropDatabase()
		{
			try
			{
				//database does not exist
				_logger.Message("Testing CreateDropDatabase");
				manager.CreateDatabase(dbName);
				Assert.IsTrue(Directory.Exists(GetFilePath.Database(dbName)));
				//not checking conf file as that is not getting made right now

				manager.DropDatabase(dbName);
				Assert.IsTrue(!Directory.Exists(GetFilePath.Database(dbName)));
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		[TestMethod]
		private void TestUseDatabase()
		{
			try
			{
				_logger.Message("Testing UseDatabase");
				manager.CreateDatabase(dbName);
				Assert.IsNull(manager.db);
				Assert.IsNotNull(manager.storageManager);

				manager.UseDatabase(dbName);
				Assert.IsNotNull(manager.db);
				Assert.AreEqual(dbName, manager.db.Name);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
			finally
			{
				manager.DropDatabase(dbName);
			}
		}

		[TestMethod]
		private void TestShowTables()
		{
			try
			{
				_logger.Message("Testing ShowTables");
				manager.CreateDatabase(dbName);
				manager.UseDatabase(dbName);

				String dbPath = GetFilePath.Database(dbName);
				for (int i = 0; i < 2; i++)
				{
					Directory.CreateDirectory(dbPath + "\\subfolder" + i);
				}

				List<String> subdirList = manager.ShowTables();
				Assert.AreEqual(2, subdirList.Count);
				Assert.AreEqual("subfolder0", subdirList[0]);
				Assert.AreEqual("subfolder1", subdirList[1]);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
			finally
			{
				manager.DropDatabase(dbName);
			}
		}

		public void Init()
		{
			_logger = new Logger("DatabaseManagerTest");

			TestCreateDropDatabase();
			TestUseDatabase();
			TestShowTables();

			_logger.Close();
		}
	}
}