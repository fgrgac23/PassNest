using System.Threading.Tasks;

namespace PassNest.Services
{
    public interface IClipboardService
    {
        Task SetTextAsync(string text);
    }
}
