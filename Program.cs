using System.Net;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".env")))
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["GoogleAuthSettings:ClientId"];
    options.ClientSecret = builder.Configuration["GoogleAuthSettings:ClientSecret"];
});

builder.Services.AddControllersWithViews();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<S3Service>();

builder.Services.AddSingleton<EndguinField.Services.MongoDB.IParticipationMongoService, EndguinField.Services.MongoDB.ParticipationMongoService>();
builder.Services.AddSingleton<EndguinField.Services.MongoDB.ICommentMongoService, EndguinField.Services.MongoDB.CommentMongoService>();
builder.Services.AddSingleton<EndguinField.Services.MongoDB.IEventMongoService, EndguinField.Services.MongoDB.EventMongoService>();

builder.Services.AddSingleton<EndguinField.Services.IUserService, EndguinField.Services.UserService>();
builder.Services.AddSingleton<EndguinField.Services.ICommentService, EndguinField.Services.CommentService>();
builder.Services.AddSingleton<EndguinField.Services.IParticipationService, EndguinField.Services.ParticipationService>();
builder.Services.AddSingleton<EndguinField.Services.IEventService, EndguinField.Services.EventService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                             ForwardedHeaders.XForwardedProto |
                             ForwardedHeaders.XForwardedHost;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
