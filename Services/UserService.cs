using AutoMapper;
using PersistentRegister.Dtos.User;
using PersistentRegister.Interfaces;
using PersistentRegister.Models;
using Serilog;

namespace PersistentRegister.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public UserService(IMapper mapper, IUserRepository userRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }
        public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
        {
            var serviceResponse = new ApiResponse<bool>();

            try
            {
                var repositoryResponse = await _userRepository.DeleteAsync(id);

                if (!repositoryResponse.Success)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = repositoryResponse.Message;
                    Log.Error(serviceResponse.Message);
                }
                else
                {
                    serviceResponse.Data = true;
                    serviceResponse.Message = SuccessMessages.UserDeleted;
                    Log.Information(serviceResponse.Message);
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
                Log.Error(serviceResponse.Message);
            }

            return serviceResponse;
        }

        public async Task<ApiResponse<List<GetUserDto>>> GetAllAsync()
        {
            var serviceResponse = new ApiResponse<List<GetUserDto>>();

            try
            {
                var repositoryResponse = await _userRepository.GetAllAsync();

                if (!repositoryResponse.Success)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = repositoryResponse.Message;
                    Log.Error(serviceResponse.Message);
                }
                else
                {
                    var users = _mapper.Map<List<GetUserDto>>(repositoryResponse.Data);
                    serviceResponse.Data = users;
                    serviceResponse.Message = repositoryResponse.Message;
                    Log.Information(serviceResponse.Message);
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
                Log.Error(serviceResponse.Message);
            }

            return serviceResponse;
        }

        public async Task<ApiResponse<GetUserDto>> GetByIdAsync(Guid id)
        {
            var serviceResponse = new ApiResponse<GetUserDto>();

            try
            {
                var repositoryResponse = await _userRepository.GetByIdAsync(id);

                if (!repositoryResponse.Success)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = repositoryResponse.Message;
                    Log.Error(serviceResponse.Message);
                } 
                else
                {
                    var user = _mapper.Map<GetUserDto>(repositoryResponse.Data);
                    serviceResponse.Data = user;
                    Log.Information(serviceResponse.Message);
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ApiResponse<GetUserDto>> InsertAsync(InsertUserDto newUser)
        {
            var serviceResponse = new ApiResponse<GetUserDto>();

            try
            {
                var user = _mapper.Map<User>(newUser);

                var emailAlreadyExists = await _userRepository.IsEmailUniqueAsync(user.Email);
                if (emailAlreadyExists.Data)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = string.Format(ErrorMessages.EmailExists, user.Email);
                    Log.Information(ErrorMessages.EmailExists, user.Email);
                    return serviceResponse;
                }

                var repositoryResponse = await _userRepository.InsertAsync(user);

                if (!repositoryResponse.Success)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = repositoryResponse.Message;
                    Log.Error(repositoryResponse.Message);
                }
                else
                {
                    var userDto = _mapper.Map<GetUserDto>(repositoryResponse.Data);
                    serviceResponse.Data = userDto; 
                    serviceResponse.Message = SuccessMessages.UserInserted;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
                Log.Error(ex.Message, ErrorMessages.RegisterError);
            }

            return serviceResponse;
        }

        public async Task<ApiResponse<GetUserDto>> UpdateAsync(UpdateUserDto updatedUser)
        {
            var serviceResponse =  new ApiResponse<GetUserDto>();

            try
            {
                var user = _mapper.Map<User>(updatedUser);
                var repositoryResponse = await _userRepository.UpdateAsync(user);

                if (!repositoryResponse.Success)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = repositoryResponse.Message;
                }
                else
                {
                    var userDto = _mapper.Map<GetUserDto>(repositoryResponse.Data);
                    serviceResponse.Data = userDto;
                    serviceResponse.Message = SuccessMessages.UserUpdated;
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }
    }
}