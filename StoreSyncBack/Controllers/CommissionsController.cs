using Microsoft.AspNetCore.Mvc;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommissionsController : ControllerBase
    {
        private readonly ICommissionService _service;
        private readonly ILogger<CommissionsController> _logger;

        public CommissionsController(ICommissionService service, ILogger<CommissionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllCommissionsAsync();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var commission = await _service.GetCommissionByIdAsync(id);
            if (commission == null) return NotFound();
            return Ok(commission);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Commission commission)
        {
            try
            {
                var affected = await _service.CreateCommissionAsync(commission);
                if (affected <= 0)
                    return BadRequest("Não foi possível criar a comissão.");

                return CreatedAtAction(nameof(GetById), new { id = commission.CommissionId }, commission);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação CreateCommission inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar comissão");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Commission commission)
        {
            if (commission.CommissionId == null || id != commission.CommissionId)
                return BadRequest("Id do caminho diferente do corpo.");

            try
            {
                var affected = await _service.UpdateCommissionAsync(commission);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação UpdateCommission inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar comissão");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var affected = await _service.DeleteCommissionAsync(id);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação DeleteCommission inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar comissão");
                return StatusCode(500, "Erro interno.");
            }
        }
    }
}
