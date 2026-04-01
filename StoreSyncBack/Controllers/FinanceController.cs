using Microsoft.AspNetCore.Mvc;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _service;
        private readonly ILogger<FinanceController> _logger;

        public FinanceController(IFinanceService service, ILogger<FinanceController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllFinanceAsync();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var finance = await _service.GetFinanceByIdAsync(id);
            if (finance == null) return NotFound();
            return Ok(finance);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Finance finance)
        {
            try
            {
                var affected = await _service.CreateFinanceAsync(finance);
                if (affected <= 0)
                    return BadRequest("Não foi possível criar o registro financeiro.");

                return CreatedAtAction(nameof(GetById), new { id = finance.FinanceId }, finance);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação CreateFinance inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar registro financeiro");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Finance finance)
        {
            if (id != finance.FinanceId)
                return BadRequest("Id do caminho diferente do corpo.");

            try
            {
                var affected = await _service.UpdateFinanceAsync(finance);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação UpdateFinance inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar registro financeiro");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var affected = await _service.DeleteFinanceAsync(id);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação DeleteFinance inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar registro financeiro");
                return StatusCode(500, "Erro interno.");
            }
        }
    }
}
