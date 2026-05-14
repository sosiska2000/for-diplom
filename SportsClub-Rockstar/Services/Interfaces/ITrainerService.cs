using Rockstar.Admin.WPF.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rockstar.Admin.WPF.Services.Interfaces
{
    public interface ITrainerService
    {
        Task<List<Trainer>> GetAllAsync();
        Task<Trainer?> GetByIdAsync(int id);
        Task<Trainer?> GetByEmailAsync(string email);
        Task<bool> CreateAsync(Trainer trainer);
        Task<bool> UpdateAsync(Trainer trainer);
        Task<bool> DeleteAsync(int id);
        List<string> GetAvailableDirections();
    }
}