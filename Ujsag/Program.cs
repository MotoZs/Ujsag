
using Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services;
using System.Security.Principal;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApiDocument(config =>
{
    config.AddSecurity("JWT", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Type 'Bearer {your JWT token}' into the field below."
    });

    config.OperationProcessors.Add(new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("JWT"));
});

builder.Services.AddAuthorizationBuilder()
  .AddPolicy("admin", policy => policy.RequireRole("Admin"))
  .AddPolicy("user", policy => policy.RequireRole("User"));


builder.Services.AddEndpointsApiExplorer();

var connectionString = builder.Configuration.GetConnectionString("NewspaperDbContext");
builder.Services.AddDbContext<NewspaperDbContext>(options =>
  options.UseSqlServer(connectionString));


builder.Services.AddIdentityApiEndpoints<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<NewspaperDbContext>();


builder.Services.AddTransient<INewsPaperService, NewsPaperService>();

builder.Services.AddAuthorization();

var allowSpecificOrigins = "_allowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(allowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("*")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                      });
});

var app = builder.Build();

app.UseCors(allowSpecificOrigins);

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}


app.MapGroup("Account").WithTags("Account").MapIdentityApi<IdentityUser>();

// ========================================
// ARTICLE ENDPOINTS
// ========================================

var articlesGroup = app.MapGroup("api/articles").WithTags("Articles");

// GET: api/articles - Public access
articlesGroup.MapGet("/", async (INewsPaperService service) =>
{
    var articles = await service.GetAllArticlesAsync();
    return Results.Ok(articles);
})
.WithName("GetAllArticles")
.WithOpenApi();

// GET: api/articles/{id} - Public access
articlesGroup.MapGet("/{id}", async (int id, INewsPaperService service) =>
{
    var article = await service.GetArticleByIdAsync(id);
    return article == null ? Results.NotFound(new { message = "Article not found" }) : Results.Ok(article);
})
.WithName("GetArticleById")
.WithOpenApi();

// POST: api/articles - Admin only
articlesGroup.MapPost("/", async (Article article, INewsPaperService service) =>
{
    var created = await service.CreateArticleAsync(article);
    return Results.Created();
})
.RequireAuthorization("admin")
.WithName("CreateArticle")
.WithOpenApi();

// PUT: api/articles/{id} - Admin only
articlesGroup.MapPut("/{id}", async (int id, Article article, INewsPaperService service) =>
{
    if (id != article.Id)
        return Results.BadRequest(new { message = "ID mismatch" });

    var result = await service.UpdateArticleAsync(article);
    return result ? Results.NoContent() : Results.NotFound(new { message = "Article not found" });
})
.RequireAuthorization("admin")
.WithName("UpdateArticle")
.WithOpenApi();

// DELETE: api/articles/{id} - Admin only
articlesGroup.MapDelete("/{id}", async (int id, INewsPaperService service) =>
{
    var result = await service.DeleteArticleAsync(id);
    return result ? Results.NoContent() : Results.NotFound(new { message = "Article not found" });
})
.RequireAuthorization("admin")
.WithName("DeleteArticle")
.WithOpenApi();

// ========================================
// AUTHOR ENDPOINTS
// ========================================

var authorsGroup = app.MapGroup("api/authors").WithTags("Authors");

// GET: api/authors/listauthors
authorsGroup.MapGet("/listauthors", async (INewsPaperService service) =>
{
    var authors = await service.GetAllAuthorsAsync();
    return Results.Ok(authors);
})
.WithName("GetAllAuthors")
.WithOpenApi();


// GET: api/authors/{id} - Public access
authorsGroup.MapGet("/{id}", async (int id, INewsPaperService service) =>
{
    var author = await service.GetAuthorByIdAsync(id);
    return author == null ? Results.NotFound(new { message = "Author not found" }) : Results.Ok(author);
})
.WithName("GetAuthorById")
.WithOpenApi();

// POST: api/authors - Admin only
authorsGroup.MapPost("/", async (Author author, INewsPaperService service) =>
{
    var created = await service.CreateAuthorAsync(author);
    return Results.Created($"/api/authors/{created.Id}", created);
})
.RequireAuthorization("admin")
.WithName("CreateAuthor")
.WithOpenApi();

using var scope = app.Services.CreateScope();
using var dbContext = scope.ServiceProvider.GetRequiredService<NewspaperDbContext>();
var migrations = await dbContext.Database.GetPendingMigrationsAsync();
if (migrations.Any())
    await dbContext.Database.MigrateAsync();

// Seed Admin Role and User
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

// Create Admin role if it doesn't exist
if (!await roleManager.RoleExistsAsync("Admin"))
{
    await roleManager.CreateAsync(new IdentityRole("Admin"));
}

// Create User role if it doesn't exist
if (!await roleManager.RoleExistsAsync("User"))
{
    await roleManager.CreateAsync(new IdentityRole("User"));
}

// Create default admin user
var adminEmail = "admin@newspaper.com";
var adminUser = await userManager.FindByEmailAsync(adminEmail);
if (adminUser == null)
{
    adminUser = new IdentityUser
    {
        UserName = adminEmail,
        Email = adminEmail,
        EmailConfirmed = true
    };
    var result = await userManager.CreateAsync(adminUser, "Admin123!");
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}



app.Run();

