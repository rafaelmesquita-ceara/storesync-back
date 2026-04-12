using SharedModels;
using SharedModels.Interfaces;

namespace StoreSyncBack.Services
{
    public class CaixaService : ICaixaService
    {
        private readonly ICaixaRepository _repo;
        private readonly CaixaPdfReportService _pdfService;

        public CaixaService(ICaixaRepository repo, CaixaPdfReportService pdfService)
        {
            _repo = repo;
            _pdfService = pdfService;
        }

        public Task<PaginatedResult<Caixa>> GetAllAsync(int limit, int offset)
        {
            return _repo.GetAllAsync(limit, offset);
        }

        public Task<Caixa?> GetByIdAsync(Guid id)
        {
            return _repo.GetByIdAsync(id);
        }

        public Task<Caixa?> GetCaixaAbertoAsync()
        {
            return _repo.GetCaixaAbertoAsync();
        }

        public async Task<Caixa> AbrirCaixaAsync(decimal valorAbertura)
        {
            var aberto = await _repo.GetCaixaAbertoAsync();
            if (aberto != null)
                throw new InvalidOperationException("Já existe um caixa aberto. Feche o caixa atual antes de abrir um novo.");

            var caixa = new Caixa
            {
                CaixaId = Guid.NewGuid(),
                Referencia = $"CAIXA-{BrazilDateTime.Now:ddMMyyyy-HHmm}",
                ValorAbertura = valorAbertura,
                Status = CaixaStatus.Aberto,
                DataAbertura = BrazilDateTime.Now
            };

            await _repo.CreateAsync(caixa);
            return caixa;
        }

        public async Task FecharCaixaAsync(Guid id, decimal valorFechamento)
        {
            var caixa = await _repo.GetByIdAsync(id);
            if (caixa == null)
                throw new ArgumentException("Caixa não encontrado.");
            if (caixa.Status != CaixaStatus.Aberto)
                throw new InvalidOperationException("Apenas caixas abertos podem ser fechados.");

            var vendas = await _repo.GetVendasByCaixaAsync(id);
            var totalVendas = vendas.Where(v => v.Status == SaleStatus.Finalizada).Sum(v => v.TotalAmount);

            var movimentacoes = await _repo.GetMovimentacoesByCaixaAsync(id);
            var movList = movimentacoes.ToList();
            var totalSangrias = movList.Where(m => m.Tipo == MovimentacaoTipo.Sangria).Sum(m => m.Valor);
            var totalSuprimentos = movList.Where(m => m.Tipo == MovimentacaoTipo.Suprimento).Sum(m => m.Valor);

            var saldoEsperado = caixa.ValorAbertura + totalVendas + totalSuprimentos - totalSangrias;
            var diferenca = valorFechamento - saldoEsperado;

            decimal? valorFaltante = diferenca < 0 ? Math.Abs(diferenca) : null;
            decimal? valorSobra = diferenca > 0 ? diferenca : null;

            var affected = await _repo.FecharAsync(id, valorFechamento, totalVendas, totalSangrias, totalSuprimentos, valorFaltante, valorSobra);
            if (affected <= 0)
                throw new InvalidOperationException("Não foi possível fechar o caixa.");
        }

        public async Task AddMovimentacaoAsync(Guid caixaId, int tipo, string? descricao, decimal valor)
        {
            var caixa = await _repo.GetCaixaAbertoAsync();
            if (caixa == null || caixa.CaixaId != caixaId)
                throw new InvalidOperationException("Caixa não encontrado ou não está aberto.");

            if (valor <= 0)
                throw new ArgumentException("O valor da movimentação deve ser maior que zero.");

            if (tipo != MovimentacaoTipo.Sangria && tipo != MovimentacaoTipo.Suprimento)
                throw new ArgumentException("Tipo de movimentação inválido.");

            var mov = new MovimentacaoCaixa
            {
                MovimentacaoCaixaId = Guid.NewGuid(),
                CaixaId = caixaId,
                Tipo = tipo,
                Descricao = descricao,
                Valor = valor,
                CreatedAt = BrazilDateTime.Now
            };

            await _repo.AddMovimentacaoAsync(mov);
        }

        public async Task<byte[]> GerarRelatorioPdfAsync(Guid caixaId)
        {
            var caixa = await _repo.GetByIdAsync(caixaId);
            if (caixa == null)
                throw new ArgumentException("Caixa não encontrado.");

            return _pdfService.GerarRelatorio(caixa);
        }
    }
}
