using Microsoft.AspNetCore.Mvc;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalePaymentsController : ControllerBase
    {
        private readonly ISalePaymentService _service;
        private readonly ILogger<SalePaymentsController> _logger;

        public SalePaymentsController(ISalePaymentService service, ILogger<SalePaymentsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("by-sale/{saleId:guid}")]
        public async Task<IActionResult> GetBySaleId(Guid saleId)
        {
            var payments = await _service.GetBySaleIdAsync(saleId);
            return Ok(payments);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SalePayment payment)
        {
            try
            {
                var affected = await _service.AddPaymentAsync(payment);
                if (affected <= 0)
                    return BadRequest("Não foi possível registrar o pagamento.");

                return CreatedAtAction(nameof(GetBySaleId), new { saleId = payment.SaleId }, payment);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação AddPayment inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar pagamento");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var affected = await _service.RemovePaymentAsync(id);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação RemovePayment inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover pagamento");
                return StatusCode(500, "Erro interno.");
            }
        }
    }
}
