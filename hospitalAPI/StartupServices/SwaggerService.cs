using Microsoft.OpenApi.Models;

namespace hospitalAPI.StartupFile;

public static class SwaggerService
{
    public static IServiceCollection AddSwaggerService(this IServiceCollection Services)
    {
        Services.AddSwaggerGen(opts =>
       {

           opts.AddServer(new OpenApiServer()
           {
               Url = "",
               Description = "https://localhost:7143/ this is for local server for testing and working."
           });

           opts.AddServer(new OpenApiServer()
           {
               Url = "http://api.validatione.com/",
               Description = "this is for prodcution  server for testing and working."
           });

           // Admin sees everything
           opts.SwaggerDoc("admin", new OpenApiInfo
           {
               Title = "Admin API",
               Version = "admin",
               Description = "This API is used by admins to manage the hospital system. It includes endpoints for managing doctors, nurses, patients, and reports.",
               TermsOfService = new Uri("https://yourdomain.com/terms"),
               Contact = new OpenApiContact
               {
                   Name = "Support Team",
                   Email = "support@yourdomain.com",
                   Url = new Uri("https://yourdomain.com/support")
               },
               License = new OpenApiLicense
               {
                   Name = "MIT License",
                   Url = new Uri("https://opensource.org/licenses/MIT")
               }
           });

           // Clients see only their endpoints
           opts.SwaggerDoc("clients", new OpenApiInfo { Title = "Client API", Version = "clients" });

           // Doctors
           opts.SwaggerDoc("doctors", new OpenApiInfo { Title = "Doctor API", Version = "doctors" });

           // Nurses
           opts.SwaggerDoc("nurses", new OpenApiInfo { Title = "Nurse API", Version = "nurses" });


           // that is auth model that you see on the swagger ui where you send requests with auth
           //You’re telling Swagger: “I’m using a Bearer token in the HTTP header, under the Authorization field.”
           //This makes Swagger show a lock icon  in the UI, where you can enter the JWT token.
           //Parameters you set:
           //In = Header → token is passed in the HTTP header.
           //Name = "Authorization" → name of the header.
           //Scheme = "Bearer" → standard keyword for JWT bearer tokens.
           //Type = Http → indicates this is HTTP authentication, not API keys or OAuth2.
           //Description → helps users know the expected format(you wrote { token}, but usually it’s "Bearer {token}").

           opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
           {
               In = ParameterLocation.Header,
               Description = "please enter token in the format {token}",
               Name = "X-MyCustomToken",
               Type = SecuritySchemeType.ApiKey,
               Scheme = "Bearer",
           });

           //Without this, Swagger would let you enter a token, but wouldn’t attach it to requests automatically.
           opts.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
         }
   });

           //will work on these are version as these are for based on policy and role base endpoint filtering 
           //opts.OperationFilter<PolicyOperationFilter>();
           //opts.DocumentFilter<RoleBasedDocumentFilter>();
       });

        return Services;
    }
}
