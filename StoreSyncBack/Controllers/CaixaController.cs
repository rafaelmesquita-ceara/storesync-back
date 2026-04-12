using Microsoft.AspNetCore.Mvc;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncBack.Services;

namespace StoreSyncBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaixaController : ControllerBase
    {
        private readonly ICaixaService _service;
        private readonly ILogger<CaixaController> _logger;

        public CaixaController(ICaixaService service, ILogger<CaixaController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            var result = await _service.GetAllAsync(limit, offset);
            return Ok(result);
        }

        [HttpGet("aberto")]
        public async Task<IActionResult> GetAberto()
        {
            var caixa = await _service.GetCaixaAbertoAsync();
            if (caixa == null) return NoContent();
            return Ok(caixa);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var caixa = await _service.GetByIdAsync(id);
            if (caixa == null) return NotFound();
            return Ok(caixa);
        }

        [HttpPost]
        public async Task<IActionResult> Abrir([FromBody] AbrirCaixaRequest request)
        {
            try
            {
                var caixa = await _service.AbrirCaixaAsync(request.ValorAbertura);
                return CreatedAtAction(nameof(GetById), new { id = caixa.CaixaId }, caixa);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação AbrirCaixa inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflito ao abrir caixa");
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao abrir caixa");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPost("{id:guid}/fechar")]
        public async Task<IActionResult> Fechar(Guid id, [FromBody] FecharCaixaRequest request)
        {
            try
            {
                await _service.FecharCaixaAsync(id, request.ValorFechamento);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação FecharCaixa inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflito ao fechar caixa");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fechar caixa");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPost("{id:guid}/movimentacao")]
        public async Task<IActionResult> AddMovimentacao(Guid id, [FromBody] AddMovimentacaoRequest request)
        {
            try
            {
                await _service.AddMovimentacaoAsync(id, request.Tipo, request.Descricao, request.Valor);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação AddMovimentacao inválida");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflito ao registrar movimentação");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar movimentação");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpGet("{id:guid}/report/pdf")]
        public async Task<IActionResult> GetPdf(Guid id, [FromServices] CaixaPdfReportService reportService)
        {
            try
            {
                var bytes = await _service.GerarRelatorioPdfAsync(id);
                return File(bytes, "application/pdf", $"Relatorio_Caixa_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório PDF do caixa");
                return StatusCode(500, "Erro ao gerar relatório.");
            }
        }
    }

    public record AbrirCaixaRequest(decimal ValorAbertura);
    public record FecharCaixaRequest(decimal ValorFechamento);
    public record AddMovimentacaoRequest(int Tipo, string? Descricao, decimal Valor);
}
