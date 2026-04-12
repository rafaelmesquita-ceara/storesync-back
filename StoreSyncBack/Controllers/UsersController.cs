using Microsoft.AspNetCore.Mvc;
using SharedModels;
using SharedModels.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace StoreSyncBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IValidator<UserLoginDto> _loginValidator;
        private readonly IValidator<UserChangePasswordDto> _changeValidator;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService service,
            IValidator<UserLoginDto> loginValidator,
            IValidator<UserChangePasswordDto> changeValidator,
            ILogger<UsersController> logger)
        {
            _service = service;
            _loginValidator = loginValidator;
            _changeValidator = changeValidator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            var usersResult = await _service.GetAllUsersAsync(limit, offset);
            // não retornar senha no payload
            usersResult.Items.ToList().ForEach(u => u.Password = null);
            return Ok(usersResult);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _service.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            user.Password = null;
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var validate = await _loginValidator.ValidateAsync(dto);
            if (!validate.IsValid) return BadRequest(validate.Errors.Select(e => e.ErrorMessage));

            var user = await _service.Login(dto);
            if (user == null) return Unauthorized("Login ou senha inválidos.");
            
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            try
            {
                var affected = await _service.CreateUserAsync(user);
                if (affected <= 0) return BadRequest("Não foi possível criar o usuário.");
                user.Password = null;
                return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação CreateUser inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] User user)
        {
            if (id != user.UserId) return BadRequest("Id do caminho diferente do corpo.");

            try
            {
                var affected = await _service.UpdateUserAsync(user);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação UpdateUser inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var affected = await _service.DeleteUserAsync(id);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação DeleteUser inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar usuário");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordDto dto)
        {
            var validate = await _changeValidator.ValidateAsync(dto);
            if (!validate.IsValid) return BadRequest(validate.Errors.Select(e => e.ErrorMessage));

            try
            {
                var ok = await _service.ChangeUserPasswordAsync(dto);
                if (!ok) return BadRequest("Não foi possível alterar a senha.");
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação ChangePassword inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar senha");
                return StatusCode(500, "Erro interno.");
            }
        }
    }
}
