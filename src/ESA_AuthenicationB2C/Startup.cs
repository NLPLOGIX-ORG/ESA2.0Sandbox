using ESA_AuthenicationB2C.Options;
using ESA_AuthenicationB2C.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ESA_AuthenicationB2C
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
            services.AddDistributedMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;
                options.HandleSameSiteCookieCompatibility();
            });

            services.AddOptions();

            services.Add(new ServiceDescriptor(typeof(IGraphApiClient), sp => new GraphApiClient(Configuration), ServiceLifetime.Transient));

            services.AddMicrosoftIdentityWebAppAuthentication(Configuration, "AzureAdB2C")
                    .EnableTokenAcquisitionToCallDownstreamApi(new string[] { Configuration["TodoList:TodoListScope"] })
                    .AddInMemoryTokenCaches();

            //Invitation link SignUp
            string invite_policy = Configuration.GetSection("AzureAdB2C")["InvitationPolicyId"];
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie("oidc-invite")
                .AddOpenIdConnect(invite_policy, GetOpenIdSignUpOptions(invite_policy)); 

            // Add APIs
            services.AddTodoListService(Configuration);

            services.AddSqlClientService(Configuration);

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();

            services.AddRazorPages();

            services.AddOptions();
            services.Configure<OpenIdConnectOptions>(Configuration.GetSection("AzureAdB2C"));

            // services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            // {
            //     var previousOptions = options.Events.OnRedirectToIdentityProvider;
            //     options.Events.OnRedirectToIdentityProvider = async context =>
            //     {
            //         await previousOptions(context);
            //         context.ProtocolMessage.ResponseType = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectResponseType.IdToken;
            //     };
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
        
        private Action<OpenIdConnectOptions> GetOpenIdSignUpOptions(string policy)
            => options =>
            {
                string clientId = Configuration.GetSection("AzureAdB2C")["ClientId"];
                string Instance = Configuration.GetSection("AzureAdB2C")["Instance"];
                string Domain = Configuration.GetSection("AzureAdB2C")["Domain"];
                string Invite = Configuration.GetSection("AzureAdB2C")["InvitationPolicyId"];

                options.MetadataAddress = $"{Instance}/{Domain}/{policy}/v2.0/.well-known/openid-configuration";
                options.ClientId = clientId;
                options.ResponseType = OpenIdConnectResponseType.IdToken;
                options.SignedOutCallbackPath = "/signout/" + policy;
                
                if (policy == Invite)
                    options.CallbackPath = "/signin-oidc-invite";

                options.SignedOutRedirectUri = "/";
                options.SignInScheme = "oidc-invite";
                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        if (context.Properties.Items.ContainsKey("id_token_hint"))
                            context.ProtocolMessage.SetParameter("id_token_hint", context.Properties.Items["id_token_hint"]);                        

                        return Task.FromResult(0);
                    }
                };
            };
    }
}
