using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace RDBMS.Util
{
	/**
	 * Use this class for any kind of
	 * conversions
	 * 
	 * Make sure that object you are 
	 * serializing is of fixed size
	 * Primitive data type will be 
	 * converted to fixed size
	 * 
	 * TestClass: ConverterTest.cs
	 */

	internal static class Converter
	{
		/**
		 * Object Conversion Methods in file
		 */

		public static void ObjectToFile(object obj, String path)
		{
			Stream stream = File.Open(path, FileMode.Create);
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, obj);
			stream.Close();
		}

		public static object FileToObject(String path)
		{
			try
			{
				Object obj;
				Stream stream = File.Open(path, FileMode.Open);
				BinaryFormatter formatter = new BinaryFormatter();
				obj = formatter.Deserialize(stream);
				stream.Close();

				return obj;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
		}


		/**
		 * Object Conversion Methods in memory
		 */

		public static byte[] ObjectToBytes(object obj)
		{
			MemoryStream fs = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			try
			{
				formatter.Serialize(fs, obj);
				return fs.ToArray();
			}
			catch (SerializationException e)
			{
				return null;
			}
			finally
			{
				fs.Close();
			}
		}

		public static object BytesToObject(byte[] bytearray)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream(bytearray);
			try
			{
				return formatter.Deserialize(stream);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
		}

		public static List<object> BytesToObjectList(byte[] bytearray, int recordLength)
		{
			try
			{
				List<object> list = new List<object>();
				for (int startIndex = 0; startIndex < bytearray.Length; startIndex += recordLength)
				{
					BinaryFormatter formatter = new BinaryFormatter();
					MemoryStream stream = new MemoryStream(bytearray, startIndex, recordLength);
					object obj = formatter.Deserialize(stream);
					list.Add(obj);
				}
				return list;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		/**
		 * Integer Conversion Methods
		 */

		public static byte[] IntToBytes(int value)
		{
			return BitConverter.GetBytes(value);
		}

		public static int BytesToInt(byte[] bytearray)
		{
			int record = BitConverter.ToInt32(bytearray, 0);
			return record;
		}

		public static List<int> BytesToIntList(byte[] bytearray)
		{
			try
			{
				int recordLength = sizeof (Int32);
				List<int> list = new List<int>();
				for (int startIndex = 0; startIndex < bytearray.Length; startIndex += recordLength)
				{
					list.Add(BitConverter.ToInt32(bytearray, startIndex));
				}
				return list;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		/**
		 * Char array conversion methods
		 */

		public static byte[] CharToBytes(char[] str)
		{
			return Encoding.ASCII.GetBytes(str);
		}

		public static char[] BytesToChar(byte[] bytearray)
		{
			return Encoding.ASCII.GetChars(bytearray);
		}

		/**
		 * String conversion methods
		 */

		public static byte[] StringToBytes(String str, int size = Constants.MaxStringLength)
		{
			byte[] bytes = new byte[size*sizeof (char)];
			Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, str.Length*sizeof (char));
			return bytes;
		}

		public static String BytesToString(byte[] bytearray)
		{
			char[] chars = new char[bytearray.Length/sizeof (char)];
			Buffer.BlockCopy(bytearray, 0, chars, 0, bytearray.Length);
			String s = new string(chars).Replace("\0", string.Empty);
			return s;
		}

		public static List<String> BytesToStringList(byte[] bytearray, int recordLength = Constants.MaxStringLength)
		{
			try
			{
				int totalRecordSize = recordLength*sizeof (char);
				List<String> list = new List<String>();
				for (int startIndex = 0; startIndex < bytearray.Length; startIndex += totalRecordSize)
				{
					char[] chars = new char[recordLength];
					Buffer.BlockCopy(bytearray, startIndex, chars, 0, totalRecordSize);
					list.Add(new string(chars).Replace("\0", string.Empty));
				}
				return list;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		/**
		 * Double Conversion methods
		 */

		public static byte[] DoubleToBytes(double value)
		{
			return BitConverter.GetBytes(value);
		}

		public static double BytesToDouble(byte[] bytearray)
		{
			double record = BitConverter.ToDouble(bytearray, 0);
			return record;
		}

		public static List<double> BytesToDoubleList(byte[] bytearray)
		{
			try
			{
				int recordLength = sizeof (double);
				List<double> list = new List<double>();
				for (int startIndex = 0; startIndex < bytearray.Length; startIndex += recordLength)
				{
					list.Add(BitConverter.ToDouble(bytearray, startIndex));
				}
				return list;
			}
			catch (Exception e)
			{
				return null;
			}
		}
	}
}