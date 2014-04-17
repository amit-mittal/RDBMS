using System;
using System.Collections.Generic;
using System.IO;
using RDBMS.Util;

/**
 * Interacts with the OS File System
 */

namespace RDBMS.SpaceManager
{
	internal class StorageManager
	{
		/** 
		 * Order of elements in the Header of each file :
		 * Record Size (4 Bytes)
		 * End-of-file address (4 Bytes)
		 * If bitmap exists or not - 1 for true / 0 for false (4 Bytes)
		 */
		public readonly int HeaderSize = 12; //fixed header size of each file

		// Returns the record size of the file from the Header
		public int GetRecordSize(Stream fs)
		{
			byte[] buffer;
			buffer = new byte[sizeof (Int32)];
			fs.Seek(0, SeekOrigin.Begin);
			fs.Read(buffer, 0, sizeof (Int32));
			return Converter.BytesToInt(buffer);
		}

		// Returns the End-of-file from the Header. This includes the Header size too
		public int GetEndOfFile(Stream fs)
		{
			const int size = sizeof (int);
			byte[] buffer = new byte[size];
			fs.Seek(size, SeekOrigin.Begin);
			fs.Read(buffer, 0, size);
			return Converter.BytesToInt(buffer);
		}

		// Sets the end-of-file to the desired value in the Header
		public void SetEndOfFile(Stream fs, int EndOfFile)
		{
			byte[] NewEndOfFile = Converter.IntToBytes(EndOfFile);
			fs.Seek(sizeof (Int32), SeekOrigin.Begin);
			fs.Write(NewEndOfFile, 0, NewEndOfFile.Length);
		}

		// To check if bitmap exists or not from the Header of the file concerned
		public int GetIfBitmapExists(Stream fs)
		{
			const int size = sizeof (Int32);
			byte[] buffer = new byte[size];
			fs.Seek(2*size, SeekOrigin.Begin);
			fs.Read(buffer, 0, size);
			return Converter.BytesToInt(buffer);
		}

		// Returns the amount of Data present in the file (excluding Header)
		public int GetSizeOfFile(Stream fs)
		{
			return GetEndOfFile(fs) - HeaderSize;
		}

		// To check if the contents of a file are empty i.e. if there is anything other than Header
		public bool IsFileEmpty(Stream fs)
		{
			if (GetEndOfFile(fs) == HeaderSize)
				return true;
			return false;
		}

		// To create a file with the required parameters. If bitmap is not required for a file, then bitmap value = false
		public void CreateFile(string fileName, int recordLength, bool bitmap)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.CreateNew))
			{
				byte[] recordBytes = Converter.IntToBytes(recordLength);
				fs.Write(recordBytes, 0, recordBytes.Length); // Write the Record size in the Header of file
				fs.Write(Converter.IntToBytes(3*recordBytes.Length), 0, recordBytes.Length);
				if (bitmap) // If bitmap is required for the current file
				{
					fs.Write(Converter.IntToBytes(1), 0, recordBytes.Length); //Write 1 for bitmap in the Header
					CreateFile(fileName + " - BitMap", sizeof (Int32), false); //Create a bitmap file with no bitmap
				}
				else // Bitmap is not required
					fs.Write(Converter.IntToBytes(0), 0, recordBytes.Length); //Write 0 for bitmap in the Header
				fs.Close();
			}
		}

		// For deleting a specific file
		public void DropFile(String fileName)
		{
			File.Delete(fileName);
		}

		// For creating a specific folder
		public void CreateFolder(String folderName)
		{
			Directory.CreateDirectory(folderName);
		}

		// For deleting a specific folder
		public void DropFolder(String folderName)
		{
			Directory.Delete(folderName, true);
		}

		// To allocate an address for writing a record. To be used in Higher layers.
		// If bitmap exists, then it reads and returns a suitable address to write. Otherwise, end-of-file address is returned
		public int Allocate(String fileName, Stream fs)
		{
			if (GetIfBitmapExists(fs) == 0) // No bitmap exists for the current file
				return GetEndOfFile(fs); // Return the end-of-file for writing

			else // Bitmap exists
			{
				using (FileStream fsBitMap = new FileStream(fileName + " - BitMap", FileMode.Open))
				{
					if (IsFileEmpty(fsBitMap)) //Bitmap exists but is empty
						return GetEndOfFile(fs);

					else
					{
						int NewEndOfFile = GetEndOfFile(fsBitMap) - GetRecordSize(fsBitMap);
						byte[] buffer = Read(fsBitMap, NewEndOfFile, 1); // Read from the end of bitmap file (1 record = address)
						SetEndOfFile(fsBitMap, NewEndOfFile); // Update the end-of-file of bitmap as the entry is removed
						return Converter.BytesToInt(buffer);
					}
				}
			}
		}

		// Random-Access Deallocation - For files WITH bitmap. 
		// Address List contains the list of addresses to be deallocated for future use
		/*NOTE : This function does NOT update the end-of-file in the Header of concerned file 
		 * even if address list contains the addresses of the last few records (in whatever order)*/

		public void Deallocate(String fileName, List<int> address) // We pass File name instead of stream to open bitmap file
		{
			// Simply add entry to bitmap and update end-of-file in Header
			using (FileStream fsBitMap = new FileStream(fileName + " - BitMap", FileMode.Open))
			{
				int recordSize = GetRecordSize(fsBitMap);
				fsBitMap.Seek(GetEndOfFile(fsBitMap), SeekOrigin.Begin); // Start adding entries to the end of bitmap file
				foreach (int addr in address)
				{
					fsBitMap.Write(Converter.IntToBytes(addr), 0, recordSize); // Write all entries in the list one-by-one
				}
				int NewEndOfFile = GetEndOfFile(fsBitMap) + (address.Count*recordSize);
				SetEndOfFile(fsBitMap, NewEndOfFile); // Update end-of-file of bitmap file as many entries have been added
			}
		}

		// For files WITHOUT bitmap
		// In this case, we deallocate #CountRecords records from the file. Just update end-of-file of concerned file
		public void Deallocate(Stream fs, int CountRecords)
		{
			int NewEndOfFile = GetEndOfFile(fs) - (CountRecords*GetRecordSize(fs)); // New end-of-file
			try
			{
				if (NewEndOfFile < HeaderSize) // This should not be allowed
				{
					throw new Exception("Number of records to deallocate exceeds number of records present in file");
				}
			}
			finally
			{
				SetEndOfFile(fs, NewEndOfFile); // Update end-of-file
			}
		}

		// To read #count records from the FileStream fs starting from 'address'
		// Default value of count = 1
		public byte[] Read(Stream fs, int address, int count = 1)
		{
			int recordSize = GetRecordSize(fs);
			byte[] buffer = new byte[count*recordSize]; // Allocate sufficient memory for buffer
			fs.Seek(address, SeekOrigin.Begin); // Seek to required address
			fs.Read(buffer, 0, count*recordSize); // Read all records from there
			return buffer;
		}

		// To write a record in the FileStream fs at 'address'
		public void Write(Stream fs, int address, byte[] record)
		{
			fs.Seek(address, SeekOrigin.Begin); // Seek to desired location
			fs.Write(record, 0, record.Length); // Write the record
			if (address >= GetEndOfFile(fs)) // If the record is written at the end or after it, update end-of-file in Header
				SetEndOfFile(fs, address + record.Length);
		}

		// To get the complete file as a byte-array (excluding Header)
		public byte[] GetCompleteFile(Stream fs)
		{
			int size = GetSizeOfFile(fs);
			int recordSize = GetRecordSize(fs);
			byte[] buffer = Read(fs, HeaderSize, size/recordSize); // Using the above defined Read function
			return buffer;
		}
	}
}