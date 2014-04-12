using System;
using System.Collections.Generic;
using System.IO;
using RDBMS.Util;


//TODO : Use Converter class.
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
			buffer = new byte[sizeof(Int32)];
			fs.Seek(0, SeekOrigin.Begin);
			fs.Read(buffer, 0, sizeof(Int32));
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

		public void SetEndOfFile(Stream fs, int EndOfFile)
		{
			byte[] NewEndOfFile = BitConverter.GetBytes(EndOfFile);
			fs.Seek(sizeof(Int32), SeekOrigin.Begin);
			fs.Write(NewEndOfFile, 0, NewEndOfFile.Length);
		}

		public int GetIfBitmapExists(Stream fs)
		{
			const int size = sizeof(Int32);
			byte[] buffer = new byte[size];
			fs.Seek(2 * size, SeekOrigin.Begin);
			fs.Read(buffer, 0, size);
			return BitConverter.ToInt32(buffer, 0);
		}

		public int GetSizeOfFile(Stream fs)
		{
			const int size = sizeof(Int32);
			byte[] buffer = new byte[size];
			fs.Seek(size, SeekOrigin.Begin);
			fs.Read(buffer, 0, size);
			return BitConverter.ToInt32(buffer, 0) - HeaderSize;
		}

        public bool IsFileEmpty(Stream fs)
        {
            if (GetEndOfFile(fs) == HeaderSize)
                return true;
            return false;
        }

		public void CreateFile(string fileName, int recordLength, bool bitmap)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
			{
				byte[] recordBytes = BitConverter.GetBytes(recordLength);
				fs.Write(recordBytes, 0, recordBytes.Length);
				fs.Write(BitConverter.GetBytes(3*recordBytes.Length), 0, recordBytes.Length);
                if (bitmap)
                {
                    fs.Write(BitConverter.GetBytes(1), 0, recordBytes.Length);
                    CreateFile(fileName + " - BitMap", sizeof(Int32), false);
                }
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
			Directory.Delete(folderName, true);
		}

        //If bitmap exists, then it reads and returns a suitable address to write. Otherwise, end-of-file
		public int Allocate(String fileName, Stream fs)
		{
			int address;
            if (GetIfBitmapExists(fs) == 0)   //No bitmap
			{
				address = GetEndOfFile(fs);
				SetEndOfFile(fs, address + GetRecordSize(fs));
				return address;
			}
      
            else 
            {
                using(FileStream fsBitMap = new FileStream(fileName + " - BitMap", FileMode.Open))
                {
                    if (IsFileEmpty(fsBitMap))  //Bitmap exists but is empty
					{
						address = GetEndOfFile(fs);
						SetEndOfFile(fs, address + GetRecordSize(fs));
						return address;
					}
						                  
                    else 
                    { 
						int NewEndOfFile = GetEndOfFile(fsBitMap) - GetRecordSize(fsBitMap);
						byte[] buffer = Read(fsBitMap, NewEndOfFile, 1);
						SetEndOfFile(fsBitMap, NewEndOfFile);
						return BitConverter.ToInt32(buffer, 0);
                    }				
                }	
            }
		}

		//Random-Access Deallocation - For files WITH bitmap
		public void Deallocate(String fileName, List<int> address)
		{
			//simply add entry to bitmap and update end-of-file in header
            using (FileStream fs = new FileStream(fileName + " - BitMap", FileMode.Open))
            {
				int recordSize = GetRecordSize(fs);
                fs.Seek(GetEndOfFile(fs), SeekOrigin.Begin);
                foreach (int addr in address)
                {
                    fs.Write(BitConverter.GetBytes(addr), 0, recordSize);
                }
				int NewEndOfFile = GetEndOfFile(fs) + (address.Count * recordSize);
				SetEndOfFile(fs, NewEndOfFile);
            }
		}

		//For files WITHOUT bitmap
		public void Deallocate(Stream fs, int CountRecords)
		{
			int NewEndOfFile = GetEndOfFile(fs) - (CountRecords * GetRecordSize(fs));
			try
			{
				if (NewEndOfFile < HeaderSize)
				{
					fs.Close();
					throw new Exception("Number of records to deallocate exceeds number of records present in file");
				}
			}

			finally
			{
				SetEndOfFile(fs, NewEndOfFile);
			}
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
