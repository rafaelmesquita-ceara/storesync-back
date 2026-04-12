using Microsoft.AspNetCore.Mvc;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;

namespace StoreSyncBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _service;
        private readonly ILogger<SalesController> _logger;

        public SalesController(ISaleService service, ILogger<SalesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            var list = await _service.GetAllSalesAsync(limit, offset);
            return Ok(list);
        }

        [HttpGet("report/pdf")]
        public async Task<IActionResult> GetPdfReport(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate, 
            [FromServices] SalesPdfReportService reportService,
            [FromServices] ISaleRepository saleRepository)
        {
            try
            {
                var sales = await saleRepository.GetSalesByPeriodAsync(startDate, endDate);
                var pdfBytes = reportService.GenerateSalesReport(sales, startDate, endDate);
                return File(pdfBytes, "application/pdf", $"Relatorio_Vendas_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório em PDF.");
                return StatusCode(500, "Erro ao gerar relatório.");
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var sale = await _service.GetSaleByIdAsync(id);
            if (sale == null) return NotFound();
            return Ok(sale);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Sale sale)
        {
            try
            {
                var affected = await _service.CreateSaleAsync(sale);
                if (affected <= 0)
                    return BadRequest("Não foi possível registrar a venda.");

                return CreatedAtAction(nameof(GetById), new { id = sale.SaleId }, sale);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação CreateSale inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar venda");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Sale sale)
        {
            if (id != sale.SaleId)
                return BadRequest("Id do caminho diferente do corpo.");

            try
            {
                var affected = await _service.UpdateSaleAsync(sale);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação UpdateSale inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar venda");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPost("{id:guid}/finalize")]
        public async Task<IActionResult> Finalize(Guid id)
        {
            try
            {
                var affected = await _service.FinalizeSaleAsync(id);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação FinalizeSale inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao finalizar venda");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var affected = await _service.CancelSaleAsync(id);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação CancelSale inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar venda");
                return StatusCode(500, "Erro interno.");
            }
        }
    }
}
