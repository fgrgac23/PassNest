using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Backup
{
    public interface IBackupStore
    {
        void WriteToFile(string data, string filePath);
        string ReadFromFile(string filePath);
    }
}
