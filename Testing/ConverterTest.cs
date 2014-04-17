using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDBMS.DataStructure;
using RDBMS.Util;

namespace RDBMS.Testing
{
	[TestClass]
	internal class ConverterTest
	{
		private Logger _logger;

		[TestMethod]
		private void TestObjectConversion()
		{
			try
			{
				_logger.Message("Testing ObjectConversion");
				Dummy col = new Dummy(Dummy.DataType.Int, "Id".ToCharArray(), 4);
				byte[] colBytes = Converter.ObjectToBytes(col);
				Dummy actualCol = (Dummy) Converter.BytesToObject(colBytes);
				Assert.AreEqual(actualCol, col);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		[TestMethod]
		private void TestObjectConversionList()
		{
			try
			{
				_logger.Message("Testing ObjectConversionList");
				Dummy col_1 = new Dummy(Dummy.DataType.Int, "Id", 4);
				byte[] colBytes_1 = Converter.ObjectToBytes(col_1);

				Dummy col_2 = new Dummy(Dummy.DataType.Char, "Name", 10);
				byte[] colBytes_2 = Converter.ObjectToBytes(col_2);

				byte[] colBytes = new byte[colBytes_1.Length + colBytes_2.Length];
				colBytes_1.CopyTo(colBytes, 0);
				colBytes_2.CopyTo(colBytes, colBytes_1.Length);
				Assert.AreEqual(colBytes_1.Length, colBytes_2.Length);

				List<object> objects = Converter.BytesToObjectList(colBytes, colBytes_1.Length);
				Assert.AreEqual(col_1, (Dummy) objects[0]);
				Assert.AreEqual(col_2, (Dummy) objects[1]);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		[TestMethod]
		private void TestIntegerConversion()
		{
			try
			{
				_logger.Message("Testing IntegerConversion");
				int v1 = 2048;
				byte[] v1Bytes = Converter.IntToBytes(v1);
				Assert.AreEqual(v1, Converter.BytesToInt(v1Bytes));

				int v2 = 4096;
				byte[] v2Bytes = Converter.IntToBytes(v2);
				byte[] combinedBytes = new byte[v1Bytes.Length + v2Bytes.Length];
				v1Bytes.CopyTo(combinedBytes, 0);
				v2Bytes.CopyTo(combinedBytes, v1Bytes.Length);
				List<int> ints = Converter.BytesToIntList(combinedBytes);
				Assert.AreEqual(v1, ints[0]);
				Assert.AreEqual(v2, ints[1]);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		[TestMethod]
		private void TestDoubleConversion()
		{
			try
			{
				_logger.Message("Testing DoubleConversion");
				double v1 = 20.48;
				byte[] v1Bytes = Converter.DoubleToBytes(v1);
				Assert.AreEqual(v1, Converter.BytesToDouble(v1Bytes));

				double v2 = 40.96;
				byte[] v2Bytes = Converter.DoubleToBytes(v2);
				byte[] combinedBytes = new byte[v1Bytes.Length + v2Bytes.Length];
				v1Bytes.CopyTo(combinedBytes, 0);
				v2Bytes.CopyTo(combinedBytes, v1Bytes.Length);
				List<double> doubles = Converter.BytesToDoubleList(combinedBytes);
				Assert.AreEqual(v1, doubles[0]);
				Assert.AreEqual(v2, doubles[1]);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		[TestMethod]
		private void TestStringConversion()
		{
			try
			{
				_logger.Message("Testing StringConversion");
				String s1 = "Hello";
				byte[] s1Bytes = Converter.StringToBytes(s1);
				Assert.AreEqual(s1, Converter.BytesToString(s1Bytes));

				String s2 = "World!!";
				byte[] s2Bytes = Converter.StringToBytes(s2);
				Assert.AreEqual(s1Bytes.Length, s1Bytes.Length);

				byte[] combinedBytes = new byte[s2Bytes.Length + s1Bytes.Length];
				s1Bytes.CopyTo(combinedBytes, 0);
				s2Bytes.CopyTo(combinedBytes, s1Bytes.Length);
				List<String> strings = Converter.BytesToStringList(combinedBytes);
				Assert.AreEqual(s1, strings[0]);
				Assert.AreEqual(s2, strings[1]);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		[TestMethod]
		private void TestCharConversion()
		{
			try
			{
				_logger.Message("Testing CharConversion");
				char[] s1 = new char[Constants.MaxStringSize];
				"Hello".ToCharArray().CopyTo(s1, 0);
				byte[] s1Bytes = Converter.CharToBytes(s1);
				Assert.AreEqual(new string(s1), new string(Converter.BytesToChar(s1Bytes)));

				char[] s2 = new char[Constants.MaxStringSize];
				"Hello".ToCharArray().CopyTo(s2, 0);
				"World!!!".ToCharArray().CopyTo(s2, Constants.MaxStringSize/2);
				byte[] s2Bytes = Converter.CharToBytes(s2);
				Assert.AreEqual(new string(s2), new string(Converter.BytesToChar(s2Bytes)));
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		public void Init()
		{
			_logger = new Logger("ConverterTest");

			TestObjectConversion();
			TestObjectConversionList();
			TestIntegerConversion();
			TestDoubleConversion();
			TestStringConversion();
			TestCharConversion();

			_logger.Close();
		}
	}
}