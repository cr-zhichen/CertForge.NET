using System.Reflection;
using CertForge.NET.Data;
using CertForge.NET.Enum;
using CertForge.NET.Filter;
using CertForge.NET.Infrastructure;
using CertForge.NET.Jwt;
using CertForge.NET.Middleware;
using CertForge.NET.Utils;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

StartupArt.Print();

#region 应用构建器与配置

//如果是开发环境则使用开发环境配置
var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

var baseDirectory = Path.GetDirectoryName(AppContext.BaseDirectory)!;

if (isDevelopment)
{
    baseDirectory = Path.Combine(Directory.GetCurrentDirectory());
}

// 配置Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(baseDirectory, "Logs/AllLogs/Log.txt"), rollingInterval: RollingInterval.Day)
    .WriteTo.File(Path.Combine(baseDirectory, "Logs/Information/Log-Information-.txt"),
        restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Day)
    .WriteTo.File(Path.Combine(baseDirectory, "Logs/Warning/Log-Warning-.txt"),
        restrictedToMinimumLevel: LogEventLevel.Warning, rollingInterval: RollingInterval.Day)
    .WriteTo.File(Path.Combine(baseDirectory, "Logs/Error/Log-Error-.txt"),
        restrictedToMinimumLevel: LogEventLevel.Error, rollingInterval: RollingInterval.Day)
    .CreateLogger();

// 如果没有wwwroot文件夹则创建
if (!Directory.Exists(Path.Combine(baseDirectory, "wwwroot")))
{
    Directory.CreateDirectory(Path.Combine(baseDirectory, "wwwroot"));
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = baseDirectory,
});

// 使用Serilog作为日志提供程序
builder.Host.UseSerilog();

#endregion

#region 响应压缩配置

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    // 这里可以添加更多 MIME 类型 
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
        { "text/plain", "text/html", "application/json" });
});

#endregion

#region 跨域设置配置

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        corsPolicyBuilder => corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

#endregion

#region MVC配置及过滤器

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomerExceptionFilter>();
    options.Filters.Add(typeof(ModelValidateActionFilterAttribute));
});

builder.Services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);

#endregion

#region JWT配置

var section = builder.Configuration.GetSection("TokenOptions");
var tokenOptions = section.Get<TokenOptions>()!;

// 检查配置并生成随机值
if (string.IsNullOrEmpty(tokenOptions.SecretKey))
    tokenOptions.SecretKey = Guid.NewGuid().ToString();
if (string.IsNullOrEmpty(tokenOptions.Issuer))
    tokenOptions.Issuer = Guid.NewGuid().ToString();
if (string.IsNullOrEmpty(tokenOptions.Audience))
    tokenOptions.Audience = Guid.NewGuid().ToString();

builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.Configure<TokenOptions>(options =>
{
    options.SecretKey = tokenOptions.SecretKey;
    options.Issuer = tokenOptions.Issuer;
    options.Audience = tokenOptions.Audience;
    options.ExpireMinutes = tokenOptions.ExpireMinutes;
});

builder.Services.AddHostedService<TokenCleanupService>();

#endregion

#region Swagger配置

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "CertForge.NET",
        Version = "v1",
        Description = @"
使用.NET 开发的简易自签发证书工具
<br/>
<a href='/'>前端页面</a>
<a href='https://github.com/cr-zhichen/CertForge.NET'>项目地址</a>
",
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "使用 Bearer 方案的 JWT 授权标头。",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "bearer",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new List<string>()
        }
    });

    // 使用全命名空间的类名作为SchemaId
    // c.CustomSchemaIds(x => x.FullName);
});

#endregion

#region 依赖注入

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // 如果在开发环境中使用了代理服务器，需要添加下面的代码以防止环回网络地址的错误
    // options.KnownNetworks.Clear();
    // options.KnownProxies.Clear();
});

#endregion

#region 数据库连接配置

var databaseOptions = (builder.Configuration.GetSection("DefaultConnection").Get<string>() ?? "").ToLower();

if (databaseOptions == nameof(DatabaseType.Mysql).ToLower())
{
    Console.WriteLine("使用MySQL数据库");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection"))));
}
else if (databaseOptions == nameof(DatabaseType.Postgresql).ToLower())
{
    Console.WriteLine("使用PostgreSQL数据库");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")));
}
else if (databaseOptions == nameof(DatabaseType.Sqlite).ToLower())
{
    Console.WriteLine("使用Sqlite数据库");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
}
else if (databaseOptions == nameof(DatabaseType.Sqlserver).ToLower())
{
    Console.WriteLine("使用SqlServer数据库");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));
}
else
{
    Console.WriteLine("使用Sqlite数据库");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={Path.Combine(baseDirectory, "App.db")}"));
}

#endregion

#region Scrutor自动注入

builder.Services.Scan(scan => scan
    // 指定要扫描的程序集（假设服务和仓库都在当前程序集）
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.AssignableTo<IMarker>())
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);

#endregion

var app = builder.Build();

#region 使用响应压缩中间件

app.UseResponseCompression();

#endregion

#region Swagger中间件配置

var isUseSwagger = (builder.Configuration.GetSection("IsUseSwagger").Get<string>() ?? nameof(UseSwaggerType.Auto))
    .ToLower();

if (isUseSwagger == nameof(UseSwaggerType.True).ToLower() ||
    (isUseSwagger == nameof(UseSwaggerType.Auto).ToLower() && app.Environment.IsDevelopment()))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#endregion

#region 基础中间件配置

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

#endregion

#region 数据库初始化

using var serviceScope = app.Services.CreateScope();
var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
context.Database.EnsureCreated();

#endregion

#region 静态文件配置

var resourcesPath = Path.Combine(Path.Combine(baseDirectory, "Resources"));

if (!Directory.Exists(resourcesPath))
{
    Directory.CreateDirectory(resourcesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(resourcesPath),
    RequestPath = "/resources",
    OnPrepareResponse = ctx => { ctx.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*"); }
});

#endregion

#region 前端配置

int nodeDevPorts = builder.Configuration.GetSection("NodeDevPorts").Get<int>();
nodeDevPorts = nodeDevPorts == 0 ? PortSelection.GetAvailablePort() : nodeDevPorts;

if (nodeDevPorts != -1 && app.Environment.IsDevelopment())
{
    app.UseVueCli(nodeDevPorts);
}
else
{
    // 生产环境 使用vue-cli打包好的静态文件
    // 设置默认文件
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        DefaultFileNames = new List<string> { "index.html" }
    });

    app.UseStaticFiles(); // 静态文件中间件

    app.MapFallbackToFile("index.html");
}

#endregion

app.Run();

// 关闭和刷新日志
Log.CloseAndFlush();