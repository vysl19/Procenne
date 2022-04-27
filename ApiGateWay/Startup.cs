using Business;
using Gateway.Business;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ApiGateWay
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
            services.AddControllers();
            services.AddEasyCaching(options =>
            {
                options.UseRedis(redisConfig =>
                {
                    redisConfig.DBConfig.Endpoints.Add(new EasyCaching.Core.Configurations.ServerEndPoint("Localhost", 6379));
                    //Setup password if applicable
                    //if (!string.IsNullOrEmpty(serverPassword))
                    //{
                    //    redisConfig.DBConfig.Password = serverPassword;
                    //}
                    redisConfig.DBConfig.AllowAdmin = true;
                }, "redis1");
            });
            services.AddScoped<IGateWayService, GateWayService>();
            var trip = new TripleDESCryptoServiceProvider();    
            DeriveBytes rgb = new Rfc2898DeriveBytes("fsgerh", Encoding.Unicode.GetBytes("Dataehaeh"));
            byte[] key = rgb.GetBytes(trip.KeySize >> 3);
            byte[] iv = rgb.GetBytes(trip.BlockSize >> 3);
            services.AddSingleton<ICryptoService>(x => new TripleDESCryptoService(key, iv));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
