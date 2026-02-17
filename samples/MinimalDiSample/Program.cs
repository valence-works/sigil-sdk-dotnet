// Spec 003 (SC-001): Minimal 3-5 line setup demonstrating DI integration
// This sample shows developers can integrate Sigil validation with minimal friction

using Sigil.Sdk.DependencyInjection;

// Make Program public so it's accessible to WebApplicationFactory for integration testing
public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder =>
            {
                builder
                    .ConfigureServices(services =>
                    {
                        // Spec 003 (FR-001): Single line registration with defaults
                        services.AddSigilValidation();

                        // Optional: Uncomment to add custom configuration
                        // services.AddSigilValidation(options =>
                        // {
                        //     options.EnableDiagnostics = true;
                        //     // Add custom proof systems or handlers here
                        // });

                        services.AddControllers();
                    })
                    .Configure(app =>
                    {
                        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                        
                        if (env.IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                            app.UseSwagger();
                            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal DI Sample v1"));
                        }

                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
            });
}

