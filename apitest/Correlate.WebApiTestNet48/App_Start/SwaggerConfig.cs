using System.Web.Http;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace Correlate.WebApiTestNet48;

public static class SwaggerConfig
{
    public static void Register(HttpConfiguration configuration)
    {
        configuration
            .EnableSwagger(ConfigureSwagger)
            .EnableSwaggerUi(ConfigureSwaggerUi);
    }

    private static void ConfigureSwagger(SwaggerDocsConfig c)
    {
        c.SingleApiVersion("v1", "Correlate.WebApiTestNet48")
            .Description("Version: 1.0");

        c.PrettyPrint();

        c.MapType<decimal>(() => new Schema { type = "number", format = "decimal" });
        c.MapType<decimal?>(() => new Schema { type = "number", format = "decimal" });

        c.UseFullTypeNameInSchemaIds();

        c.DescribeAllEnumsAsStrings();

    }

    private static void ConfigureSwaggerUi(SwaggerUiConfig c)
    {
        c.DocumentTitle("Swagger - Correlate.WebApiTestNet48");
    }
}
