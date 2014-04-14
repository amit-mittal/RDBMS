using System;
using System.Collections;
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
			//TODO test pending
			String path = GetFilePath.Table(dbName, tableName);
			String conf = GetFilePath.TableConf(dbName, tableName);
			if (!Directory.Exists(path))
			{
				throw new Exception("Table does not exist");
			}
			else
			{
				storageManager = new StorageManager();
				table = (Table) Converter.FileToObject(conf);
			}
		}

		public void CreateTable(string dbName, string tableName, List<Column> columns, List<Column> indexColumns)
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
				table = new Table(dbName, tableName, columns, indexColumns);
				storageManager.CreateFolder(path);//table folder
				Converter.ObjectToFile(table, conf);//schema file of table
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

		public Table DescribeTable()
		{
			return table;
		}

		/**
		 * Inserts a record into the table
		 * Those fields should be null which have not been set
		 */
		public void InsertRecord(Record record)
		{
			String recordsPath = GetFilePath.TableRecords(table.DbName, table.Name);
			Stream fs = new FileStream(recordsPath, FileMode.OpenOrCreate);
			int address = storageManager.Allocate(recordsPath, fs);
			char[] recordStream = table.RecordToCharArray(record);
			storageManager.Write(fs, address, Converter.CharToBytes(recordStream));
			fs.Close();
		}

		/**
		 * @returns Dictionary<key, Record>
		 * key => address
		 * Record => record corresponding to the address
		 * which satisfies the given condition
		 * 
		 * if condition is null means select all records
		 */
		public Dictionary<int, Record> GetAddressRecordDict(Condition condition)
		{
			int index = -1;//means unitialized
			if (condition != null)
			{
				index = table.GetColumnIndex(condition.Attribute);
				if (index == -1)//no column matched
					return new Dictionary<int, Record>();
			}

			//initializing the variables
			Dictionary<int, Record> finalDict = new Dictionary<int, Record>();
			Stream fs = new FileStream(GetFilePath.TableRecords(table.DbName, table.Name), FileMode.OpenOrCreate);
			int recordSize = storageManager.GetRecordSize(fs);
			int endOfFile = storageManager.GetEndOfFile(fs);

			//getting the whole bitmap here so that we know there is no record there
			Stream bitmapFs = new FileStream(GetFilePath.TableRecordsBitmap(table.DbName, table.Name), FileMode.OpenOrCreate);
			List<int> bitmapList = Converter.BytesToIntList(storageManager.GetCompleteFile(bitmapFs));
			HashSet<int> bitmapSet = new HashSet<int>(bitmapList);
			bitmapFs.Close();

			//traversing the whole file
			int NumRecords = Constants.MaxSelectRecords;//TODO use this if read more than a chunk
			for (int offset = storageManager.HeaderSize; offset < endOfFile; offset += recordSize)
			{
				if (!bitmapSet.Contains(offset))
				{
					char[] recordStr = new char[recordSize];
					Converter.BytesToChar(storageManager.Read(fs, offset)).CopyTo(recordStr, 0);

					Record record = table.StringToRecord(new string(recordStr));
					if (condition == null)
					{
						finalDict.Add(offset, record);
					}
					else if (IsRecordValid(record, condition, index))
					{
						finalDict.Add(offset, record);
					}
				}
			}
			fs.Close();
			return finalDict;
		}

		/**
		 * Gets the dictionary containing the address => updated record
		 */
		public void UpdateRecord(Dictionary<int, Record> updatedRecords)
		{
			String recordsPath = GetFilePath.TableRecords(table.DbName, table.Name);
			Stream fs = new FileStream(recordsPath, FileMode.OpenOrCreate);
			foreach (int address in updatedRecords.Keys)
			{
				char[] recordStream = table.RecordToCharArray(updatedRecords[address]);
				storageManager.Write(fs, address, Converter.CharToBytes(recordStream));
			}
			fs.Close();
		}

		/**
		 * Gets the dictionary containing the address => to be deleted record
		 */
		public void DeleteRecords(Dictionary<int, Record> uselessRecords)
		{
			String recordsPath = GetFilePath.TableRecords(table.DbName, table.Name);
			Stream fs = new FileStream(recordsPath, FileMode.OpenOrCreate);
			List<int> addresses = new List<int>(uselessRecords.Keys);
			storageManager.Deallocate(recordsPath, addresses);
			fs.Close();
		}

		/**
		 * @returns List<Record> 
		 * which satisfies the given condition
		 * 
		 * if condition is null means select all records
		 */
		public List<Record> SelectRecords(Condition condition)
		{
			Dictionary<int, Record> selectedRecords = GetAddressRecordDict(condition);
			return new List<Record>(selectedRecords.Values);
		}
		
		/**
		 * Tells if the record satisfies the condition or not
		 * @param name="index" column index on which condition is applied
		 */
		private bool IsRecordValid(Record record, Condition condition, int index)
		{
			String value = record.Fields[index];
			if (value == null)
			{
				return false;
			}
				
			if (table.Columns[index].Type == Column.DataType.Int)
			{
				if(value.Length == 0)
					return false;
				return condition.CompareIntegers(int.Parse(value));
			}

			if (table.Columns[index].Type == Column.DataType.Double)
			{
				if (value.Length == 0)
					return false;
				return condition.CompareDoubles(double.Parse(record.Fields[index]));
			}

			if (table.Columns[index].Type == Column.DataType.Char)
			{
				return condition.CompareStrings(record.Fields[index]);
			}
				
			return false;
		}

		/**
		 * Checks if record contains some error or not
		 */
		public void CheckRecord(Record record)
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
						throw new Exception((i + 1) + " parameter expects an integer");
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
				}

				if (value != null && value.Length > table.Columns[i].Length)
				{
					throw new Exception((i + 1) + " parameter has length more than alloted to it");
				}
			}
		}
	}
}
