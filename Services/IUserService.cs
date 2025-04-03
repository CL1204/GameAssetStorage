using GameAssetStorage.Models;
using System.Threading.Tasks;

namespace GameAssetStorage.Services
{
    public interface IUserService
    {
        Task<User?> Authenticate(string username, string password);
        Task<User> Register(string username, string password);
        Task<User?> GetUserByUsername(string username);
    }
}
