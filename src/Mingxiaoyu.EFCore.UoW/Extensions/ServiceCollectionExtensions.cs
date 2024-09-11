using Microsoft.EntityFrameworkCore;
using Mingxiaoyu.EFCore.UoW;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services, params Assembly[] assemblies)
        {
            // If no assemblies are specified, use the executing assembly
            if (assemblies == null || assemblies.Length == 0)
            {
                assemblies = new[] { Assembly.GetExecutingAssembly() };
            }

            assemblies = assemblies.Where(x => x != typeof(IRepository<,>).Assembly).ToArray();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

            // Scan the provided assemblies for classes that implement IGenericRepository<>
            AddRepository(services, typeof(Repository<,>), assemblies);

            return services;
        }
        public static IServiceCollection AddDbContext<TContext>(this IServiceCollection services)
            where TContext : DbContext, IDbContext
        {
            services.AddScoped<IDbContext, TContext>();

            return services;
        }

        public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        private static void AddRepository(IServiceCollection services, Type repositoryType, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                                    .Where(x => x.IsClass
                                            && !x.IsAbstract
                                            && x.BaseType != null
                                            && x.HasImplementedRawGeneric(repositoryType));

                foreach (var type in types)
                {
                    var interfaceType = type.GetInterface(repositoryType.Name);
                    //interfaceType = type;
                    //var serviceDescriptor = new ServiceDescriptor(type, type, ServiceLifetime.Scoped);
                    //if (!services.Contains(serviceDescriptor)) services.Add(serviceDescriptor);
                    //serviceDescriptor = new ServiceDescriptor(interfaceType, type, ServiceLifetime.Scoped);
                    //if (!services.Contains(serviceDescriptor)) services.Add(serviceDescriptor);
                    if (interfaceType == null)
                    {
                        //interfaceType = type;
                        var serviceDescriptor = new ServiceDescriptor(type, type, ServiceLifetime.Scoped);
                        if (!services.Contains(serviceDescriptor)) services.Add(serviceDescriptor);
                    }
                    else
                    {
                        var serviceDescriptor = new ServiceDescriptor(interfaceType, type, ServiceLifetime.Scoped);
                        if (!services.Contains(serviceDescriptor)) services.Add(serviceDescriptor);
                    }

                }
            }
        }

        private static bool HasImplementedRawGeneric(this Type type, Type generic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (generic == null) throw new ArgumentNullException(nameof(generic));
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
            if (isTheRawGenericType) return true;
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }
            return false;

            bool IsTheRawGenericType(Type test)
                => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }
    }
}
