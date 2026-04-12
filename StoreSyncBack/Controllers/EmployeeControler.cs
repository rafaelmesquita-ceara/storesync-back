using Microsoft.AspNetCore.Mvc;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeService service, ILogger<EmployeesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            var list = await _service.GetAllEmployeesAsync(limit, offset);
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var emp = await _service.GetEmployeeByIdAsync(id);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Employee employee)
        {
            try
            {
                var affected = await _service.CreateEmployeeAsync(employee);
                if (affected <= 0)
                    return BadRequest("Não foi possível criar o funcionário.");

                return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeId }, employee);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação CreateEmployee inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar employee");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Employee employee)
        {
            if (id != employee.EmployeeId)
                return BadRequest("Id do caminho diferente do corpo.");

            try
            {
                var affected = await _service.UpdateEmployeeAsync(employee);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação UpdateEmployee inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar employee");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var affected = await _service.DeleteEmployeeAsync(id);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação DeleteEmployee inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar employee");
                return StatusCode(500, "Erro interno.");
            }
        }
    }
}
