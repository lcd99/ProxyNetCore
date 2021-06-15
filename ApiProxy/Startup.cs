using AspNetCore.Proxy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiProxy
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
            services.AddRazorPages();
            services.AddProxies();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.RunProxy(proxy => proxy.UseHttp((context, args) =>
            {
                var listproxy = Configuration.GetSection("webproxy").Get<List<Proxy>>();
                var proxy = listproxy.Where(p => context.Request.Path.ToString().ToLower().Contains(p.path.ToLower())).FirstOrDefault();
                if (proxy != null)
                {

                    if (proxy.isRemovePath.ToString() != "0")
                        context.Request.Path = context.Request.Path.ToString().ToLower().Replace(proxy.path.ToLower(), "");
                    return proxy.link;
                }
                return "/";
            }));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }

    public class Proxy
    {
        public string path { get; set; }
        public string link { get; set; }
        public string isRemovePath { get; set; }
    }
}
