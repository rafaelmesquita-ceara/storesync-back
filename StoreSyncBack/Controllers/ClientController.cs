using Microsoft.AspNetCore.Mvc;
using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _service;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IClientService service, ILogger<ClientsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int limit = 50, [FromQuery] int offset = 0)
        {
            var list = await _service.GetAllClientsAsync(limit, offset);
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var client = await _service.GetClientByIdAsync(id);
            if (client == null) return NotFound();
            return Ok(client);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Client client)
        {
            try
            {
                var affected = await _service.CreateClientAsync(client);
                if (affected <= 0)
                    return BadRequest("Não foi possível criar o cliente.");

                return CreatedAtAction(nameof(GetById), new { id = client.ClientId }, client);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação CreateClient inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cliente");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Client client)
        {
            if (id != client.ClientId)
                return BadRequest("Id do caminho diferente do corpo.");

            try
            {
                var affected = await _service.UpdateClientAsync(client);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação UpdateClient inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar cliente");
                return StatusCode(500, "Erro interno.");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var affected = await _service.DeleteClientAsync(id);
                if (affected <= 0) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validação DeleteClient inválida");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar cliente");
                return StatusCode(500, "Erro interno.");
            }
        }
    }
}
