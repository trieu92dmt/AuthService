using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Authentication.Configurations
{
    public static class SwaggerConfig
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // Cấu hình Bearer token cho Swagger UI
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Authen API",
                    Version = "v1",
                });

                options.OperationFilter<AddAuthorizationHeader>();

            });
        }

        public class AddAuthorizationHeader : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                operation.Parameters?.Add(new OpenApiParameter
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your token",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
            }
        }
    }
}
