using System;
using System.IO;

/**
 * Interacts with the OS File System
 */
namespace RDBMS.SpaceManager
{
	class StorageManager
	{
		private int GetRecordSize(FileStream fs)
		{
			byte[] buffer;
			buffer = new byte[sizeof(int)];
			fs.Read(buffer, 0, sizeof(int));
			return BitConverter.ToInt32(buffer, 0);
		}

		private int GetEndOfFile(FileStream fs)
		{
			int size = sizeof (int);
			byte[] buffer;
			buffer = new byte[size];
			fs.Read(buffer, size, size);
			return BitConverter.ToInt32(buffer, 0);
		}
		
		public void CreateFile(String fileName, int recordLength)
		{
			try
			{
				using (FileStream fs = new FileStream(fileName, FileMode.Append))
				{
					byte[] recordBytes = BitConverter.GetBytes(recordLength);
					fs.Write(recordBytes, 0, recordBytes.Length);
					fs.Write(BitConverter.GetBytes(2*recordLength), 0, recordBytes.Length);
					fs.Close();	
				}
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public void DropFile(String fileName)
		{
			try
			{
				File.Delete(fileName);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public void CreateFolder(String folderName)
		{
			try
			{
				Directory.CreateDirectory(folderName);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public void DropFolder(String folderName)
		{
			try
			{
				Directory.Delete(folderName);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public int Allocate(String fileName)
		{
			//reads from bitmap
			return 1;
		}

		public void Deallocate(String fileName, int address)
		{
			//simply add entry to bitmap
		}

		public byte[] Read(String fileName, int address)
		{
			FileStream fs = File.OpenRead(fileName);
			int recordSize = GetRecordSize(fs);
			byte[] buffer = new byte[recordSize];
			fs.Read(buffer, address, recordSize);
			fs.Close();
			return buffer;
		}

		public void Write(String fileName, int address, byte[] record)
		{
			FileStream fs = File.OpenRead(fileName);
			int recordSize = GetRecordSize(fs);
			fs.Write(record, address, recordSize);
			fs.Close();
		}
	}
}
