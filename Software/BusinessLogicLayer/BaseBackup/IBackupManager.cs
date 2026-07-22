using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.BaseBackup
{
    public interface IBackupManager
    {
        void CreateBackup(string filePath);
        void RestoreBackup(string filePath, string masterPassword);
    }
}
