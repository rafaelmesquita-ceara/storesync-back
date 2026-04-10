using Microsoft.AspNetCore.Mvc;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaleItemsController : ControllerBase
    {
        private readonly ISaleItemService _service;
        private readonly ILogger<SaleItemsController> _logger;

        public SaleItemsController(ISaleItemService service, ILogger<SaleItemsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllSaleItemsAsync();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _service.GetSaleItemByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("by-sale/{saleId:guid}")]
        public async Task<IActionResult> GetBySaleId(Guid saleId)
        {
            var items = await _service.GetSaleItemsBySaleIdAsync(saleId);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaleItem saleItem)
        {
            try
            {
                var affected = await _service.CreateSaleItemAsync(saleItem);
                if (affected <= 0)
                    return BadRequest("Não foi possível criar o item da venda.");

                return CreatedAtAction(nameof(GetById), new { id = saleItem.SaleItemId }, saleItem);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação CreateSaleItem inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar sale item");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SaleItem saleItem)
        {
            if (id != saleItem.SaleItemId)
                return BadRequest("Id do caminho diferente do corpo.");

            try
            {
                var affected = await _service.UpdateSaleItemAsync(saleItem);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação UpdateSaleItem inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar sale item");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var affected = await _service.DeleteSaleItemAsync(id);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação DeleteSaleItem inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar sale item");
                return StatusCode(500, "Erro interno.");
            }
        }
    }
}
