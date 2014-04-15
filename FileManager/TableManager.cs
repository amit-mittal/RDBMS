using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kw.Data;
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
				table = (Table) Converter.FileToObject(conf);
			}
		}

		/**
		 * Creates all the folders/files related to a new table
		 * IMPORTANT - Presently assumed indexColumns would be empty
		 * Index can be added on later using AddIndex
		 */
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
			//TODO insert into btree also - similarly update/delete/select
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
		 * Does Linear Search of all the records
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
		 * @returns Dictionary<key, Record>
		 * key => address
		 * Record => record corresponding to the address
		 * which satisfies the given condition
		 * 
		 * Does searching in the btree
		 * Call this function only if that column is an index
		 * @param name="condition" NotNullable
		 */
		public Dictionary<int, Record> GetAddressRecordDictOnIndex(Condition condition)
		{
			//todo test pending
			//initializing the variables
			Dictionary<int, Record> finalDict = new Dictionary<int, Record>();
			List<int> addresses = new List<int>();

			int index = -1;//means unitialized
			if (condition != null)
				index = table.GetColumnIndex(condition.Attribute);
			if (index == -1 || condition == null)//not feasible columns
				return finalDict;

			String indexFile = GetFilePath.TableColumnIndex(table.DbName, table.Name, condition.Attribute.Name);
			String recordsFile = GetFilePath.TableRecords(table.DbName, table.Name);

			//getting the b+ tree into the main memory
			BtreeDictionary<Index<int>, int> intbptree;
			//TODO implementation of other types still left

			//traversing the whole btree
			if (condition.Attribute.Type == Column.DataType.Int)
			{
				//getting tree into main memory
				intbptree = (BtreeDictionary<Index<int>, int>)Converter.FileToObject(indexFile);
				
				//initialized possible feasible range of the records
				IEnumerable range;
				if (condition.Sign == Condition.ConditionType.Equal)
				{
					range = intbptree.BetweenKeys(new Index<int>(int.Parse(condition.Value), int.MinValue),
						new Index<int>(int.Parse(condition.Value), int.MaxValue));
				}
				else if (condition.Sign == Condition.ConditionType.Less || condition.Sign == Condition.ConditionType.LessEqual)
				{
					range = intbptree.BetweenKeys(new Index<int>(int.MinValue, int.MinValue),
						new Index<int>(int.Parse(condition.Value), int.MaxValue));
				}
				else
				{
					range = intbptree.BetweenKeys(new Index<int>(int.Parse(condition.Value), int.MinValue),
						new Index<int>(int.MaxValue, int.MaxValue));	
				}

				//initialized the selected addresses list
				foreach (KeyValuePair<Index<int>, int> pair in range)
				{
					if (IsColumnValid(pair.Key.Key.ToString(), condition, index))
						addresses.Add(pair.Value);
				}

				//getting all records corresponding to the addresses list
				Stream fs = new FileStream(recordsFile, FileMode.OpenOrCreate);
				foreach (int address in addresses)
				{
					byte[] recordsBytes = storageManager.Read(fs, address);
					Record record = table.StringToRecord(Converter.BytesToChar(recordsBytes).ToString());
					finalDict.Add(address, record);
				}
			}
			
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
		 * Adding index to particular column
		 * if some records were already there
		 */
		public void AddIndex(Column index)
		{
			//todo test pending
			String indexFile = GetFilePath.TableColumnIndex(table.DbName, table.Name, index.Name);
			Dictionary<int, Record> allRecords = GetAddressRecordDict(null);
			int position = table.GetColumnIndex(index);

			BtreeDictionary<Index<int>, int> intbptree = new BtreeDictionary<Index<int>, int>();
			BtreeDictionary<Index<String>, int> stringbptree = new BtreeDictionary<Index<String>, int>();
			BtreeDictionary<Index<double>, int> doublebptree = new BtreeDictionary<Index<double>, int>();

			if (index.Type == Column.DataType.Int)
			{
				//making a b+tree
				foreach (KeyValuePair<int, Record> keyValuePair in allRecords)
				{
					String s = keyValuePair.Value.Fields[position];
					if (s != null)
					{
						Index<int> idx = new Index<int>(int.Parse(s), keyValuePair.Key);
						intbptree.Add(idx, keyValuePair.Key);
					}
					else
					{
						Index<int> idx = new Index<int>(Constants.NullAsInt, keyValuePair.Key);
						intbptree.Add(idx, keyValuePair.Key);
					}
				}

				//writing b+tree to the file
				Converter.ObjectToFile(intbptree, indexFile);
			}
			else if (index.Type == Column.DataType.Double)
			{
				//TODO implementation left
				throw new NotImplementedException("Double not implemented");
			}
			else if (index.Type == Column.DataType.Char)
			{
				//TODO implementation left
				throw new NotImplementedException("String not implemented");
			}
		}
		
		/**
		 * Tells if the record satisfies the condition or not
		 * @param name="index" column index on which condition is applied
		 */
		private bool IsRecordValid(Record record, Condition condition, int index)
		{
			String value = record.Fields[index];
			return IsColumnValid(value, condition, index);
		}

		/**
		 * Tells if the column value satisfies the condition or not
		 * @param name="index" column index on which condition is applied
		 */
		private bool IsColumnValid(String value, Condition condition, int index)
		{
			if (value == null)
			{
				return false;
			}

			if (table.Columns[index].Type == Column.DataType.Int)
			{
				if (value.Length == 0)
					return false;
				return condition.CompareIntegers(int.Parse(value));
			}

			if (table.Columns[index].Type == Column.DataType.Double)
			{
				if (value.Length == 0)
					return false;
				return condition.CompareDoubles(double.Parse(value));
			}

			if (table.Columns[index].Type == Column.DataType.Char)
			{
				return condition.CompareStrings(value);
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
