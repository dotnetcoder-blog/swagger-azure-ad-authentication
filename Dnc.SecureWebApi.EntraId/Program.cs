using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(swaggerGenOptions =>
{
    swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo { Title ="DotNetCoder Swagger Azure Ad Authentication Demo", Version = "v1" });
    swaggerGenOptions.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Name = "oauth2",
        Description = "Authorozation Code flow with PKCE",
        //Tells Swagger that we’re using the OAuth2 mechanism for authentication.
        Type = SecuritySchemeType.OAuth2, // The type of scurity mechanism used in API
        Flows = new OpenApiOAuthFlows   // The flow we are using to authenticate 
        {
            //We are using the Authorization Code Flow here
            AuthorizationCode = new OpenApiOAuthFlow
            {
                //AuthorizationUrl : this is the URL where Swagger sends users to log in
                AuthorizationUrl = new Uri($"{builder.Configuration["SwaggerAzureAd:Instance"]}/{builder.Configuration["SwaggerAzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                //TokenUrl : After logging in, this is where Swagger exchanges the login code for an access token
                TokenUrl = new Uri($"{builder.Configuration["SwaggerAzureAd:Instance"]}/{builder.Configuration["SwaggerAzureAd:TenantId"]}/oauth2/v2.0/token"),
                //These are the specific permissions the API offers.// access_as_user in our case
                Scopes = new Dictionary<string, string>
                {
                    {$"{builder.Configuration["SwaggerAzureAd:Audience"]}/{builder.Configuration["SwaggerAzureAd:Scopes"]}", "Access API as user" }
                }
            }
        }
    });
    swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { $"{builder.Configuration["SwaggerAzureAd:Audience"]}/{builder.Configuration["SwaggerAzureAd:Scopes"]}" }
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("access_as_user", policy =>
        policy.RequireScope("access_as_user"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(swaggerUIOptions =>
    {
        swaggerUIOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "DotNetCoder Swagger Authentication Demo V1");
        swaggerUIOptions.OAuthClientId(builder.Configuration["SwaggerAzureAd:ClientId"]);
        swaggerUIOptions.OAuthUsePkce();
        swaggerUIOptions.OAuthScopeSeparator(" ");
        swaggerUIOptions.OAuthUseBasicAuthenticationWithAccessCodeGrant(); 
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
