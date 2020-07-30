using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Shortcodes.Controllers;
using OrchardCore.Shortcodes.Drivers;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Shortcodes.ViewModels;
using Shortcodes;
using Sc = Shortcodes;

namespace OrchardCore.Shortcodes
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<ShortcodeViewModel>();

            TemplateContext.GlobalMemberAccessStrategy.Register<Context, object>((obj, name) => obj[name]);

            // Prevent Context from being converted to an ArrayValue as it implements IEnumerable
            FluidValue.SetTypeMapping<Context>(o => new ObjectValue(o));

            TemplateContext.GlobalMemberAccessStrategy.Register<Sc.Arguments, object>((obj, name) => obj.NamedOrDefault(name));

            // Prevent Arguments from being converted to an ArrayValue as it implements IEnumerable
            FluidValue.SetTypeMapping<Sc.Arguments>(o => new ObjectValue(o));
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShortcodeService, ShortcodeService>();
            services.AddScoped<IShortcodeTableManager, ShortcodeTableManager>();
            services.AddScoped<IShortcodeTableProvider, ShortcodeOptionsTableProvider>();
            services.AddScoped<IShortcodeContextProvider, DefaultShortcodeContextProvider>();

            services.AddOptions<ShortcodeOptions>();
            services.AddScoped<IShortcodeProvider, OptionsShortcodeProvider>();
            services.AddScoped<IDisplayManager<ShortcodeDescriptor>, DisplayManager<ShortcodeDescriptor>>();
            services.AddScoped<IDisplayDriver<ShortcodeDescriptor>, ShortcodeDescriptorDisplayDriver>();

            //TODo testing code remove.
            services.AddShortcode("bold", (args, content, ctx) => {
                var text = args.Named("text");
                if (!String.IsNullOrEmpty(text))
                {
                    content = text;
                }

                return new ValueTask<string>($"<em>{content}</em>");
            });

            services.AddShortcode("bold", (args, content, ctx) => {
                var text = args.NamedOrDefault("text");
                if (!String.IsNullOrEmpty(text))
                {
                    content = text;
                }

                return new ValueTask<string>($"<b>{content}</b>");

            }, d => {
                d.ReturnShortcode = "[bold ]";

                d.Hint = (sp) => {
                    var S = sp.GetRequiredService<IStringLocalizer<Startup>>();
                    return S["Add bold formatting with a shortcode."];
                };
                d.Usage = "[bold 'your bold content here]'";
                d.Categories = (sp) => {
                    var S = sp.GetRequiredService<IStringLocalizer<Startup>>();
                    return new string[] { S["HTML Content"], S["Content Item"] };
                };

            });
        }

    }

    [Feature("OrchardCore.Shortcodes.Templates")]
    public class ShortcodeTemplatesStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public ShortcodeTemplatesStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        // Register this first so the templates provide overrides for any code driven shortcodes.
        public override int Order => -10;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ShortcodeTemplatesManager>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<IShortcodeProvider, TemplateShortcodeProvider>();
            services.AddScoped<IShortcodeTableProvider, ShortcodeTemplatesTableProvider>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var templateControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Shortcodes.Index",
                areaName: "OrchardCore.Shortcodes",
                pattern: _adminOptions.AdminUrlPrefix + "/Shortcodes",
                defaults: new { controller = templateControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "Shortcodes.Create",
                areaName: "OrchardCore.Shortcodes",
                pattern: _adminOptions.AdminUrlPrefix + "/Shortcodes/Create",
                defaults: new { controller = templateControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "Shortcodes.Edit",
                areaName: "OrchardCore.Shortcodes",
                pattern: _adminOptions.AdminUrlPrefix + "/Shortcodes/Edit/{name}",
                defaults: new { controller = templateControllerName, action = nameof(AdminController.Edit) }
            );
        }
    }
}
