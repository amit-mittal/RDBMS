using System;
using System.Collections.Generic;
using System.IO;
using RDBMS.DataStructure;
using RDBMS.SpaceManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDBMS.Util;

//TODO:Use Converter Class
namespace RDBMS.Testing
{
	[TestClass]
	class StorageManagerTest
	{
		private static readonly StorageManager Manager = new StorageManager();
		private Logger _logger;
		private const String SampleFile = "E:\\Sample";
		private const String SampleFile_BM = "E:\\Sample_BM";
		private const String SampleFolder = "E:\\Sample";

		[TestMethod]
		private void TestCreateDropFile()
		{
			try
			{
				Manager.CreateFile(SampleFile, 10, false);
				Manager.CreateFile(SampleFile_BM, 7, true);
				Assert.IsTrue(File.Exists(SampleFile));
				Assert.IsTrue(File.Exists(SampleFile_BM));
				Assert.IsTrue(File.Exists(SampleFile_BM + " - BitMap"));
				Assert.AreEqual(File.ReadAllBytes(SampleFile).Length, Manager.HeaderSize);
				Assert.AreEqual(File.ReadAllBytes(SampleFile_BM).Length, Manager.HeaderSize);
				Assert.AreEqual(File.ReadAllBytes(SampleFile_BM + " - BitMap").Length, Manager.HeaderSize);

				Manager.DropFile(SampleFile);
				Manager.DropFile(SampleFile_BM);
				Manager.DropFile(SampleFile_BM + " - BitMap");
				Assert.IsTrue(!File.Exists(SampleFile));
				Assert.IsTrue(!File.Exists(SampleFile_BM));
				Assert.IsTrue(!File.Exists(SampleFile_BM + " - BitMap"));
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		[TestMethod]
		private void TestCreateDropFolder()
		{
			try
			{
				Manager.CreateFolder(SampleFolder);
				Assert.IsTrue(Directory.Exists(SampleFolder));

				Manager.DropFolder(SampleFile);
				Assert.IsTrue(!Directory.Exists(SampleFolder));
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		[TestMethod]
		private void TestRead()
		{
			try
			{
				Manager.CreateFile(SampleFile, 4, false);
				FileStream fs = File.OpenRead(SampleFile);
				int firstVal = BitConverter.ToInt32(Manager.Read(fs, 0, 1), 0);
				List<int> list = Converter.BytesToIntList(Manager.Read(fs, 4, 2));
				fs.Close();

				Assert.AreEqual(list.Count, 2);
				Assert.AreEqual(4, firstVal);
				Assert.AreEqual(Manager.HeaderSize, Convert.ToInt32(list[0]));
				Assert.AreEqual(0, Convert.ToInt32(list[1]));
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
			finally
			{
				Manager.DropFile(SampleFile);
			}
		}

		[TestMethod]
		private void TestWrite()
		{
			try
			{
				//Writing just an integer
				Manager.CreateFile(SampleFile, 4, false);
				Stream fs = new FileStream(SampleFile, FileMode.OpenOrCreate);
				Manager.Write(fs, Manager.HeaderSize, BitConverter.GetBytes(100));
				Assert.AreEqual(100, BitConverter.ToInt32(Manager.Read(fs, Manager.HeaderSize, 1), 0));
				fs.Close();
				Manager.DropFile(SampleFile);

				//Write a data structure
				Dummy col = new Dummy(Dummy.DataType.Int, "ColName".ToCharArray(), 10);
				byte[] colBytes = Converter.ObjectToBytes(col);
				Manager.CreateFile(SampleFile, colBytes.Length, false);
				fs = new FileStream(SampleFile, FileMode.OpenOrCreate);
				Manager.Write(fs, Manager.HeaderSize, colBytes);
				Dummy actualObj = (Dummy)Converter.BytesToObject(Manager.Read(fs, Manager.HeaderSize, 1));
				Assert.AreEqual(colBytes.Length, Manager.Read(fs, Manager.HeaderSize, 1).Length);
				fs.Close();

				Assert.AreEqual(col, actualObj);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
			finally
			{
				Manager.DropFile(SampleFile);
			}
		}

		[TestMethod]
		private void TestAllocate()
		{
			try
			{
				int address;
				Manager.CreateFile(SampleFile, 4, false);	//Without bitmap
				Manager.CreateFile(SampleFile_BM, 4, true);	//With bitmap
				Stream fs = new FileStream(SampleFile, FileMode.OpenOrCreate);
				Stream fsbm = new FileStream(SampleFile_BM, FileMode.OpenOrCreate);

				//Tests for Allocate
				Assert.AreEqual(Manager.Allocate(SampleFile, fs), Manager.GetEndOfFile(fs) - Manager.GetRecordSize(fs));
				Assert.AreEqual(Manager.Allocate(SampleFile_BM, fsbm), Manager.GetEndOfFile(fsbm) - Manager.GetRecordSize(fsbm));
				using (FileStream fsBitMap = new FileStream(SampleFile_BM + " - BitMap", FileMode.Open))
				{
					Manager.Write(fsBitMap, Manager.HeaderSize, BitConverter.GetBytes(16));
					Manager.SetEndOfFile(fsBitMap, Manager.HeaderSize + Manager.GetRecordSize(fsBitMap));
				}
				Assert.AreEqual(Manager.Allocate(SampleFile_BM, fsbm), 16);
				using (FileStream fsBitMap = new FileStream(SampleFile_BM + " - BitMap", FileMode.Open))
					Assert.AreEqual(Manager.GetEndOfFile(fsBitMap), Manager.HeaderSize);
				Manager.SetEndOfFile(fs, Manager.HeaderSize);
				Manager.SetEndOfFile(fsbm, Manager.HeaderSize);
				for (int i = 0; i < 5; i++)
				{
					address = Manager.Allocate(SampleFile_BM, fsbm);
					Assert.AreEqual(Manager.GetEndOfFile(fsbm) - Manager.GetRecordSize(fsbm), address);
					Manager.Write(fsbm, address, BitConverter.GetBytes(100 + i));
				}
				
				fs.Close();
				fsbm.Close();				
			}

			catch (Exception e)
			{
				_logger.Error(e.Message);
			}

			finally 
			{	
				Manager.DropFile(SampleFile);
				Manager.DropFile(SampleFile_BM);
				Manager.DropFile(SampleFile_BM + " - BitMap");
			}
		}

		[TestMethod]
		private void TestDeallocate()
		{
			try
			{
				Manager.CreateFile(SampleFile, 4, false);	//Without bitmap
				Manager.CreateFile(SampleFile_BM, 4, true);	//With bitmap
				Stream fs = new FileStream(SampleFile, FileMode.OpenOrCreate);
				Stream fsbm = new FileStream(SampleFile_BM, FileMode.OpenOrCreate);

				for (int i = 0; i < 5; i++)
				{
					Manager.Write(fsbm, Manager.Allocate(SampleFile_BM, fsbm), BitConverter.GetBytes(100 + i));
					Manager.Write(fs, Manager.Allocate(SampleFile, fs), BitConverter.GetBytes(500 + i));
				}

				//Tests for Deallocate - For files WITH bitmap
				List<int> dealloc = new List<int>();
				dealloc.Add(16);
				dealloc.Add(24);
				Manager.Deallocate(SampleFile_BM, dealloc);
				using (FileStream fsBitMap = new FileStream(SampleFile_BM + " - BitMap", FileMode.Open))
				{
					Assert.AreEqual(Manager.GetEndOfFile(fsBitMap), 20);
				}
				Assert.AreEqual(Manager.Allocate(SampleFile_BM, fsbm), 24);
				using (FileStream fsBitMap = new FileStream(SampleFile_BM + " - BitMap", FileMode.Open))
				{
					Assert.AreEqual(Manager.GetEndOfFile(fsBitMap), 16);
				}
				Assert.AreEqual(Manager.Allocate(SampleFile_BM, fsbm), 16);
				using (FileStream fsBitMap = new FileStream(SampleFile_BM + " - BitMap", FileMode.Open))
				{
					Assert.AreEqual(Manager.GetEndOfFile(fsBitMap), Manager.HeaderSize);
					Assert.IsTrue(Manager.IsFileEmpty(fsBitMap));
				}
				Assert.AreEqual(Manager.Allocate(SampleFile_BM, fsbm), Manager.GetEndOfFile(fsbm) - Manager.GetRecordSize(fsbm));
				fsbm.Close();

				//Tests for Deallocate - For files WITHOUT bitmap
				Manager.Deallocate(fs, 2);
				Assert.AreEqual(Manager.GetEndOfFile(fs), 24);
				Manager.Deallocate(fs, 3);
				fs.Close();
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
			finally
			{
				Manager.DropFile(SampleFile);
				Manager.DropFile(SampleFile_BM);
				Manager.DropFile(SampleFile_BM + " - BitMap");
			}
		}

		public void Init()
		{
			_logger = new Logger("StorageManagerTest");

			TestCreateDropFile();
			TestCreateDropFolder();
			TestRead();
			TestWrite();
			TestAllocate();
			TestDeallocate();

			_logger.Close();
		}
	}
}
