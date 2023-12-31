using ApiLibrary.Responses;
using ApiLibrary.TextConstants;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PersistentRegister.Interfaces;
using PersistentRegister.Models;
using Serilog;

namespace PersistentRegister.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public UserRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
        {
            var apiResponse = new ApiResponse<bool>();
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
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

                    string filePath = _configuration["AppSettings:FileStorageFile"];
                    List<User> existingUsers = LoadJsonData(filePath);
                    DeleteUserFromJson(existingUsers, id, filePath);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    apiResponse.Success = false;
                    apiResponse.Message = ex.Message;
                }
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
                apiResponse.Message = users.Count > 0 ? SuccessMessages.UsersRetrieved : InformationMessages.NoUsersFound;
            }
            catch (Exception ex)
            {
                apiResponse.Success = false;
                apiResponse.Message = ex.Message;
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
            catch (Exception ex)
            {
                apiResponse.Success = false;
                apiResponse.Message = ex.Message;
            }

            return apiResponse;
        }

        public async Task<ApiResponse<User>> InsertAsync(User user)
        {
            var apiResponse = new ApiResponse<User>();
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.User.Add(user);
                    await _context.SaveChangesAsync();

                    string filePath = _configuration["AppSettings:FileStorageFile"];
                    List<User> existingUsers = LoadJsonData(filePath);
                    SaveJsonData(existingUsers, user, filePath);

                    await transaction.CommitAsync();

                    apiResponse.Data = user;
                    apiResponse.Message = SuccessMessages.UserInserted;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    apiResponse.Success = false;
                    apiResponse.Message = ex.Message;
                }
                return apiResponse;
            }
        }

        public async Task<ApiResponse<bool>> IsEmailUniqueAsync(string email)
        {
            var apiResponse = new ApiResponse<bool>();

            try
            {
                bool isUnique = await _context.User.AnyAsync(u => u.Email == email);
                apiResponse.Data = isUnique;
            }
            catch (Exception ex)
            {
                apiResponse.Success = false;
                apiResponse.Message = ex.Message;
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
                apiResponse.Message = SuccessMessages.UserUpdated;
            }
            catch (Exception ex)
            {
                apiResponse.Success = false;
                apiResponse.Message = ex.Message;
            }
            return apiResponse;
        }

        #region Helpers
        public List<User> LoadJsonData(string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (!File.Exists(filePath))
                File.Create(filePath).Close();

            string jsonContent = File.ReadAllText(filePath);
            List<User> existingUsers = JsonConvert.DeserializeObject<List<User>>(jsonContent) ?? new List<User>();

            return existingUsers;
        }

        public void SaveJsonData(List<User> existingUsers, User user, string filePath)
        {
            try
            {
                // throw new Exception("Error saving data to JSON file");
                existingUsers.Add(user);

                string json = JsonConvert.SerializeObject(existingUsers, Formatting.Indented);

                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    File.WriteAllText(filePath, json);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorMessages.SavingJson, ex);
            }

        }

        public void DeleteUserFromJson(List<User> existingUsers, Guid userID, string filePath)
        {
            try
            {
                var userToDeleted = existingUsers.FirstOrDefault(user => user.ID == userID);

                existingUsers.Remove(userToDeleted);

                string json = JsonConvert.SerializeObject(existingUsers, Formatting.Indented);

                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    File.WriteAllText(filePath, json);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorMessages.SavingJson, ex);
            }

        }
        #endregion
    }
}