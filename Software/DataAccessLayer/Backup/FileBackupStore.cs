using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Backup
{
    public class FileBackupStore : IBackupStore
    {
        public string ReadFromFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public void WriteToFile(string data, string filePath)
        {
            File.WriteAllText(filePath, data);
        }
    }
}
