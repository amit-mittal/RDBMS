using System;
using System.IO;

/**
 * Interacts with the OS File System
 */
namespace RDBMS.SpaceManager{
	class StorageManager{
		public void CreateFile(String fileName, int recordLength){
			try {
				File.Create(fileName);
				File.WriteAllBytes(fileName, BitConverter.GetBytes(recordLength));
			} catch(Exception e) {
				throw e;
			}
		}

		public void DropFile(String fileName){
			try {
				File.Delete(fileName);
			} catch(Exception e){
				throw e;
			}
		}

		public void CreateFolder(String fileName){
			try{
				File.Delete(fileName);
			} catch (Exception e){
				throw e;
			}
		}
	}
}
