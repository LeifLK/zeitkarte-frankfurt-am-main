using RmvApiBackend.Models;
using System.Threading.Tasks;

namespace RmvApiBackend.Services
{
    /// <summary>
    /// This is the "contract" for our RMV service.
    /// It defines a method for finding a location.
    /// </summary>
    public interface IRmvService
    {
        Task<RmvLocationResponse?> FindLocationAsync(string searchTerm);
    }
}
