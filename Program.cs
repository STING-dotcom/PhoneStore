using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.EnableForHttps = true;
});

var provider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseResponseCompression();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var isSqlite = db.Database.ProviderName!.Contains("Sqlite", StringComparison.OrdinalIgnoreCase);
    db.Database.EnsureCreated();

    if (isSqlite)
    {
        void EnsureTableSqlite(string tableName, string createSql)
        {
            var exists = db.Database.SqlQuery<int>($@"
                SELECT COUNT(*) AS [Value] FROM sqlite_master
                WHERE type='table' AND name={tableName}").FirstOrDefault() == 1;
            if (!exists)
                db.Database.ExecuteSqlRaw(createSql);
        }

        void EnsureColumnSqlite(string table, string column, string alterSql)
        {
            var exists = db.Database.SqlQuery<int>($@"
                SELECT COUNT(*) AS [Value] FROM pragma_table_info({table})
                WHERE name={column}").FirstOrDefault() == 1;
            if (!exists)
                db.Database.ExecuteSqlRaw(alterSql);
        }

        EnsureTableSqlite("Reviews", @"
            CREATE TABLE IF NOT EXISTS Reviews (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                ReviewerName TEXT NOT NULL DEFAULT 'Khách Hàng',
                ReviewerUserId INTEGER NULL,
                Rating INTEGER NOT NULL DEFAULT 5,
                Comment TEXT NOT NULL DEFAULT '',
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            )");

        EnsureColumnSqlite("Reviews", "ReviewerUserId",
            "ALTER TABLE Reviews ADD COLUMN ReviewerUserId INTEGER NULL");

        EnsureTableSqlite("WalletTransactions", @"
            CREATE TABLE IF NOT EXISTS WalletTransactions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
                Amount REAL NOT NULL,
                Type TEXT NOT NULL DEFAULT '',
                Method TEXT NOT NULL DEFAULT '',
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                BalanceBefore REAL NOT NULL DEFAULT 0,
                BalanceAfter REAL NOT NULL DEFAULT 0,
                Status TEXT NOT NULL DEFAULT 'Hoàn thành'
            )");

        EnsureTableSqlite("Orders", @"
            CREATE TABLE IF NOT EXISTS Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NULL REFERENCES Users(Id) ON DELETE SET NULL,
                CustomerName TEXT NOT NULL DEFAULT '',
                Phone TEXT NOT NULL DEFAULT '',
                Email TEXT NOT NULL DEFAULT '',
                Address TEXT NOT NULL DEFAULT '',
                Note TEXT NOT NULL DEFAULT '',
                OrderDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                TotalAmount REAL NOT NULL DEFAULT 0,
                Status TEXT NOT NULL DEFAULT 'Chờ xác nhận',
                PaymentMethod TEXT NOT NULL DEFAULT 'COD',
                PaymentMethodLabel TEXT NOT NULL DEFAULT 'Tiền mặt (COD)'
            )");

        EnsureTableSqlite("OrderItems", @"
            CREATE TABLE IF NOT EXISTS OrderItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER NOT NULL REFERENCES Orders(Id) ON DELETE CASCADE,
                ProductId INTEGER NOT NULL,
                ProductName TEXT NOT NULL DEFAULT '',
                Price REAL NOT NULL DEFAULT 0,
                Quantity INTEGER NOT NULL DEFAULT 1
            )");
    }
    else
    {
        void EnsureTable(string tableName, string createSql)
        {
            var exists = db.Database.SqlQuery<int>($@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {tableName}
                ) THEN 1 ELSE 0 END AS [Value]").FirstOrDefault() == 1;
            if (!exists)
                db.Database.ExecuteSqlRaw(createSql);
        }

        void EnsureColumn(string table, string column, string alterSql)
        {
            var exists = db.Database.SqlQuery<int>($@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = {table} AND COLUMN_NAME = {column}
                ) THEN 1 ELSE 0 END AS [Value]").FirstOrDefault() == 1;
            if (!exists)
                db.Database.ExecuteSqlRaw(alterSql);
        }

        EnsureTable("Reviews", @"
            CREATE TABLE Reviews (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                ProductId INT NOT NULL,
                ReviewerName NVARCHAR(100) NOT NULL DEFAULT N'Khách Hàng',
                ReviewerUserId INT NULL,
                Rating INT NOT NULL DEFAULT 5,
                Comment NVARCHAR(MAX) NOT NULL DEFAULT '',
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
            )");

        EnsureColumn("Reviews", "ReviewerUserId",
            "ALTER TABLE Reviews ADD ReviewerUserId INT NULL");

        EnsureTable("WalletTransactions", @"
            CREATE TABLE WalletTransactions (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
                Amount DECIMAL(18,2) NOT NULL,
                Type NVARCHAR(100) NOT NULL DEFAULT '',
                Method NVARCHAR(100) NOT NULL DEFAULT '',
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                BalanceBefore DECIMAL(18,2) NOT NULL DEFAULT 0,
                BalanceAfter DECIMAL(18,2) NOT NULL DEFAULT 0,
                Status NVARCHAR(50) NOT NULL DEFAULT N'Hoàn thành'
            )");

        EnsureTable("Orders", @"
            CREATE TABLE Orders (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                UserId INT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE SET NULL,
                CustomerName NVARCHAR(200) NOT NULL DEFAULT '',
                Phone NVARCHAR(50) NOT NULL DEFAULT '',
                Email NVARCHAR(200) NOT NULL DEFAULT '',
                Address NVARCHAR(MAX) NOT NULL DEFAULT '',
                Note NVARCHAR(MAX) NOT NULL DEFAULT '',
                OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
                Status NVARCHAR(50) NOT NULL DEFAULT N'Chờ xác nhận',
                PaymentMethod NVARCHAR(50) NOT NULL DEFAULT 'COD',
                PaymentMethodLabel NVARCHAR(100) NOT NULL DEFAULT N'Tiền mặt (COD)'
            )");

        EnsureTable("OrderItems", @"
            CREATE TABLE OrderItems (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(Id) ON DELETE CASCADE,
                ProductId INT NOT NULL,
                ProductName NVARCHAR(500) NOT NULL DEFAULT '',
                Price DECIMAL(18,2) NOT NULL DEFAULT 0,
                Quantity INT NOT NULL DEFAULT 1
            )");
    }

    var hasNewData = false;

    if (!db.Users.Any(u => u.Email == "Admin123@gmail.com"))
    {
        var hasher = new PasswordHasher<User>();
        var admin = new User
        {
            FullName = "Administrator",
            Email = "Admin123@gmail.com",
            Phone = "0123456789",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123");
        db.Users.Add(admin);
        hasNewData = true;
    }

    if (!db.Products.Any())
    {
        var products = PhoneStore.Controllers.ProductController._products;
        if (products.Count > 0)
        {
            db.Products.AddRange(products);
            db.SaveChanges();
        }
        hasNewData = false;
    }

    if (hasNewData)
        db.SaveChanges();

    if (!db.Reviews.Any())
    {
        db.Reviews.AddRange(new[]
        {
            new Review { ProductId = 1, ReviewerName = "Thành Nam", Rating = 5, Comment = "Sản phẩm tuyệt vời! Thiết kế sang trọng, hiệu năng mượt mà.", CreatedAt = DateTime.Now.AddDays(-1) },
            new Review { ProductId = 1, ReviewerName = "Quốc Bảo", Rating = 4, Comment = "Máy đẹp, pin trâu. Đáng đồng tiền!", CreatedAt = DateTime.Now.AddDays(-3) },
            new Review { ProductId = 2, ReviewerName = "Minh Tuấn", Rating = 5, Comment = "Sản phẩm tốt, đóng gói cẩn thận.", CreatedAt = DateTime.Now.AddDays(-5) },
            new Review { ProductId = 11, ReviewerName = "Hoàng Yến", Rating = 5, Comment = "Điện thoại đẹp xuất sắc, AI thông minh vượt trội!", CreatedAt = DateTime.Now.AddDays(-2) },
            new Review { ProductId = 21, ReviewerName = "Đức Anh", Rating = 5, Comment = "Camera Leica quá đỉnh, chụp đêm siêu nét.", CreatedAt = DateTime.Now.AddDays(-7) },
            new Review { ProductId = 3, ReviewerName = "Minh Thư", Rating = 4, Comment = "Màn hình quá đẹp, pin dùng cả ngày!", CreatedAt = DateTime.Now.AddDays(-4) },
            new Review { ProductId = 5, ReviewerName = "Trí Dũng", Rating = 3, Comment = "Sản phẩm ổn, nhưng giá hơi cao.", CreatedAt = DateTime.Now.AddDays(-10) },
            new Review { ProductId = 22, ReviewerName = "Ngọc Ánh", Rating = 5, Comment = "Xiaomi 14 quá ngon so với giá!", CreatedAt = DateTime.Now.AddDays(-6) },
        });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
