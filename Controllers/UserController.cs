using ApiLibrary.Responses;
using Microsoft.AspNetCore.Mvc;
using PersistentRegister.Dtos.User;
using PersistentRegister.Interfaces;
using PersistentRegister.Models;

namespace PersistentRegister.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
        {
            var response = await _userService.DeleteAsync(id);
            if (response.Data is false)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<ApiResponse<List<GetUserDto>>>> GetAll()
        {
            return Ok(await _userService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GetUserDto>>> Get(Guid id)
        {
            return Ok(await _userService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<GetUserDto>>> Insert(InsertUserDto newUser)
        {
            return Ok(await _userService.InsertAsync(newUser));
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse<GetUserDto>>> Update(UpdateUserDto updatedUser)
        {
            return Ok(await _userService.UpdateAsync(updatedUser));
        }
    }
}