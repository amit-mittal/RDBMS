using RDBMS.Testing;

namespace RDBMS
{
    class Program
    {
        static void Main(string[] args)
        {
			InitTests();
        }

		static void InitTests()
		{
			var storageManagerTest = new StorageManagerTest();
			storageManagerTest.Init();
		}
    }
}
