using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDBMS.DataStructure;
using RDBMS.FileManager;
using RDBMS.Util;

namespace RDBMS.Testing
{
	[TestClass]
	class TableManagerTest
	{
		private Logger _logger;
		private static TableManager manager = new TableManager();
		private const String dbName = "Sample";
		private const String tableName = "Table1";

		[TestInitialize]
		private void setUp()
		{
			DatabaseManager dbManager = new DatabaseManager();
			dbManager.CreateDatabase(dbName);
		}

		[TestCleanup]
		private void cleanUp()
		{
			DatabaseManager dbManager = new DatabaseManager();
			dbManager.DropDatabase(dbName);
		}

		[TestMethod]
		private void TestCreateDropTable()
		{
			try
			{
				_logger.Message("Testing TestCreateDropTable");
				manager.CreateTable(dbName, tableName, new List<Column>());
				Assert.IsTrue(Directory.Exists(GetFilePath.Table(dbName, tableName)));
				Assert.IsTrue(File.Exists(GetFilePath.TableConf(dbName, tableName)));
				Assert.IsTrue(File.Exists(GetFilePath.TableRecords(dbName, tableName)));
				
				manager.DropTable(dbName, tableName);
				Assert.IsTrue(!Directory.Exists(GetFilePath.Table(dbName, tableName)));
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		[TestMethod]
		private void TestInsertRecord()
		{
			try
			{
				_logger.Message("Testing TestInsertRecord");
				//path to files related to table
				String recordPath = GetFilePath.TableRecords(dbName, tableName);

				//making the table
				List<Column> cols = new List<Column>();
				cols.Add(new Column(Column.DataType.Int, "Int", 100));
				cols.Add(new Column(Column.DataType.Double, "Double", 1));
				cols.Add(new Column(Column.DataType.Char, "String", 20));
				manager.CreateTable(dbName, tableName, cols);

				//making records to be inserted
				List<String> l1 = new List<string>();
				l1.Add("5");
				l1.Add("5.1");
				l1.Add("random1");
				Record r1 = new Record(l1);

				manager.InsertRecord(r1);

				Stream fs = new FileStream(recordPath, FileMode.OpenOrCreate);
				byte[] r1Bytes = manager.storageManager.Read(fs, manager.storageManager.HeaderSize, 1);
				Record actualR1 = manager.table.StringToRecord(new string(Converter.BytesToChar(r1Bytes)));
				AssertRecords(r1, actualR1);
				
				fs.Close();
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
			finally
			{
				manager.DropTable(dbName, tableName);
			}
		}

		private void AssertRecords(Record expected, Record actual)
		{
			Assert.AreEqual(expected.Fields.Count, actual.Fields.Count);
			for (int i = 0; i < expected.Fields.Count; i++)
			{
				Assert.AreEqual(expected.Fields[i], actual.Fields[i]);
			}
		}

		public void Init()
		{
			_logger = new Logger("TableManagerTest");

			setUp();
			TestCreateDropTable();
			TestInsertRecord();
			cleanUp();

			_logger.Close();
		}
	}
}
