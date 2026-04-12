using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Tests.Fixtures;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Tests.Unit.ViewModels;

public class FinancesViewModelTests
{
    private readonly Mock<IFinanceService> _serviceMock;
    private readonly FinancesViewModel _vm;

    public FinancesViewModelTests()
    {
        _serviceMock = new Mock<IFinanceService>();
        _vm = new FinancesViewModel(_serviceMock.Object, FinanceType.Pagar);
    }

    #region Title

    [Fact]
    public void Title_TipoPagar_RetornaContasAPagar()
    {
        var vm = new FinancesViewModel(_serviceMock.Object, FinanceType.Pagar);
        vm.Title.Should().Be("Contas a Pagar");
    }

    [Fact]
    public void Title_TipoReceber_RetornaContasAReceber()
    {
        var vm = new FinancesViewModel(_serviceMock.Object, FinanceType.Receber);
        vm.Title.Should().Be("Contas a Receber");
    }

    #endregion

    #region LoadDataAsync

    [Fact]
    public async Task LoadDataAsync_ComFinances_PopulaCollectionECalculaPaginas()
    {
        // Arrange
        var finances = TestData.CreateFinances(4);
        _serviceMock.Setup(s => s.GetAllByTypeAsync(FinanceType.Pagar, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(finances));

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.Finances.Should().HaveCount(4);
        _vm.TotalCount.Should().Be(4);
        _vm.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task LoadDataAsync_SemRegistros_CollectionVaziaETotalPagesUm()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllByTypeAsync(FinanceType.Pagar, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Finance>()));

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.Finances.Should().BeEmpty();
        _vm.TotalPages.Should().Be(1);
    }

    #endregion

    #region CanSettle / CanCancelSettle

    [Fact]
    public void CanSettle_StatusAberto_Verdadeiro()
    {
        _vm.SelectedStatus = FinanceStatus.Aberto;
        _vm.CanSettle.Should().BeTrue();
    }

    [Fact]
    public void CanSettle_StatusLiquidadoParcialmente_Verdadeiro()
    {
        _vm.SelectedStatus = FinanceStatus.LiquidadoParcialmente;
        _vm.CanSettle.Should().BeTrue();
    }

    [Fact]
    public void CanSettle_StatusLiquidado_Falso()
    {
        _vm.SelectedStatus = FinanceStatus.Liquidado;
        _vm.CanSettle.Should().BeFalse();
    }

    [Fact]
    public void CanCancelSettle_StatusLiquidado_Verdadeiro()
    {
        _vm.SelectedStatus = FinanceStatus.Liquidado;
        _vm.CanCancelSettle.Should().BeTrue();
    }

    [Fact]
    public void CanCancelSettle_StatusAberto_Falso()
    {
        _vm.SelectedStatus = FinanceStatus.Aberto;
        _vm.CanCancelSettle.Should().BeFalse();
    }

    #endregion

    #region OpenEdit

    [Fact]
    public async Task OpenEdit_StatusAberto_PopulaFormularioModoEdicao()
    {
        // Arrange — DueDate UTC para evitar conflito com DateTimeOffset(DateTime, TimeSpan.Zero)
        var finance = TestData.CreateFinance(status: FinanceStatus.Aberto);
        finance.Description = "Conta teste";
        finance.DueDate = DateTime.SpecifyKind(finance.DueDate, DateTimeKind.Utc);
        _serviceMock.Setup(s => s.GetAllByTypeAsync(FinanceType.Pagar, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Finance> { finance }));
        await _vm.LoadDataAsync();

        // Act
        _vm.OpenEditCommand.Execute(finance.FinanceId);

        // Assert
        _vm.FinanceId.Should().Be(finance.FinanceId);
        _vm.Description.Should().Be("Conta teste");
        _vm.IsEdit.Should().BeTrue();
        _vm.IsViewOnly.Should().BeFalse();
    }

    [Fact]
    public async Task OpenEdit_StatusLiquidado_NaoAbreEdicao()
    {
        // Arrange — DueDate UTC para evitar conflito com DateTimeOffset(DateTime, TimeSpan.Zero)
        var finance = TestData.CreateFinance(status: FinanceStatus.Liquidado);
        finance.DueDate = DateTime.SpecifyKind(finance.DueDate, DateTimeKind.Utc);
        _serviceMock.Setup(s => s.GetAllByTypeAsync(FinanceType.Pagar, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Finance> { finance }));
        await _vm.LoadDataAsync();

        // Act — OpenEdit bloqueia contas não-abertas; SnackBarService.Send lança sem UI Avalonia,
        //       por isso capturamos a exceção e verificamos apenas que IsEdit permanece false.
        try { _vm.OpenEditCommand.Execute(finance.FinanceId); } catch { /* SnackBar sem UI */ }

        // Assert
        _vm.IsEdit.Should().BeFalse();
    }

    #endregion

    #region OpenView

    [Fact]
    public async Task OpenView_FinanceExistente_PopulaFormularioModoVisualizacao()
    {
        // Arrange — DueDate UTC para evitar conflito com DateTimeOffset(DateTime, TimeSpan.Zero)
        var finance = TestData.CreateFinance(status: FinanceStatus.Liquidado);
        finance.DueDate = DateTime.SpecifyKind(finance.DueDate, DateTimeKind.Utc);
        finance.SettledAmount = finance.Amount;
        finance.SettledAt = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
        _serviceMock.Setup(s => s.GetAllByTypeAsync(FinanceType.Pagar, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Finance> { finance }));
        await _vm.LoadDataAsync();

        // Act
        _vm.OpenViewCommand.Execute(finance.FinanceId);

        // Assert
        _vm.IsViewOnly.Should().BeTrue();
        _vm.IsEdit.Should().BeTrue();
        _vm.HasSettlementInfo.Should().BeTrue();
    }

    #endregion

    #region Save — criação

    [Fact]
    public async Task Save_DadosValidos_ChamaCreateERecarrega()
    {
        // Arrange
        _vm.Reference = "REF001";
        _vm.Description = "Despesa teste";
        _vm.Amount = "100,00";
        _vm.DueDate = DateTimeOffset.Now.AddDays(5);
        _serviceMock.Setup(s => s.CreateFinanceAsync(It.IsAny<Finance>())).ReturnsAsync(1);
        _serviceMock.Setup(s => s.GetAllByTypeAsync(FinanceType.Pagar, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Finance>()));

        // Act
        await _vm.SaveCommand.ExecuteAsync(null);

        // Assert
        _serviceMock.Verify(s => s.CreateFinanceAsync(It.Is<Finance>(f => f.Description == "Despesa teste")), Times.Once);
    }

    [Fact]
    public async Task Save_DescricaoVazia_NaoChamaCreate()
    {
        // Arrange
        _vm.Reference = "REF001";
        _vm.Description = string.Empty;
        _vm.Amount = "100,00";

        // Act
        await _vm.SaveCommand.ExecuteAsync(null);

        // Assert
        _serviceMock.Verify(s => s.CreateFinanceAsync(It.IsAny<Finance>()), Times.Never);
    }

    #endregion

    #region Save — edição

    [Fact]
    public async Task Save_ComFinanceIdPreenchido_ChamaUpdateEmVezDeCreate()
    {
        // Arrange
        var id = Guid.NewGuid();
        _vm.FinanceId = id;
        _vm.Reference = "REF002";
        _vm.Description = "Despesa atualizada";
        _vm.Amount = "200,00";
        _vm.DueDate = DateTimeOffset.Now.AddDays(10);
        _serviceMock.Setup(s => s.UpdateFinanceAsync(It.IsAny<Finance>())).ReturnsAsync(1);
        _serviceMock.Setup(s => s.GetAllByTypeAsync(FinanceType.Pagar, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Finance>()));

        // Act
        await _vm.SaveCommand.ExecuteAsync(null);

        // Assert
        _serviceMock.Verify(s => s.UpdateFinanceAsync(It.Is<Finance>(f => f.FinanceId == id)), Times.Once);
        _serviceMock.Verify(s => s.CreateFinanceAsync(It.IsAny<Finance>()), Times.Never);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_IdValido_ChamaDeleteERecarrega()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteFinanceAsync(id)).ReturnsAsync(1);
        _serviceMock.Setup(s => s.GetAllByTypeAsync(FinanceType.Pagar, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Finance>()));

        // Act
        await _vm.DeleteCommand.ExecuteAsync(id);

        // Assert
        _serviceMock.Verify(s => s.DeleteFinanceAsync(id), Times.Once);
    }

    #endregion

    #region ClearForm

    [Fact]
    public void ClearForm_Sempre_ResetaEstado()
    {
        // Arrange
        _vm.Description = "Alguma desc";
        _vm.IsEdit = true;
        _vm.IsViewOnly = true;

        // Act
        _vm.ClearFormCommand.Execute(null);

        // Assert
        _vm.Description.Should().BeEmpty();
        _vm.IsEdit.Should().BeFalse();
        _vm.IsViewOnly.Should().BeFalse();
        _vm.FinanceId.Should().Be(Guid.Empty);
    }

    #endregion

    #region Search

    [Fact]
    public async Task Search_TermoEncontrado_FiltraCollection()
    {
        // Arrange
        var f1 = TestData.CreateFinance(); f1.Description = "Aluguel";
        var f2 = TestData.CreateFinance(); f2.Description = "Energia";
        _serviceMock.Setup(s => s.GetAllByTypeAsync(FinanceType.Pagar, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Finance> { f1, f2 }));
        await _vm.LoadDataAsync();

        // Act
        _vm.SearchBarField = "Aluguel";
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Finances.Should().HaveCount(1);
        _vm.Finances[0].Description.Should().Be("Aluguel");
    }

    [Fact]
    public async Task Search_TermoVazio_RestauraTodos()
    {
        // Arrange
        var finances = TestData.CreateFinances(3);
        _serviceMock.Setup(s => s.GetAllByTypeAsync(FinanceType.Pagar, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(finances));
        await _vm.LoadDataAsync();
        _vm.SearchBarField = "xyz";
        _vm.SearchCommand.Execute(null);

        // Act
        _vm.SearchBarField = string.Empty;
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Finances.Should().HaveCount(3);
    }

    #endregion

    #region Paginação

    [Fact]
    public void CanPreviousPage_PrimeiraPagina_Falso()
    {
        _vm.CurrentPage = 1;
        _vm.TotalPages = 3;
        _vm.CanPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void CanNextPage_UltimaPagina_Falso()
    {
        _vm.CurrentPage = 2;
        _vm.TotalPages = 2;
        _vm.CanNextPage.Should().BeFalse();
    }

    #endregion
}
