using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using netcore_happypath.data;
using netcore_happypath.data.Session;
using netcore_happypath.services.DatabaseActivities;
using netcore_happypath.services.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace netcore_happypath
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddMvc()
                // Turn off camel-casing
                .AddJsonOptions(opt => {
                    opt.SerializerSettings.NullValueHandling = NullValueHandling.Include;
                    opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    opt.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
                    opt.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Error;
                    opt.SerializerSettings.Formatting = Formatting.Indented;
                    opt.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

            services.AddLogging();


            services.AddDbContext<NetCoreHappyPathDbContext>(options => options.UseInMemoryDatabase("NetCoreHappyPathDb"));
            services.AddTransient<DbContext, NetCoreHappyPathDbContext>();
            services.AddTransient<INetCoreHappyPathSession, NetCoreHappyPathSession>();

            // Glen's Way
            //AutoRegisterTypesFromAssembly(typeof(IUserService), services);
            //AutoRegisterTypesFromAssembly(typeof(NetCoreHappyPathDbContext), services);

            // I dono what we need this for yet. Haven't gotten to user stuff.
            //services.AddSingleton<ClaimsPrincipal>(s => s.GetService<IHttpContextAccessor>().HttpContext.User);
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ICurrentUserService, CurrentUserService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        // Glen's Way
        ///// <summary>
        ///// Automatically registers types from the assembly the provided type is a member of.
        ///// </summary>
        ///// <param name="typeFromAssemblyToRegister">The type from assembly to register.</param>
        ///// <param name="services">The services.</param>
        //protected internal void AutoRegisterTypesFromAssembly(Type typeFromAssemblyToRegister, IServiceCollection services)
        //{
        //    Assembly assembly = Assembly.GetAssembly(typeFromAssemblyToRegister);
        //    string nameSpace = typeFromAssemblyToRegister.Namespace.Split(".", StringSplitOptions.RemoveEmptyEntries)[0];

        //    List<TypeInfo> assemblyClasses = assembly.DefinedTypes.Where(t => t.IsClass && !t.IsAbstract).ToList();
        //    foreach (TypeInfo assemblyClass in assemblyClasses)
        //    {
        //        //List<Type> classInterfaces = assemblyClass.ImplementedInterfaces.Where(x => x.Namespace.StartsWith(nameSpace)).ToList();
        //        //if (classInterfaces.Count == 1)
        //        //{
        //        //    services.AddScoped(classInterfaces[0], assemblyClass.AsType());
        //        //}
        //    }
        //}
    }
}
