using HotelsWebApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});
builder.Services.AddScoped<IHotelRepository, HotelRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.EnsureCreated();
}


app.MapGet("/hotels", async (IHotelRepository repos) =>
    await repos.GetHotelsAsync())
        .Produces<List<Hotel>>(StatusCodes.Status200OK)
        .WithName("GetAllHotels")
        .WithTags("Getters");

app.MapGet("/hotels/{id}", async (int id, IHotelRepository repos) =>
    {
        return await repos.GetHotelAsync(id) is Hotel hotel ?
            Results.Ok(hotel) :
            Results.BadRequest();
    })
    .Produces<Hotel>(StatusCodes.Status200OK)
    .WithName("GetHotel")
    .WithTags("Getters");

app.MapGet("/hotels/search/name/{query}", async (string query, IHotelRepository repos) =>
    {
        return await repos.GetHotelsAsync(query) is IEnumerable<Hotel> hotels
            ? Results.Ok(hotels)
            : Results.NotFound(Array.Empty<Hotel>());
    })
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotels")
    .WithTags("Getters")
    .ExcludeFromDescription();


app.MapGet("hotels/search/location/{coordinate}", async (Coordinate coordinate, IHotelRepository repos) =>
    await repos.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels
        ? Results.Ok(hotels)
        : Results.NotFound(Array.Empty<Hotel>()))
    .ExcludeFromDescription();


app.MapGet("/hotels", async ([FromBody] Hotel hotel, IHotelRepository repos) =>
    {
        await repos.InsertHotelAsync(hotel);
        await repos.SaveAsync();

        return Results.Created($"hotels/{hotel.Id}", hotel);
    })
    .Accepts<Hotel>("application/json")
    .Produces<Hotel>(StatusCodes.Status201Created)
    .WithName("CreateHotel")
    .WithTags("Creators");

app.MapPut("/hotels", async ([FromBody] Hotel hotel, IHotelRepository repos) =>
    {
        await repos.UpdateHotelAsync(hotel);
        await repos.SaveAsync();

        return Results.NoContent();
    })
    .Accepts<Hotel>("application/json")
    .WithName("UpdateHotel")
    .WithTags("Updaters"); ;


app.MapDelete("/hotels/{id}", async (int id, IHotelRepository repos) =>
    {
        await repos.DeleteHotelAsync(id);
        await repos.SaveAsync();

        return Results.NoContent();
    })
    .WithName("DeleteHotel")
    .WithTags("Deleters");

app.UseHttpsRedirection();

app.Run();



