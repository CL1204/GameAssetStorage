using System.Threading.Tasks;
using GameAssetStorage.Models;

namespace GameAssetStorage.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsername(string user);
        Task AddUser(User user);
    }
}
