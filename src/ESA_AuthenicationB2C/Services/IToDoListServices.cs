using System.Collections.Generic;
using System.Threading.Tasks;
using ESA_AuthenicationB2CApi.Models;

namespace ESA_AuthenicationB2C.Services
{
    public interface ITodoListService
    {
        Task<IEnumerable<Todo>> GetAsync();
        
        Task<IEnumerable<Todo>> GetAllAsync();

        Task<Todo> GetAsync(int id);

        Task DeleteAsync(int id);

        Task<Todo> AddAsync(Todo todo);

        Task<Todo> EditAsync(Todo todo);
    }
}