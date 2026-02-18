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

                        // Optional: Production diagnostics configuration (Spec 003 T023-T027)
                        // Uncomment and customize as needed for your environment
                        // services.AddSigilValidation(options =>
                        // {
                        //     // Enable diagnostic information collection and reporting
                        //     options.EnableDiagnostics = true;
                        //
                        //     // Include detailed failure messages in logs
                        //     // WARNING: Only enable in production if logs are secure/sanitized
                        //     options.LogFailureDetails = true;
                        //
                        //     // Spec 003 (T042): Register custom proof system and statement handler
                        //     // options.AddProofSystem("custom-system", new CustomProofSystemVerifier());
                        //     // options.AddStatementHandler(new CustomStatementHandler());
                        //
                        //     // Add custom proof systems or statement handlers here
                        //     // options.AddProofSystem("ps-id", verifier);
                        //     // options.AddStatementHandler(handler);
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

