using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassNest.Services
{
    public interface IFIleDialogService
    {
        Task<string?> ChooseSaveLocationAsync(string suggestedFileName);
        Task<string?> ChooseOpenLocationAsync();
    }
}
