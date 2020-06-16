using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using BookStore_API.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using BookStore_API.Contracts;
using BookStore_API.Services;
using AutoMapper;
using BookStore_API.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BookStore_API
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
            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            // ADD CORS, build policy
            services.AddCors(o => {
                o.AddPolicy("CorsPolicy", 
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            }); // now go down to Configure (line 78) 

            //Adding AutoMapper
            services.AddAutoMapper(typeof(Maps));

            // Adding jwt bearer token, configure it.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(o => {
                        o.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = Configuration["Jwt:Issuer"],
                            ValidAudience = Configuration["Jwt:Issuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                        };
                    });

            // add swagger services added from nuget package manager
            services.AddSwaggerGen(c => { // lambda expression c, "token".
                c.SwaggerDoc("v1", 
                    new OpenApiInfo { 
                        Title = "Book Store API", 
                        Version = "v1" ,
                        Description = "This is an educational API for a Book Store"
                    });// 2 params, giving it "version1" name, new OpenApiInfo 

                // below line getting the path where this project sits, and find xml
                var xfile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"; // $ > interpolation?
                // getting path to xml file
                var xpath = Path.Combine(AppContext.BaseDirectory, xfile);
                // include xml/comments in documentation when swagger runs.
                // refer to ///<Comments> in HomeController.cs
                c.IncludeXmlComments(xpath);
            });

            // initialize NLog
            services.AddSingleton<ILoggerService, LoggerService>();
            // Iauthorrepo too
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            // and the ibookrepo as well
            services.AddScoped<IBookRepository, BookRepository>();
            //services.AddRazorPages();
            services.AddControllers().AddNewtonsoftJson(op => {
                op.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                }); 
            // leave it for last because everything else should be ready before adding controllers `ideally`
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env, 
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            // app, when you are about to run, use Swagger.
            app.UseSwagger();

            // and use swagger UI.
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Book Store API");
                // display when route is / instead (start up page)
                c.RoutePrefix = "";
            });

            app.UseHttpsRedirection();
            //app.UseStaticFiles(); // such as css, js inside www

            //USE CORS
            app.UseCors("CorsPolicy"); //Ready for global interaction.

            // Seed Data
            SeedData.Seed(userManager, roleManager).Wait(); // using .Wait because if I use await, need to make configure async, will mess up things.

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
