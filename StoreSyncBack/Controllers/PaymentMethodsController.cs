using Microsoft.AspNetCore.Mvc;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodsController : ControllerBase
    {
        private readonly IPaymentMethodService _service;
        private readonly ILogger<PaymentMethodsController> _logger;

        public PaymentMethodsController(IPaymentMethodService service, ILogger<PaymentMethodsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var pm = await _service.GetByIdAsync(id);
            if (pm == null) return NotFound();
            return Ok(pm);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PaymentMethod pm)
        {
            try
            {
                var affected = await _service.CreateAsync(pm);
                if (affected <= 0)
                    return BadRequest("Não foi possível registrar a forma de pagamento.");

                return CreatedAtAction(nameof(GetById), new { id = pm.PaymentMethodId }, pm);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação CreatePaymentMethod inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar forma de pagamento");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PaymentMethod pm)
        {
            if (id != pm.PaymentMethodId)
                return BadRequest("Id do caminho diferente do corpo.");

            try
            {
                var affected = await _service.UpdateAsync(pm);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação UpdatePaymentMethod inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar forma de pagamento");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var affected = await _service.DeleteAsync(id);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir forma de pagamento");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpGet("{id:guid}/rates")]
        public async Task<IActionResult> GetRates(Guid id)
        {
            var pm = await _service.GetByIdAsync(id);
            if (pm == null) return NotFound();
            return Ok(pm.Rates ?? new List<PaymentMethodRate>());
        }

        [HttpPost("{id:guid}/rates")]
        public async Task<IActionResult> AddRate(Guid id, [FromBody] PaymentMethodRate rate)
        {
            try
            {
                var affected = await _service.AddRateAsync(id, rate);
                if (affected <= 0)
                    return BadRequest("Não foi possível adicionar a taxa.");

                return CreatedAtAction(nameof(GetById), new { id }, rate);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação AddRate inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar taxa");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpDelete("{id:guid}/rates/{rateId:guid}")]
        public async Task<IActionResult> DeleteRate(Guid id, Guid rateId)
        {
            try
            {
                var affected = await _service.DeleteRateAsync(id, rateId);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir taxa");
                return StatusCode(500, "Erro interno.");
            }
        }
    }
}
