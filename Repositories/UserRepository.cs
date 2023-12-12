using Microsoft.EntityFrameworkCore;
using PersistentRegister.Interfaces;
using PersistentRegister.Models;

namespace PersistentRegister.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
        {

            var apiResponse = new ApiResponse<bool>();

            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.ID == id);
                if (user is null)
                {
                    throw new Exception($"User with Id '{id}' not found.");
                }

                _context.User.Remove(user);
                await _context.SaveChangesAsync();
                apiResponse.Message = "User deleted successfully.";
            }
            catch (Exception e)
            {
                apiResponse.Success = false;
                apiResponse.Message = e.Message;
            }

            return apiResponse;
        }

        public async Task<ApiResponse<List<User>>> GetAllAsync()
        {
            var apiResponse = new ApiResponse<List<User>>();

            try
            {
                var users = await _context.User.ToListAsync();
                apiResponse.Data = users;
            }
            catch (Exception e)
            {
                apiResponse.Success = false;
                apiResponse.Message = e.Message;
            }

            return apiResponse;
        }

        public async Task<ApiResponse<User>> GetByIdAsync(Guid id)
        {
            var apiResponse = new ApiResponse<User>();

            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.ID == id);
                apiResponse.Data = user;
            }
            catch (Exception e)
            {
                apiResponse.Success = false;
                apiResponse.Message = e.Message;
            }

            return apiResponse;
        }

        public async Task<ApiResponse<User>> InsertAsync(User user)
        {
            var apiResponse = new ApiResponse<User>();

            try
            {
                _context.User.Add(user);
                await _context.SaveChangesAsync();
                apiResponse.Data = user;
                apiResponse.Message = "User inserted successfully.";
            }
            catch (Exception e)
            {
                apiResponse.Success = false;
                apiResponse.Message = e.Message;
            }
            return apiResponse;
        }

        public async Task<ApiResponse<bool>> IsEmailUniqueAsync(string email)
        {
            var apiResponse = new ApiResponse<bool>();

            try
            {
                bool isUnique = await _context.User.AnyAsync(u => u.Email == email);
                apiResponse.Data = isUnique;
            }
            catch (Exception e)
            {
                apiResponse.Success = false;
                apiResponse.Message = e.Message;
            }

            return apiResponse;
        }

        public async Task<ApiResponse<User>> UpdateAsync(User user)
        {
            var apiResponse = new ApiResponse<User>();

            try
            {
                _context.User.Update(user);
                await _context.SaveChangesAsync();
                apiResponse.Data = user;
                apiResponse.Message = "User updated successfully.";
            }
            catch (Exception e)
            {
                apiResponse.Success = false;
                apiResponse.Message = e.Message;
            }
            return apiResponse;
        }
    }
}