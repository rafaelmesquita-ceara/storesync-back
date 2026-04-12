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

        [HttpGet("calculate")]
        public async Task<IActionResult> Calculate(
            [FromQuery] Guid employeeId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var (totalSales, commissionRate, commissionValue) =
                    await _service.CalculateAsync(employeeId, startDate, endDate);

                return Ok(new
                {
                    totalSales,
                    commissionRate,
                    commissionValue
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação Calculate inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular comissão");
                return StatusCode(500, "Erro interno.");
            }
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
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Regra de negócio CreateCommission violada");
                return UnprocessableEntity(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar comissão");
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
