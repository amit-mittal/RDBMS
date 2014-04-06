using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDBMS.DataStructure;
using RDBMS.Util;

namespace RDBMS.Testing
{
	[TestClass]
	class ConverterTest
	{
		private Logger _logger;

		[TestMethod]
		private void TestConversion()
		{
			try
			{
				Dummy col = new Dummy(Dummy.DataType.Int, "Id".ToCharArray(), 4);
				byte[] colBytes = Converter.GetBytes(col);
				Dummy actualCol = (Dummy)Converter.FromBytes(colBytes);
				Assert.AreEqual(actualCol, col);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		[TestMethod]
		private void TestConversionList()
		{
			try
			{
				Dummy col_1 = new Dummy(Dummy.DataType.Int, "Id", 4);
				byte[] colBytes_1 = Converter.GetBytes(col_1);
				
				Dummy col_2 = new Dummy(Dummy.DataType.Char, "Name", 10);
				byte[] colBytes_2 = Converter.GetBytes(col_2);

				byte[] colBytes = new byte[colBytes_1.Length + colBytes_2.Length];
				colBytes_1.CopyTo(colBytes, 0);
				colBytes_2.CopyTo(colBytes, colBytes_1.Length);
				Assert.AreEqual(colBytes_1.Length, colBytes_2.Length);

				List<object> objects = Converter.FromBytes(colBytes, colBytes_1.Length);
				Assert.AreEqual(col_1, (Dummy)objects[0]);
				Assert.AreEqual(col_2, (Dummy)objects[1]);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message);
			}
		}

		public void Init()
		{
			_logger = new Logger("ConverterTest");

			TestConversion();
			TestConversionList();

			_logger.Close();
		}
	}
}
