using System;
using System.Collections.Generic;
using System.IO;
using RDBMS.DataStructure;
using RDBMS.SpaceManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDBMS.Util;

namespace RDBMS.Testing
{
	[TestClass]
	class StorageManagerTest
	{
		private static readonly StorageManager Manager = new StorageManager();
		private Logger _logger;
		private const String SampleFile = "E:\\Sample";
		private const String SampleFolder = "E:\\Sample";

		[TestMethod]
		private void TestCreateDropFile()
		{
			try
			{
				Manager.CreateFile(SampleFile, 10, false);
				Assert.IsTrue(File.Exists(SampleFile));
				Assert.AreEqual(File.ReadAllBytes(SampleFile).Length, Manager.HeaderSize);
				
				Manager.DropFile(SampleFile);
				Assert.IsTrue(!File.Exists(SampleFile));
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
				List<int> list = Converter.FromBytesParseInt(Manager.Read(fs, 4, 2));
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
				byte[] colBytes = Converter.GetBytes(col);
				Manager.CreateFile(SampleFile, colBytes.Length, false);
				fs = new FileStream(SampleFile, FileMode.OpenOrCreate);
				Manager.Write(fs, Manager.HeaderSize, colBytes);
				Dummy actualObj = (Dummy)Converter.FromBytes(Manager.Read(fs, Manager.HeaderSize, 1));
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

		public void Init()
		{
			_logger = new Logger("StorageManagerTest");
			
			TestCreateDropFile();
			TestCreateDropFolder();
			TestRead();
			TestWrite();
			
			_logger.Close();
		}
	}
}
