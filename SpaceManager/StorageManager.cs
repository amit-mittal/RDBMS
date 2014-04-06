using System;
using System.Collections.Generic;
using System.IO;

/**
 * Interacts with the OS File System
 */
namespace RDBMS.SpaceManager
{
	class StorageManager
	{
		/**
		 * Record Size
		 * End of file address
		 * if bitmap exists or not
		 */
		public readonly int HeaderSize = 12;//fixed header size of each file
		

		public int GetRecordSize(Stream fs)
		{
			byte[] buffer;
			buffer = new byte[sizeof(int)];
			fs.Seek(0, SeekOrigin.Begin);
			fs.Read(buffer, 0, sizeof(int));
			return BitConverter.ToInt32(buffer, 0);
		}

		public int GetEndOfFile(Stream fs)
		{
			const int size = sizeof (int);
			byte[] buffer = new byte[size];
			fs.Seek(size, SeekOrigin.Begin);
			fs.Read(buffer, 0, size);
			return BitConverter.ToInt32(buffer, 0);
		}

		public int GetIfBitmapExists(Stream fs)
		{
			const int size = sizeof(int);
			byte[] buffer = new byte[size];
			fs.Seek(2 * size, SeekOrigin.Begin);
			fs.Read(buffer, 0, size);
			return BitConverter.ToInt32(buffer, 0);
		}

		public int GetSizeOfFile(Stream fs)
		{
			const int size = sizeof(int);
			byte[] buffer = new byte[size];
			fs.Seek(size, SeekOrigin.Begin);
			fs.Read(buffer, 0, size);
			return BitConverter.ToInt32(buffer, 0) - HeaderSize;
		}
		
		public void CreateFile(string fileName, int recordLength, bool bitmap)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
			{
				byte[] recordBytes = BitConverter.GetBytes(recordLength);
				fs.Write(recordBytes, 0, recordBytes.Length);
				fs.Write(BitConverter.GetBytes(3*recordBytes.Length), 0, recordBytes.Length);
				if(bitmap)
					fs.Write(BitConverter.GetBytes(1), 0, recordBytes.Length);
				else
					fs.Write(BitConverter.GetBytes(0), 0, recordBytes.Length);
				fs.Close();	
			}
		}

		public void DropFile(String fileName)
		{
			File.Delete(fileName);
		}

		public void CreateFolder(String folderName)
		{
			Directory.CreateDirectory(folderName);
		}

		public void DropFolder(String folderName)
		{
			Directory.Delete(folderName);
		}

		public int Allocate(Stream fs)
		{
			//reads from bitmap
			return 1;
		}

		public void Deallocate(Stream fs, List<int> address)
		{
			//simply add entry to bitmap
		}

		public byte[] Read(Stream fs, int address, int count)
		{
			int recordSize = GetRecordSize(fs);
			byte[] buffer = new byte[count * recordSize];
			fs.Seek(address, SeekOrigin.Begin);
			fs.Read(buffer, 0, count * recordSize);
			return buffer;
		}

		public void Write(Stream fs, int address, byte[] record)
		{
			fs.Seek(address, SeekOrigin.Begin);
			fs.Write(record, 0, record.Length);
		}

		public byte[] GetCompleteFile(Stream fs)
		{
			int size = GetSizeOfFile(fs);
			int recordSize = GetRecordSize(fs);
			byte[] buffer = Read(fs, HeaderSize, size / recordSize);

			return buffer;
		}
	}
}
