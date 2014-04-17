using System;
using System.IO;

namespace RDBMS.SpaceManager
{
	//not needed as of now
	internal class BufferManager
	{
		//handles storage storageManager
		//this class can bring any amount of data
		//this keeps track of file descriptors for the file stream
		//and returns the whole list to subquery handler

		private StorageManager _manager;

		public BufferManager()
		{
			_manager = new StorageManager();
		}

		public StorageManager GetManager()
		{
			return _manager;
		}
	}
}