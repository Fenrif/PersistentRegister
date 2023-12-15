using ApiLibrary.Responses;
using PersistentRegister.Models;

namespace PersistentRegister.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<ApiResponse<bool>> DeleteAsync(Guid id);
        Task<ApiResponse<List<T>>> GetAllAsync();
        Task<ApiResponse<T>> GetByIdAsync(Guid id);
        Task<ApiResponse<T>> InsertAsync(T entity);
        Task<ApiResponse<T>> UpdateAsync(T entity);
    }
}