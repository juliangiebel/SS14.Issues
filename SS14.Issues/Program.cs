using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;
using SS14.Issues;
using SS14.Issues.Data;
using SS14.Issues.Jobs;
using SS14.Issues.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddYamlFile("appsettings.yaml", false, true);
builder.Configuration.AddYamlFile($"appsettings.{builder.Environment.EnvironmentName}.yaml", true, true);
builder.Configuration.AddYamlFile("appsettings.Secret.yaml", true, true);

//Database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

//Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<IssueSyncService>();
builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddSingleton<IssueRateLimiterService>();
builder.Services.AddSingleton<GithubApiService>();

//Auth
    var configuration = builder.Configuration;
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    })
    .AddOpenIdConnect("oidc", options => 
    {
        options.SignInScheme = "Cookies";
        
        options.Authority = configuration["Auth:Authority"];
        options.ClientId = configuration["Auth:ClientId"];
        options.ClientSecret = configuration["Auth:ClientSecret"];
        options.SaveTokens = true;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.GetClaimsFromUserInfoEndpoint = true;
        options.TokenValidationParameters = new 
            TokenValidationParameters
            {
                NameClaimType = "name"
            };

        options.Events = new OpenIdConnectEvents
        {
            OnAccessDenied = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/");
                return Task.CompletedTask;
            },
            
           OnTokenValidated = async ctx =>
            {
                var handler = ctx.HttpContext.RequestServices.GetRequiredService<LoginHandler>();
                await handler.HandleTokenValidated(ctx);
            }
        };
    });

builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

//Scheduler
builder.Services.AddQuartz(q => { q.UseMicrosoftDependencyInjectionJobFactory(); });
builder.Services.AddQuartzServer(q => { q.WaitForJobsToComplete = true; });

//Logging
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
builder.Logging.AddSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


/*
if (app.Configuration.GetSection("ForwardProxies") != null)
{
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    };

    
    foreach (var ip in app.Configuration.GetSection("ForwardProxies").Get<string[]>())
    {
        forwardedHeadersOptions.KnownProxies.Add(IPAddress.Parse(ip));
    }

    app.UseForwardedHeaders(forwardedHeadersOptions);
    
}*/

app.UseSerilogRequestLogging();
//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Use((context, next) =>
    {
         context.Request.EnableBuffering();
         return next();
     });

var scheduler = app.Services.GetRequiredService<ISchedulerFactory>();
CronAttributeScheduler.ScheduleMarkedJobs(scheduler);

app.Run();