using Ensek.Domain.Accounts;
using Ensek.Domain.Accounts.DataConverters;
using Ensek.Repository.Accounts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s => {
        //Donmain
        s.AddScoped<IMeterReadingProcessor, MeterReadingProcessor>();

        // This is added just to reflect that if we extend our system to have multiple types then we can get benefit
        // of IoC and implement changes in domain wihtout refactoring domain layer
        s.AddScoped(typeof(IDataConverter<>), typeof(CsvDataConverter<>));
        
        //Csv converter
        s.AddScoped(typeof(ICsvDataConverter<>), typeof(CsvDataConverter<>));

        //Repositories
        s.AddSingleton<IDatabaseProvider, SqlLiteDatabaseProvider>();
        s.AddSingleton<ICustomerAccountsRepository, CustomerAccountsRepository>();
        s.AddSingleton<ICustomerMeteringDataRepository, CustomerMeteringDataRepository>();
    })
    .Build();

host.Run();

