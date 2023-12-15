using System.Diagnostics;
using System.Net;
using System.Text;
using ApiLibrary.Responses;
using ApiLibrary.TextConstants;
using AutoMapper;
using Newtonsoft.Json;
using PersistentRegister.Dtos.User;
using PersistentRegister.Interfaces;
using PersistentRegister.Models;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Serilog;

namespace PersistentRegister.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        // private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string endPoint2;
        private readonly string endPoint3;
        //Set the Retry Policy using Polly
        private AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy<HttpResponseMessage>
                                                .Handle<Exception>()
                                                .OrResult(x => x.StatusCode is >= HttpStatusCode.BadRequest)
                                                // .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), retryCount: 5),
                                                // onRetry: (exception, retryCount, context) =>
                                                // {
                                                //     Log.Error(ErrorMessages.HttpPostRetry, exception.Exception.Message, retryCount);
                                                // });
                                                .RetryAsync(5, onRetry: (exception, retryCount, context) =>
                                                {
                                                    Log.Error(ErrorMessages.HttpPostRetry, exception.Exception.Message, retryCount);
                                                });



        public UserService(IMapper mapper, IUserRepository userRepository, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            // _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            endPoint2 = configuration["AppSettings:EndPoint2"];
            endPoint3 = configuration["AppSettings:EndPoint3"];
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
            var watch = new Stopwatch();

            try
            {
                bool sentToEndPoint2 = false, sentToEndPoint3 = false;
                watch.Start();
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
                watch.Stop();

                if (!repositoryResponse.Success)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = repositoryResponse.Message;
                    Log.Error(repositoryResponse.Message);
                }
                else
                {
                    bool rollBack = false;
                    var userDto = _mapper.Map<GetUserDto>(repositoryResponse.Data);
                    serviceResponse.Data = userDto;
                    serviceResponse.Message = SuccessMessages.UserInserted;

                    //Map to UserRegisterInfo to send to the other EndPoints
                    UserRegisterInfo userRegisterInfo = new UserRegisterInfo();
                    _mapper.Map(user, userRegisterInfo);
                    userRegisterInfo.RegistrationDate = DateTime.Now;
                    userRegisterInfo.TotalPersistTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);

                    //Create json for EndPoints
                    var jsonUser = JsonConvert.SerializeObject(userRegisterInfo);
                    var jsonContent = new StringContent(jsonUser, Encoding.UTF8, "application/json");

                    HttpResponseMessage responseEndPoint2 = new HttpResponseMessage();
                    HttpResponseMessage responseEndPoint3 = new HttpResponseMessage();
                    string endPointExMessage = string.Empty;
                    try
                    {
                        responseEndPoint2 = await retryPolicy.ExecuteAsync(async () =>
                        {
                            var responseMessage = await _httpClient.PostAsync(endPoint2, jsonContent);
                            return responseMessage;
                        });

                        sentToEndPoint2 = true;
                    }
                    catch (Exception ex)
                    {
                        rollBack = true;
                        endPointExMessage = ex.Message;
                    }

                    try
                    {
                        responseEndPoint3 = await retryPolicy.ExecuteAsync(async () =>
                        {
                            var responseMessage = await _httpClient.PostAsync(endPoint3, jsonContent);
                            return responseMessage;
                        });

                        sentToEndPoint3 = true;
                    }
                    catch (Exception ex)
                    {
                        rollBack = true;
                        endPointExMessage = string.IsNullOrEmpty(endPointExMessage)? ex.Message : Environment.NewLine + ex.Message;
                    }

                    if (rollBack || !responseEndPoint2.IsSuccessStatusCode || !responseEndPoint3.IsSuccessStatusCode)
                    {
                        await DeleteAsync(user.ID);
                        if (sentToEndPoint2)
                            await _httpClient.DeleteAsync(endPoint2 + "/" + user.ID);
                        if (sentToEndPoint3)
                            await _httpClient.DeleteAsync(endPoint3 + "/" + user.ID);

                        serviceResponse.Success = false;
                        serviceResponse.Message = ErrorMessages.RegisterError + ": " + endPointExMessage;
                        Log.Error(ErrorMessages.RegisterError + ": " + endPointExMessage);
                    }
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
            var serviceResponse = new ApiResponse<GetUserDto>();
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