using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RDBMS.Util
{
	static class Converter
	{
		public static byte[] GetBytes(object obj)
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

		public static object FromBytes(byte[] bytearray)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream stream = new MemoryStream(bytearray);
			try
			{
				return formatter.Deserialize(stream);
			}
			catch
			{
				return null;
			}
		}

		public static List<object> FromBytes(byte[] bytearray, int recordLength)
		{
			try
			{
				List<object> list = new List<object>();
				for (int startIndex = 0; startIndex < bytearray.Length; startIndex+=recordLength)
				{
					BinaryFormatter formatter = new BinaryFormatter();
					MemoryStream stream = new MemoryStream(bytearray, startIndex, recordLength);
					object obj = formatter.Deserialize(stream);
					list.Add(obj);
				}
				return list;
			}
			catch(Exception e)
			{
				return null;
			}
		}

		public static List<int> FromBytesParseInt(byte[] bytearray)
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
	}
}
