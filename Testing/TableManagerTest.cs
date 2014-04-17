using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Kw.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDBMS.DataStructure;
using RDBMS.FileManager;
using RDBMS.Util;

namespace RDBMS.Testing
{
	[TestClass]
	internal class TableManagerTest
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
		private void TestConstructor()
		{
			try
			{
				_logger.Message("Testing Constructor");
				List<Column> cols = new List<Column>();
				cols.Add(new Column(Column.DataType.Int, "Int", 100));
				cols.Add(new Column(Column.DataType.Double, "Double", 1));
				cols.Add(new Column(Column.DataType.Char, "String", 20));

				List<Column> indices = new List<Column>();
				manager.CreateTable(dbName, tableName, cols);

				TableManager tm = new TableManager(dbName, tableName);
				Assert.AreEqual(dbName, tm.table.DbName);
				Assert.AreEqual(tableName, tm.table.Name);
				Assert.AreEqual(manager.table.IndexColumns.Count, tm.table.IndexColumns.Count);
				Assert.AreEqual(manager.table.Columns.Count, tm.table.Columns.Count);
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

		[TestMethod]
		private void TestCreateDropTable()
		{
			try
			{
				_logger.Message("Testing CreateDropTable");
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
		private void TestUpdateTableToFile()
		{
			try
			{
				_logger.Message("Testing UpdateTableToFile");
				manager.CreateTable(dbName, tableName, new List<Column>());
				manager.table.IndexColumns.Add(new Column(Column.DataType.Char, "hello", 5));
				manager.table.Columns.Add(new Column(Column.DataType.Int, "hello", 5));

				manager.UpdateTableToFile();

				TableManager tm = new TableManager(dbName, tableName);
				Assert.AreEqual(manager.table.DbName, tm.table.DbName);
				Assert.AreEqual(manager.table.Name, tm.table.Name);
				Assert.AreEqual(manager.table.Columns.Count, tm.table.Columns.Count);
				Assert.AreEqual(manager.table.IndexColumns.Count, tm.table.IndexColumns.Count);

				manager.DropTable(dbName, tableName);
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
				_logger.Message("Testing InsertRecord");
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
				byte[] r1Bytes = manager.storageManager.Read(fs, manager.storageManager.HeaderSize);
				Record actualR1 = manager.table.StringToRecord(new string(Converter.BytesToChar(r1Bytes)));
				AssertRecords(r1, actualR1);
				fs.Close();

				//inserting a field having null value
				List<String> l2 = new List<string>();
				l2.Add("2048000");
				l2.Add("5.2");
				l2.Add("random2");
				Record r2 = new Record(l2);

				manager.InsertRecord(r2);

				fs = new FileStream(recordPath, FileMode.OpenOrCreate);
				r1Bytes = manager.storageManager.Read(fs, manager.storageManager.HeaderSize);
				actualR1 = manager.table.StringToRecord(new string(Converter.BytesToChar(r1Bytes)));
				AssertRecords(r1, actualR1);

				byte[] r2Bytes = manager.storageManager.Read(fs, manager.storageManager.HeaderSize + r1Bytes.Length);
				Record actualR2 = manager.table.StringToRecord(new string(Converter.BytesToChar(r2Bytes)));
				AssertRecords(r2, actualR2);
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

		[TestMethod]
		private void TestSelectRecords()
		{
			try
			{
				_logger.Message("Testing SelectRecords");
				//path to files related to table
				String bitmapPath = GetFilePath.TableRecordsBitmap(dbName, tableName);

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

				List<String> l2 = new List<string>();
				l2.Add("2048000");
				l2.Add("5.2");
				l2.Add("random2");
				Record r2 = new Record(l2);

				manager.InsertRecord(r1);
				manager.InsertRecord(r2);

				List<Record> records = manager.SelectRecords(null);
				Assert.IsTrue(records.Count == 2);
				AssertRecords(r1, records[0]);
				AssertRecords(r2, records[1]);

				records =
					manager.SelectRecords(new Condition(new Column(Column.DataType.Double, "Double", 1), Condition.ConditionType.Equal,
						"5.20"));
				Assert.IsTrue(records.Count == 1);
				AssertRecords(r2, records[0]);

				//adding a value in bitmap where in between space is empty
				using (Stream fs = new FileStream(bitmapPath, FileMode.OpenOrCreate))
				{
					manager.storageManager.Write(fs, 12, Converter.IntToBytes(12 + (42*10)));
					manager.storageManager.SetEndOfFile(fs, 16);
				}
				List<String> l3 = new List<string>();
				l3.Add("-409600");
				l3.Add("5.3");
				l3.Add("random3");
				Record r3 = new Record(l3);

				manager.InsertRecord(r3);
				records =
					manager.SelectRecords(new Condition(new Column(Column.DataType.Int, "Int", 100), Condition.ConditionType.LessEqual,
						"5"));
				Assert.AreEqual(2, records.Count);
				AssertRecords(r1, records[0]);
				AssertRecords(r3, records[1]);
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

		[TestMethod]
		private void TestDeleteRecords()
		{
			try
			{
				_logger.Message("Testing DeleteRecords");
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

				List<String> l2 = new List<string>();
				l2.Add("2048000");
				l2.Add("5.2");
				l2.Add("random2");
				Record r2 = new Record(l2);

				manager.InsertRecord(r1);
				manager.InsertRecord(r2);

				Dictionary<int, Record> recordsToDelete = new Dictionary<int, Record>();
				recordsToDelete.Add(12, r1);
				manager.DeleteRecords(recordsToDelete);

				List<Record> allRecords = manager.SelectRecords(null);
				Assert.AreEqual(1, allRecords.Count);
				AssertRecords(r2, allRecords[0]);
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

		[TestMethod]
		private void TestUpdateRecords()
		{
			try
			{
				_logger.Message("Testing UpdateRecords");
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

				List<String> l2 = new List<string>();
				l2.Add("2048000");
				l2.Add("5.2");
				l2.Add("random2");
				Record r2 = new Record(l2);

				manager.InsertRecord(r1);
				manager.InsertRecord(r2);

				Dictionary<int, Record> updatedRecords = new Dictionary<int, Record>();
				l2[2] = "string updated";
				updatedRecords.Add(54, r2);
				manager.UpdateRecord(updatedRecords);

				List<Record> allRecords = manager.SelectRecords(null);
				Assert.AreEqual(2, allRecords.Count);
				AssertRecords(r1, allRecords[0]);
				AssertRecords(r2, allRecords[1]);
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

		[TestMethod]
		private void TestAddIndex()
		{
			try
			{
				_logger.Message("Testing AddIndex");
				//making the table
				String indexPath = GetFilePath.TableColumnIndex(dbName, tableName, "Int");

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

				List<String> l2 = new List<string>();
				l2.Add("2048000");
				l2.Add("5.2");
				l2.Add("random2");
				Record r2 = new Record(l2);

				List<String> l3 = new List<string>();
				l3.Add("-409600");
				l3.Add("5.3");
				l3.Add("random3");
				Record r3 = new Record(l3);

				manager.InsertRecord(r1);
				manager.InsertRecord(r2);
				manager.InsertRecord(r3);

				manager.AddIndex(new Column(Column.DataType.Int, "Int", 100));

				Assert.IsTrue(File.Exists(indexPath));
				BtreeDictionary<Index<int>, int> btree = (BtreeDictionary<Index<int>, int>) Converter.FileToObject(indexPath);
				Assert.AreEqual(3, btree.Count);
				Assert.IsTrue(btree.ContainsKey(new Index<int>(2048000, 54)));
				Assert.IsTrue(btree.ContainsValue(96));
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

		[TestMethod]
		private void TestSelectRecordsOnIndex()
		{
			try
			{
				_logger.Message("Testing SelectRecordsOnIndex");
				//making the table
				Column indexedColumn = new Column(Column.DataType.Int, "Int", 100);
				String indexPath = GetFilePath.TableColumnIndex(dbName, tableName, indexedColumn.Name);

				List<Column> cols = new List<Column>();
				cols.Add(indexedColumn);
				cols.Add(new Column(Column.DataType.Double, "Double", 1));
				cols.Add(new Column(Column.DataType.Char, "String", 20));
				manager.CreateTable(dbName, tableName, cols);

				//making records to be inserted
				List<String> l1 = new List<string>();
				l1.Add("5");
				l1.Add("5.1");
				l1.Add("random1");
				Record r1 = new Record(l1);

				List<String> l2 = new List<string>();
				l2.Add("2048000");
				l2.Add("5.2");
				l2.Add("random2");
				Record r2 = new Record(l2);

				List<String> l3 = new List<string>();
				l3.Add("-409600");
				l3.Add(null);
				l3.Add("random3");
				Record r3 = new Record(l3);

				manager.InsertRecord(r1);
				manager.InsertRecord(r2);
				manager.InsertRecord(r3);

				manager.AddIndex(indexedColumn);

				List<Record> records = manager.SelectRecordsOnIndex(new Condition(indexedColumn, Condition.ConditionType.Equal, "5"));
				Assert.AreEqual(1, records.Count);
				AssertRecords(r1, records[0]);

				records = manager.SelectRecordsOnIndex(new Condition(indexedColumn, Condition.ConditionType.Less, "100"));
				Assert.AreEqual(2, records.Count);
				AssertRecords(r3, records[0]);
				AssertRecords(r1, records[1]);

				manager.AddIndex(new Column(Column.DataType.Double, "Double", 1));

				records =
					manager.SelectRecordsOnIndex(new Condition(new Column(Column.DataType.Double, "Double", 1),
						Condition.ConditionType.GreaterEqual, "5.2"));
				Assert.AreEqual(1, records.Count);
				AssertRecords(r2, records[0]);

				manager.AddIndex(new Column(Column.DataType.Char, "String", 20));

				manager.AddIndex(new Column(Column.DataType.Char, "String", 20));
				records =
					manager.SelectRecordsOnIndex(new Condition(new Column(Column.DataType.Char, "String", 20),
						Condition.ConditionType.Equal, "random2"));
				Assert.AreEqual(1, records.Count);
				AssertRecords(r2, records[0]);
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

		[TestMethod]
		private void TestInsertRecordsOnIndex()
		{
			try
			{
				_logger.Message("Testing InsertRecordsOnIndex");
				//making the table
				Column indexedColumn = new Column(Column.DataType.Int, "Int", 100);
				String indexPath = GetFilePath.TableColumnIndex(dbName, tableName, indexedColumn.Name);

				List<Column> cols = new List<Column>();
				cols.Add(indexedColumn);
				cols.Add(new Column(Column.DataType.Double, "Double", 1));
				cols.Add(new Column(Column.DataType.Char, "String", 20));
				manager.CreateTable(dbName, tableName, cols);

				//making records to be inserted
				List<String> l1 = new List<string>();
				l1.Add("5");
				l1.Add("5.1");
				l1.Add("random1");
				Record r1 = new Record(l1);

				List<String> l2 = new List<string>();
				l2.Add("2048000");
				l2.Add("5.2");
				l2.Add("random2");
				Record r2 = new Record(l2);

				List<String> l3 = new List<string>();
				l3.Add("-409600");
				l3.Add("5.3");
				l3.Add("random3");
				Record r3 = new Record(l3);

				manager.InsertRecord(r1);
				manager.InsertRecord(r2);

				manager.AddIndex(indexedColumn);
				manager.AddIndex(new Column(Column.DataType.Char, "String", 20));

				List<Record> records = manager.SelectRecordsOnIndex(new Condition(indexedColumn, Condition.ConditionType.Less, "0"));
				Assert.AreEqual(0, records.Count);

				manager.InsertRecord(r3);
				manager.InsertRecordToIndices(r3, 96);

				records = manager.SelectRecordsOnIndex(new Condition(indexedColumn, Condition.ConditionType.Less, "0"));
				Assert.AreEqual(1, records.Count);
				AssertRecords(r3, records[0]);

				records =
					manager.SelectRecordsOnIndex(new Condition(new Column(Column.DataType.Char, "String", 20),
						Condition.ConditionType.Equal, "random3"));
				Assert.AreEqual(1, records.Count);
				AssertRecords(r3, records[0]);
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

		[TestMethod]
		private void TestDeleteRecordsOnIndex()
		{
			try
			{
				_logger.Message("Testing DeleteRecordsOnIndex");
				//making the table
				Column indexedColumn = new Column(Column.DataType.Int, "Int", 100);
				String indexPath = GetFilePath.TableColumnIndex(dbName, tableName, indexedColumn.Name);

				List<Column> cols = new List<Column>();
				cols.Add(indexedColumn);
				cols.Add(new Column(Column.DataType.Double, "Double", 1));
				cols.Add(new Column(Column.DataType.Char, "String", 20));
				manager.CreateTable(dbName, tableName, cols);

				//making records to be inserted
				List<String> l1 = new List<string>();
				l1.Add("5");
				l1.Add("5.1");
				l1.Add("random1");
				Record r1 = new Record(l1);

				List<String> l2 = new List<string>();
				l2.Add("2048000");
				l2.Add("5.2");
				l2.Add("random2");
				Record r2 = new Record(l2);

				List<String> l3 = new List<string>();
				l3.Add("-409600");
				l3.Add("5.3");
				l3.Add("random3");
				Record r3 = new Record(l3);

				manager.InsertRecord(r1);
				manager.InsertRecord(r2);
				manager.InsertRecord(r3);

				manager.AddIndex(indexedColumn);

				List<Record> records = manager.SelectRecordsOnIndex(new Condition(indexedColumn, Condition.ConditionType.Equal, "5"));
				Assert.AreEqual(1, records.Count);
				AssertRecords(r1, records[0]);

				records = manager.SelectRecordsOnIndex(new Condition(indexedColumn, Condition.ConditionType.Less, "100"));
				Assert.AreEqual(2, records.Count);
				AssertRecords(r3, records[0]);
				AssertRecords(r1, records[1]);

				records = manager.SelectRecordsOnIndex(new Condition(indexedColumn, Condition.ConditionType.GreaterEqual, "5"));
				Assert.AreEqual(2, records.Count);
				AssertRecords(r1, records[0]);
				AssertRecords(r2, records[1]);

				Dictionary<int, Record> dict = new Dictionary<int, Record>();
				dict.Add(54, r2);
				dict.Add(12, r1);

				manager.DeleteRecordsFromIndices(dict);

				records = manager.SelectRecordsOnIndex(new Condition(indexedColumn, Condition.ConditionType.Less, "10"));
				Assert.AreEqual(1, records.Count);
				AssertRecords(r3, records[0]);
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

		[TestMethod]
		private void TestUpdateRecordsOnIndex()
		{
			try
			{
				_logger.Message("Testing UpdateRecordsOnIndex");
				//making the table
				Column indexedColumn = new Column(Column.DataType.Int, "Int", 100);
				String indexPath = GetFilePath.TableColumnIndex(dbName, tableName, indexedColumn.Name);

				List<Column> cols = new List<Column>();
				cols.Add(indexedColumn);
				cols.Add(new Column(Column.DataType.Double, "Double", 1));
				cols.Add(new Column(Column.DataType.Char, "String", 20));
				manager.CreateTable(dbName, tableName, cols);

				//making records to be inserted
				List<String> l1 = new List<string>();
				l1.Add("5");
				l1.Add("5.1");
				l1.Add("random1");
				Record r1 = new Record(l1);

				List<String> l2 = new List<string>();
				l2.Add("2048000");
				l2.Add("5.2");
				l2.Add("random2");
				Record r2 = new Record(l2);

				List<String> l3 = new List<string>();
				l3.Add("-409600");
				l3.Add("5.3");
				l3.Add("random3");
				Record r3 = new Record(l3);

				List<String> l4 = new List<string>();
				l4.Add("2048");
				l4.Add("5.2");
				l4.Add("random2");
				Record r4 = new Record(l4);

				manager.InsertRecord(r1);
				manager.InsertRecord(r2);
				manager.InsertRecord(r3);

				manager.AddIndex(indexedColumn);
				manager.AddIndex(new Column(Column.DataType.Char, "String", 20));

				Dictionary<int, Record> oldDict = new Dictionary<int, Record>();
				oldDict.Add(54, r2);
				Dictionary<int, Record> newDict = new Dictionary<int, Record>();
				newDict.Add(54, r4);

				manager.UpdateRecord(newDict);
				manager.UpdateRecordToIndices(oldDict, newDict);

				List<Record> records =
					manager.SelectRecordsOnIndex(new Condition(indexedColumn, Condition.ConditionType.Equal, "2048000"));
				Assert.AreEqual(0, records.Count);

				records =
					manager.SelectRecordsOnIndex(new Condition(new Column(Column.DataType.Char, "String", 20),
						Condition.ConditionType.Equal, "random2"));
				Assert.AreEqual(1, records.Count);
				AssertRecords(r4, records[0]);
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
			TestConstructor();
			TestCreateDropTable();
			TestUpdateTableToFile();
			TestInsertRecord();
			TestSelectRecords();
			TestDeleteRecords();
			TestUpdateRecords();
			TestAddIndex();
			TestSelectRecordsOnIndex();
			TestInsertRecordsOnIndex();
			TestDeleteRecordsOnIndex();
			TestUpdateRecordsOnIndex();
			cleanUp();

			_logger.Close();
		}
	}
}