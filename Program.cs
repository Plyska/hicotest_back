var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MaterialDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CORSPolicy",
    builder =>
    {
        builder.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:3000");
    });
});

builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddSingleton<ITokenService>(new TokenService());
builder.Services.AddSingleton<IUserRepository>(new UserRepository());
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => 
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MaterialDb>();
    db.Database.EnsureCreated();
}

app.UseCors("CORSPolicy");

app.MapGet("/login", [AllowAnonymous] async (HttpContext context, 
    ITokenService tokenService, IUserRepository userRepository) => 
    {
        UserModel userModel = new()
        {
            UserName = context.Request.Query["username"],
            Email = context.Request.Query["email"],
            Password = context.Request.Query["password"]
        };
        var userDto = userRepository.GetUser(userModel);
        if (userDto == null) return Results.Unauthorized();
        var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"],
            builder.Configuration["JwtIssuer"], userDto);
        return Results.Ok(token);
    });

app.MapGet("/materials", async (IMaterialRepository repository) => Results.Ok(await repository.GetMaterialsAsync()))
    .Produces<List<Material>>(StatusCodes.Status200OK)
    .WithName("GetAllMaterials")
    .WithTags("Getters");
app.MapGet("/materials/{id}", async (int id, IMaterialRepository repository) =>
    await repository.GetMaterialAsync(id) is Material material
    ? Results.Ok(material)
    : Results.NotFound())
    .Produces<Material>(StatusCodes.Status200OK)
    .WithName("GetMaterial")
    .WithTags("Getters");
app.MapGet("/materials/search/partnumber/{query}", 
    [Authorize] async (string query, IMaterialRepository repository) => 
        await repository.GetMaterialsAsync(query) is IEnumerable<Material> materials
            ? Results.Ok(materials)
            : Results.NotFound(Array.Empty<Material>())
    )
    .Produces<List<Material>>(StatusCodes.Status200OK)
    .Produces<List<Material>>(StatusCodes.Status404NotFound)
    .WithName("SearchMaterials")
    .WithTags("Getters");
app.MapPost("/materials", async ([FromBody] Material material, IMaterialRepository repository) =>
    {
        await repository.InsertMaterialAync(material);
        await repository.SaveAsync();
        return Results.Created($"/materials/{material.Id}", material);
    })
    .Accepts<Material>("application/json")
    .Produces<Material>(StatusCodes.Status201Created)
    .WithName("CreateMaterial")
    .WithTags("Creators");
app.MapPut("/materials", async ([FromBody] Material material, IMaterialRepository repository) =>
    {
        await repository.UpdateMaterialAsync(material);
        await repository.SaveAsync();
        return Results.NoContent();
    })
    .Accepts<Material>("application/json")
    .WithName("UpdateMaterial")
    .WithTags("Updaters");
app.MapDelete("/materials/{id}", async (int id, IMaterialRepository repository) =>
    {
        await repository.DeleteMaterialAsync(id);
        await repository.SaveAsync();
        return Results.NoContent();
    })
    .WithName("DeleteMaterial")
    .WithTags("Deleters");

app.UseHttpsRedirection();

app.Run();
