using System;
using System.Collections.Generic;
using System.IO;
using RDBMS.DataStructure;
using RDBMS.SpaceManager;
using RDBMS.Util;

namespace RDBMS.FileManager
{
	/**
	 * Handles queries related to the table
	 * 
	 * Tests in TableManagerTest.cs
	 */
	class TableManager
	{
		public Table table;
		public StorageManager storageManager;

		public TableManager()
		{
			storageManager = new StorageManager();
		}

		/**
		 * This constructor would be used to init the conf
		 * of table
		 * Will be needed when using select, insert, etc.
		 */
		public TableManager(String dbName, String tableName)
		{
			String path = GetFilePath.Table(dbName, tableName);
			String conf = GetFilePath.TableConf(dbName, tableName);
			if (!Directory.Exists(path))
			{
				throw new Exception("Table does not exist");
			}
			else
			{
				storageManager = new StorageManager();
				byte[] confBytes = storageManager.GetCompleteFile(File.OpenRead(conf));
				table = (Table)Converter.BytesToObject(confBytes);
			}
		}

		public void CreateTable(String dbName, String tableName, List<Column> columns)
		{
			String path = GetFilePath.Table(dbName, tableName);
			String conf = GetFilePath.TableConf(dbName, tableName);
			String records = GetFilePath.TableRecords(dbName, tableName);
			if (Directory.Exists(path))
			{
				throw new Exception("Table already exists");
			}
			else
			{
				table = new Table(dbName, tableName, columns);
				storageManager.CreateFolder(path);//table folder
				storageManager.CreateFile(conf, Converter.ObjectToBytes(table).Length, false);//schema file of table
				byte[] record = Converter.CharToBytes(new char[table.GetSizeOfRecordArray()]);
				storageManager.CreateFile(records, record.Length, true);//records file of table
			}
		}

		public void DropTable(String dbName, String tableName)
		{
			String path = GetFilePath.Table(dbName, tableName);
			if (!Directory.Exists(path))
			{
				throw new Exception("Table does not exist");
			}
			else
			{
				storageManager.DropFolder(path);
			}
		}

		public void InsertRecord(Record record)
		{
			//checking if some error is in the field values
			for (int i = 0; i < record.Fields.Count; i++)
			{
				String value = record.Fields[i];
				if (table.Columns[i].Type == Column.DataType.Int)
				{
					int dummy;
					if (value != null && int.TryParse(value, out dummy) == false)
					{
						throw new Exception((i+1) + " parameter expects an integer");
					}
				}
				else if (table.Columns[i].Type == Column.DataType.Double)
				{
					double dummy;
					if (value != null && double.TryParse(value, out dummy) == false)
					{
						throw new Exception((i + 1) + " parameter expects a double");
					}
				}
				else if (table.Columns[i].Type == Column.DataType.Char)
				{
					if (value != null && value.Length>table.Columns[i].Length)
					{
						throw new Exception((i + 1) + " parameter has length more than alloted to it");
					}
				}
			}

			//inserting the record into the table
			String recordsPath = GetFilePath.TableRecords(table.DbName, table.Name);
			Stream fs = new FileStream(recordsPath, FileMode.OpenOrCreate);
			int address = storageManager.Allocate(recordsPath, fs);
			char[] recordStream = table.RecordToCharArray(record);
			storageManager.Write(fs, address, Converter.CharToBytes(recordStream));
			fs.Close();
		}

		public void DeleteRecord()
		{
			
		}
	}
}
