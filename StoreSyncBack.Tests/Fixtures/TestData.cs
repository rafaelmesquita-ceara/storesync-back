using Bogus;
using SharedModels;

namespace StoreSyncBack.Tests.Fixtures
{
    /// <summary>
    /// Gera dados de teste fake usando Bogus
    /// </summary>
    public static class TestData
    {
        // Configuração padrão do Bogus para português
        private static readonly Faker Faker = new("pt_BR");

        #region Category

        private static readonly Faker<Category> CategoryFaker = new Faker<Category>("pt_BR")
            .RuleFor(c => c.CategoryId, f => Guid.NewGuid())
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1).First())
            .RuleFor(c => c.CreatedAt, f => f.Date.Past());

        public static Category CreateCategory(string? name = null)
        {
            var category = CategoryFaker.Generate();
            if (name != null)
                category.Name = name;
            return category;
        }

        public static List<Category> CreateCategories(int count = 5) => CategoryFaker.Generate(count);

        #endregion

        #region Employee

        private static readonly Faker<Employee> EmployeeFaker = new Faker<Employee>("pt_BR")
            .RuleFor(e => e.EmployeeId, f => Guid.NewGuid())
            .RuleFor(e => e.Name, f => f.Name.FullName())
            .RuleFor(e => e.Cpf, f => f.Random.ReplaceNumbers("###########"))
            .RuleFor(e => e.Role, f => f.PickRandom("admin", "user", "manager"))
            .RuleFor(e => e.CommissionRate, f => f.Random.Decimal(0, 20))
            .RuleFor(e => e.CreatedAt, f => f.Date.Past());

        public static Employee CreateEmployee(string? role = null)
        {
            var employee = EmployeeFaker.Generate();
            if (role != null)
                employee.Role = role;
            return employee;
        }

        public static List<Employee> CreateEmployees(int count = 5) => EmployeeFaker.Generate(count);

        #endregion

        #region User

        private static readonly Faker<User> UserFaker = new Faker<User>("pt_BR")
            .RuleFor(u => u.UserId, f => Guid.NewGuid())
            .RuleFor(u => u.Login, f => f.Internet.UserName())
            .RuleFor(u => u.Password, f => f.Internet.Password(10))
            .RuleFor(u => u.EmployeeId, f => Guid.NewGuid());

        public static User CreateUser(string? login = null, string? password = null)
        {
            var user = UserFaker.Generate();
            if (login != null)
                user.Login = login;
            if (password != null)
                user.Password = password;
            return user;
        }

        public static List<User> CreateUsers(int count = 5) => UserFaker.Generate(count);

        #endregion

        #region Product

        private static readonly Faker<Product> ProductFaker = new Faker<Product>("pt_BR")
            .RuleFor(p => p.ProductId, f => Guid.NewGuid())
            .RuleFor(p => p.Reference, f => f.Commerce.Ean13())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.CategoryId, f => Guid.NewGuid())
            .RuleFor(p => p.Price, f => f.Random.Decimal(10, 1000))
            .RuleFor(p => p.StockQuantity, f => f.Random.Int(0, 100));

        public static Product CreateProduct(decimal? price = null, int? stock = null)
        {
            var product = ProductFaker.Generate();
            if (price.HasValue)
                product.Price = price.Value;
            if (stock.HasValue)
                product.StockQuantity = stock.Value;
            return product;
        }

        public static List<Product> CreateProducts(int count = 5) => ProductFaker.Generate(count);

        #endregion

        #region Sale

        private static int _saleReferenciaCounter;

        private static readonly Faker<Sale> SaleFaker = new Faker<Sale>("pt_BR")
            .RuleFor(s => s.SaleId, f => Guid.NewGuid())
            .RuleFor(s => s.Referencia, _ => (++_saleReferenciaCounter).ToString("D5"))
            .RuleFor(s => s.EmployeeId, f => Guid.NewGuid())
            .RuleFor(s => s.Discount, _ => 0)
            .RuleFor(s => s.Addition, _ => 0)
            .RuleFor(s => s.TotalAmount, f => f.Random.Decimal(50, 5000))
            .RuleFor(s => s.Status, _ => SaleStatus.Aberta)
            .RuleFor(s => s.SaleDate, f => f.Date.Past());

        public static Sale CreateSale(decimal? totalAmount = null, int status = SaleStatus.Aberta)
        {
            var sale = SaleFaker.Generate();
            sale.Status = status;
            if (totalAmount.HasValue)
                sale.TotalAmount = totalAmount.Value;
            return sale;
        }

        public static List<Sale> CreateSales(int count = 5) => SaleFaker.Generate(count);

        #endregion

        #region SaleItem

        private static readonly Faker<SaleItem> SaleItemFaker = new Faker<SaleItem>("pt_BR")
            .RuleFor(si => si.SaleItemId, f => Guid.NewGuid())
            .RuleFor(si => si.SaleId, f => Guid.NewGuid())
            .RuleFor(si => si.ProductId, f => Guid.NewGuid())
            .RuleFor(si => si.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(si => si.Discount, _ => 0)
            .RuleFor(si => si.Addition, _ => 0)
            .RuleFor(si => si.TotalPrice, f => f.Random.Decimal(10, 500));

        public static SaleItem CreateSaleItem(int? quantity = null)
        {
            var item = SaleItemFaker.Generate();
            if (quantity.HasValue)
                item.Quantity = quantity.Value;
            return item;
        }

        public static List<SaleItem> CreateSaleItems(int count = 5) => SaleItemFaker.Generate(count);

        #endregion

        #region Commission

        private static int _commissionReferenceCounter;

        private static readonly Faker<Commission> CommissionFaker = new Faker<Commission>("pt_BR")
            .RuleFor(c => c.CommissionId, f => Guid.NewGuid())
            .RuleFor(c => c.EmployeeId, f => Guid.NewGuid())
            .RuleFor(c => c.Reference, _ => (++_commissionReferenceCounter).ToString("D3"))
            .RuleFor(c => c.StartDate, f => f.Date.Past(1).Date)
            .RuleFor(c => c.EndDate, (f, c) => c.StartDate.AddDays(f.Random.Int(1, 30)))
            .RuleFor(c => c.CommissionRate, f => f.Random.Decimal(1, 20))
            .RuleFor(c => c.TotalSales, f => f.Random.Decimal(1000, 50000))
            .RuleFor(c => c.CommissionValue, f => f.Random.Decimal(50, 5000))
            .RuleFor(c => c.Observation, f => f.Lorem.Sentence());

        public static Commission CreateCommission(string? reference = null, Guid? employeeId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var commission = CommissionFaker.Generate();
            if (reference != null) commission.Reference = reference;
            if (employeeId.HasValue) commission.EmployeeId = employeeId.Value;
            if (startDate.HasValue) commission.StartDate = startDate.Value;
            if (endDate.HasValue) commission.EndDate = endDate.Value;
            return commission;
        }

        public static List<Commission> CreateCommissions(int count = 5) => CommissionFaker.Generate(count);

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
            var finance = FinanceFaker.Generate();
            finance.Status = status;
            finance.Type = type;
            return finance;
        }

        public static Finance CreateFinanceWithAmount(decimal amount, int status = FinanceStatus.Aberto)
        {
            var finance = FinanceFaker.Generate();
            finance.Amount = amount;
            finance.Status = status;
            return finance;
        }

        public static List<Finance> CreateFinances(int count = 5) => FinanceFaker.Generate(count);

        #endregion

        #region Client

        private static int _clientReferenceCounter;

        private static readonly Faker<Client> ClientFaker = new Faker<Client>("pt_BR")
            .RuleFor(c => c.ClientId, f => Guid.NewGuid())
            .RuleFor(c => c.Reference, _ => $"CLI{(++_clientReferenceCounter):D5}")
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.CpfCnpj, f => f.Random.ReplaceNumbers("###########"))
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("(##) #####-####"))
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.Address, f => f.Address.StreetName())
            .RuleFor(c => c.AddressNumber, f => f.Address.BuildingNumber())
            .RuleFor(c => c.AddressComplement, f => f.Address.SecondaryAddress())
            .RuleFor(c => c.City, f => f.Address.City())
            .RuleFor(c => c.State, f => f.Address.StateAbbr())
            .RuleFor(c => c.PostalCode, f => f.Random.ReplaceNumbers("#####-###"))
            .RuleFor(c => c.Status, _ => ClientStatus.Ativo)
            .RuleFor(c => c.CreatedAt, f => f.Date.Past())
            .RuleFor(c => c.UpdatedAt, f => f.Date.Past());

        public static Client CreateClient(int status = ClientStatus.Ativo)
        {
            var client = ClientFaker.Generate();
            client.Status = status;
            return client;
        }

        public static List<Client> CreateClients(int count = 5) => ClientFaker.Generate(count);

        #endregion

        #region DTOs

        public static UserLoginDto CreateUserLoginDto(string login = "testuser", string password = "testpass123")
            => new() { Login = login, Password = password };

        public static UserChangePasswordDto CreateUserChangePasswordDto(
            Guid userId, string oldPassword = "oldpass123", string newPassword = "newpass123")
            => new() { UserId = userId, OldPassword = oldPassword, NewPassword = newPassword };

        #endregion
    }
}
