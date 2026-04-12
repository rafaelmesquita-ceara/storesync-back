using Bogus;
using SharedModels;

namespace StoreSyncFront.Tests.Fixtures;

public static class TestData
{
    private static readonly Faker Faker = new("pt_BR");

    #region Category

    private static readonly Faker<Category> CategoryFaker = new Faker<Category>("pt_BR")
        .RuleFor(c => c.CategoryId, f => Guid.NewGuid())
        .RuleFor(c => c.Name, f => f.Commerce.Categories(1).First())
        .RuleFor(c => c.CreatedAt, f => f.Date.Past());

    public static Category CreateCategory(string? name = null)
    {
        var c = CategoryFaker.Generate();
        if (name != null) c.Name = name;
        return c;
    }

    public static List<Category> CreateCategories(int count = 5) => CategoryFaker.Generate(count);

    #endregion

    #region Product

    private static readonly Faker<Product> ProductFaker = new Faker<Product>("pt_BR")
        .RuleFor(p => p.ProductId, f => Guid.NewGuid())
        .RuleFor(p => p.Reference, f => f.Commerce.Ean13())
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.CategoryId, f => Guid.NewGuid())
        .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000))
        .RuleFor(p => p.StockQuantity, f => f.Random.Int(1, 100));

    public static Product CreateProduct(decimal? price = null, int? stock = null)
    {
        var p = ProductFaker.Generate();
        if (price.HasValue) p.Price = price.Value;
        if (stock.HasValue) p.StockQuantity = stock.Value;
        return p;
    }

    public static List<Product> CreateProducts(int count = 5) => ProductFaker.Generate(count);

    #endregion

    #region Employee

    private static readonly Faker<Employee> EmployeeFaker = new Faker<Employee>("pt_BR")
        .RuleFor(e => e.EmployeeId, f => Guid.NewGuid())
        .RuleFor(e => e.Name, f => f.Name.FullName())
        .RuleFor(e => e.Cpf, f => f.Random.ReplaceNumbers("###########"))
        .RuleFor(e => e.Role, f => f.PickRandom("admin", "user"))
        .RuleFor(e => e.CommissionRate, f => f.Random.Decimal(1, 20))
        .RuleFor(e => e.CreatedAt, f => f.Date.Past());

    public static Employee CreateEmployee() => EmployeeFaker.Generate();
    public static List<Employee> CreateEmployees(int count = 3) => EmployeeFaker.Generate(count);

    #endregion

    #region Client

    private static int _clientCounter;

    private static readonly Faker<Client> ClientFaker = new Faker<Client>("pt_BR")
        .RuleFor(c => c.ClientId, f => Guid.NewGuid())
        .RuleFor(c => c.Reference, _ => $"CLI{(++_clientCounter):D5}")
        .RuleFor(c => c.Name, f => f.Name.FullName())
        .RuleFor(c => c.CpfCnpj, f => f.Random.ReplaceNumbers("###########"))
        .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("(##) #####-####"))
        .RuleFor(c => c.Email, f => f.Internet.Email())
        .RuleFor(c => c.Address, f => f.Address.StreetName())
        .RuleFor(c => c.AddressNumber, f => f.Address.BuildingNumber())
        .RuleFor(c => c.City, f => f.Address.City())
        .RuleFor(c => c.State, f => f.Address.StateAbbr())
        .RuleFor(c => c.PostalCode, f => f.Random.ReplaceNumbers("#####-###"))
        .RuleFor(c => c.Status, _ => ClientStatus.Ativo)
        .RuleFor(c => c.CreatedAt, f => f.Date.Past())
        .RuleFor(c => c.UpdatedAt, f => f.Date.Past());

    public static Client CreateClient(int status = ClientStatus.Ativo)
    {
        var c = ClientFaker.Generate();
        c.Status = status;
        return c;
    }

    public static List<Client> CreateClients(int count = 5) => ClientFaker.Generate(count);

    #endregion

    #region Finance

    private static readonly Faker<Finance> FinanceFaker = new Faker<Finance>("pt_BR")
        .RuleFor(f => f.FinanceId, f => Guid.NewGuid())
        .RuleFor(f => f.Reference, f => f.Finance.Account())
        .RuleFor(f => f.Description, f => f.Lorem.Sentence())
        .RuleFor(f => f.Amount, f => f.Random.Decimal(100, 10000))
        .RuleFor(f => f.DueDate, f => f.Date.Future())
        .RuleFor(f => f.Status, _ => FinanceStatus.Aberto)
        .RuleFor(f => f.Type, _ => FinanceType.Pagar)
        .RuleFor(f => f.TitleType, _ => FinanceTitleType.Original)
        .RuleFor(f => f.CreatedAt, f => f.Date.Past());

    public static Finance CreateFinance(int status = FinanceStatus.Aberto, int type = FinanceType.Pagar)
    {
        var f = FinanceFaker.Generate();
        f.Status = status;
        f.Type = type;
        return f;
    }

    public static List<Finance> CreateFinances(int count = 5, int type = FinanceType.Pagar)
        => FinanceFaker.Generate(count).Select(f => { f.Type = type; return f; }).ToList();

    #endregion

    #region Commission

    private static int _commissionCounter;

    private static readonly Faker<Commission> CommissionFaker = new Faker<Commission>("pt_BR")
        .RuleFor(c => c.CommissionId, f => Guid.NewGuid())
        .RuleFor(c => c.EmployeeId, f => Guid.NewGuid())
        .RuleFor(c => c.Reference, _ => (++_commissionCounter).ToString("D3"))
        .RuleFor(c => c.StartDate, f => f.Date.Past(1).Date)
        .RuleFor(c => c.EndDate, (f, c) => c.StartDate.AddDays(f.Random.Int(1, 30)))
        .RuleFor(c => c.CommissionRate, f => f.Random.Decimal(1, 20))
        .RuleFor(c => c.TotalSales, f => f.Random.Decimal(1000, 50000))
        .RuleFor(c => c.CommissionValue, f => f.Random.Decimal(50, 5000));

    public static Commission CreateCommission(Guid? employeeId = null)
    {
        var c = CommissionFaker.Generate();
        if (employeeId.HasValue) c.EmployeeId = employeeId.Value;
        return c;
    }

    public static List<Commission> CreateCommissions(int count = 5) => CommissionFaker.Generate(count);

    #endregion

    public static PaginatedResult<T> Paginate<T>(List<T> items, int limit = 50, int offset = 0)
        => new() { Items = items, TotalCount = items.Count, Limit = limit, Offset = offset };
}
