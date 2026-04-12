using FluentAssertions;
using Moq;
using SharedModels;
using SharedModels.Interfaces;
using StoreSyncFront.Tests.Fixtures;
using StoreSyncFront.ViewModels;

namespace StoreSyncFront.Tests.Unit.ViewModels;

public class CommissionsViewModelTests
{
    private readonly Mock<ICommissionService> _commissionServiceMock;
    private readonly Mock<IEmployeeService> _employeeServiceMock;
    private readonly Mock<IFinanceService> _financeServiceMock;
    private readonly CommissionsViewModel _vm;

    public CommissionsViewModelTests()
    {
        _commissionServiceMock = new Mock<ICommissionService>();
        _employeeServiceMock   = new Mock<IEmployeeService>();
        _financeServiceMock    = new Mock<IFinanceService>();

        // Stubs padrão de listagem
        _commissionServiceMock.Setup(s => s.GetAllCommissionsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Commission>()));
        _employeeServiceMock.Setup(s => s.GetAllEmployeesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Employee>()));

        _vm = new CommissionsViewModel(
            _commissionServiceMock.Object,
            _employeeServiceMock.Object,
            _financeServiceMock.Object);
    }

    #region LoadDataAsync

    [Fact]
    public async Task LoadDataAsync_ComComissoes_PopulaCollectionECalculaPaginas()
    {
        // Arrange
        var commissions = TestData.CreateCommissions(3);
        _commissionServiceMock.Setup(s => s.GetAllCommissionsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(commissions));

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.Commissions.Should().HaveCount(3);
        _vm.TotalCount.Should().Be(3);
        _vm.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task LoadDataAsync_CarregaFuncionarios()
    {
        // Arrange
        var employees = TestData.CreateEmployees(2);
        _employeeServiceMock.Setup(s => s.GetAllEmployeesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(employees));

        // Act
        await _vm.LoadDataAsync();

        // Assert
        _vm.Employees.Should().HaveCount(2);
    }

    #endregion

    #region OpenNew

    [Fact]
    public void OpenNew_Sempre_LimpaFormularioEAbreEdicao()
    {
        // Arrange — estado sujo
        _vm.Reference = "001";
        _vm.IsEdit = false;

        // Act
        _vm.OpenNewCommand.Execute(null);

        // Assert
        _vm.IsEdit.Should().BeTrue();
        _vm.IsViewOnly.Should().BeFalse();
        _vm.IsPreview.Should().BeFalse();
    }

    #endregion

    #region OpenView

    [Fact]
    public async Task OpenView_ComissaoExistente_PopulaFormularioModoVisualizacao()
    {
        // Arrange — StartDate/EndDate UTC para evitar conflito com DateTimeOffset(DateTime, TimeSpan.Zero)
        var employee = TestData.CreateEmployee();
        var commission = TestData.CreateCommission(employeeId: employee.EmployeeId);
        commission.StartDate = DateTime.SpecifyKind(commission.StartDate, DateTimeKind.Utc);
        commission.EndDate   = DateTime.SpecifyKind(commission.EndDate,   DateTimeKind.Utc);

        _commissionServiceMock.Setup(s => s.GetAllCommissionsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Commission> { commission }));
        _employeeServiceMock.Setup(s => s.GetAllEmployeesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Employee> { employee }));

        await _vm.LoadDataAsync();

        // Act
        _vm.OpenViewCommand.Execute(commission.CommissionId);

        // Assert
        _vm.IsEdit.Should().BeTrue();
        _vm.IsViewOnly.Should().BeTrue();
        _vm.IsPreview.Should().BeTrue();
        _vm.Reference.Should().Be(commission.Reference);
    }

    #endregion

    #region ShowConfirmButton

    [Fact]
    public void ShowConfirmButton_IsPreviewTrueEIsViewOnlyFalse_Verdadeiro()
    {
        _vm.IsPreview  = true;
        _vm.IsViewOnly = false;
        _vm.ShowConfirmButton.Should().BeTrue();
    }

    [Fact]
    public void ShowConfirmButton_IsPreviewFalse_Falso()
    {
        _vm.IsPreview  = false;
        _vm.IsViewOnly = false;
        _vm.ShowConfirmButton.Should().BeFalse();
    }

    [Fact]
    public void ShowConfirmButton_IsViewOnlyTrue_Falso()
    {
        _vm.IsPreview  = true;
        _vm.IsViewOnly = true;
        _vm.ShowConfirmButton.Should().BeFalse();
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_IdValido_ChamaDeleteERecarrega()
    {
        // Arrange
        var id = Guid.NewGuid();
        _commissionServiceMock.Setup(s => s.DeleteCommissionAsync(id)).ReturnsAsync(1);

        // Act
        await _vm.DeleteCommand.ExecuteAsync(id);

        // Assert
        _commissionServiceMock.Verify(s => s.DeleteCommissionAsync(id), Times.Once);
    }

    #endregion

    #region ClearForm

    [Fact]
    public void ClearForm_Sempre_ResetaTodoOEstado()
    {
        // Arrange
        _vm.Reference  = "099";
        _vm.IsEdit     = true;
        _vm.IsPreview  = true;
        _vm.IsViewOnly = true;

        // Act
        _vm.ClearFormCommand.Execute(null);

        // Assert
        _vm.Reference.Should().BeEmpty();
        _vm.IsEdit.Should().BeFalse();
        _vm.IsPreview.Should().BeFalse();
        _vm.IsViewOnly.Should().BeFalse();
        _vm.SelectedEmployee.Should().BeNull();
        _vm.StartDate.Should().BeNull();
        _vm.EndDate.Should().BeNull();
    }

    #endregion

    #region ConfirmCreateAsync

    [Fact]
    public async Task ConfirmCreateAsync_DadosValidos_ChamaCreateCommissionELimpaFormulario()
    {
        // Arrange
        var employee = TestData.CreateEmployee();
        _vm.SelectedEmployee = employee;
        _vm.Reference  = "001";
        _vm.StartDate  = DateTimeOffset.Now.AddDays(-30);
        _vm.EndDate    = DateTimeOffset.Now;

        _commissionServiceMock.Setup(s => s.CreateCommissionAsync(It.IsAny<Commission>())).ReturnsAsync(1);

        // Act
        await _vm.ConfirmCreateAsync();

        // Assert
        _commissionServiceMock.Verify(s => s.CreateCommissionAsync(It.Is<Commission>(
            c => c.EmployeeId == employee.EmployeeId && c.Reference == "001")), Times.Once);
        _vm.IsEdit.Should().BeFalse();
    }

    #endregion

    #region CreateFinanceForCommissionAsync

    [Fact]
    public async Task CreateFinanceForCommissionAsync_Sucesso_RetornaFinanceId()
    {
        // Arrange
        _vm.Reference = "001";
        _financeServiceMock.Setup(s => s.CreateFinanceAsync(It.IsAny<Finance>())).ReturnsAsync(0);

        // Act
        var result = await _vm.CreateFinanceForCommissionAsync();

        // Assert
        result.Should().NotBeNull();
        _financeServiceMock.Verify(s => s.CreateFinanceAsync(
            It.Is<Finance>(f => f.Reference == "COM001" && f.Type == FinanceType.Pagar)), Times.Once);
    }

    [Fact]
    public async Task CreateFinanceForCommissionAsync_Falha_RetornaNull()
    {
        // Arrange
        _vm.Reference = "002";
        _financeServiceMock.Setup(s => s.CreateFinanceAsync(It.IsAny<Finance>())).ReturnsAsync(1);

        // Act
        var result = await _vm.CreateFinanceForCommissionAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Search

    [Fact]
    public async Task Search_TermoEncontrado_FiltraCollection()
    {
        // Arrange
        var e1 = TestData.CreateEmployee(); e1.Name = "Bruno Alves";
        var e2 = TestData.CreateEmployee(); e2.Name = "Carla Matos";
        var c1 = TestData.CreateCommission(employeeId: e1.EmployeeId);
        var c2 = TestData.CreateCommission(employeeId: e2.EmployeeId);
        c1.Reference = "001";
        c2.Reference = "002";

        _commissionServiceMock.Setup(s => s.GetAllCommissionsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Commission> { c1, c2 }));
        _employeeServiceMock.Setup(s => s.GetAllEmployeesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(new List<Employee> { e1, e2 }));
        await _vm.LoadDataAsync();

        // Act
        _vm.SearchBarField = "001";
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Commissions.Should().HaveCount(1);
        _vm.Commissions[0].Reference.Should().Be("001");
    }

    [Fact]
    public async Task Search_TermoVazio_RestauraTodas()
    {
        // Arrange
        var commissions = TestData.CreateCommissions(3);
        _commissionServiceMock.Setup(s => s.GetAllCommissionsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(TestData.Paginate(commissions));
        await _vm.LoadDataAsync();
        _vm.SearchBarField = "xyz";
        _vm.SearchCommand.Execute(null);

        // Act
        _vm.SearchBarField = string.Empty;
        _vm.SearchCommand.Execute(null);

        // Assert
        _vm.Commissions.Should().HaveCount(3);
    }

    #endregion

    #region Paginação

    [Fact]
    public void CanPreviousPage_PrimeiraPagina_Falso()
    {
        _vm.CurrentPage = 1;
        _vm.TotalPages  = 3;
        _vm.CanPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void CanNextPage_TemProximaPagina_Verdadeiro()
    {
        _vm.CurrentPage = 1;
        _vm.TotalPages  = 2;
        _vm.CanNextPage.Should().BeTrue();
    }

    #endregion
}
