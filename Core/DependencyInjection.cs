namespace APIology.Runner.Core
{
	using Configuration;
	using Autofac;
	using Serilog;
	using System;
	using System.Reflection;
	using Microsoft.Extensions.Configuration;

	public class DependencyInjection
	{
		public static IContainer Container { get; private set; }

		internal static Lazy<IContainer> LazyContainer
			=> new Lazy<IContainer>(() => Container);

		public static void CreateContainerFor<T>()
			where T : ServiceConfiguration, new()
		{
			var builder = new ContainerBuilder();

			AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) =>
				Assembly.ReflectionOnlyLoad(args.Name);

			var assembly = Assembly.GetEntryAssembly();

			if (ReferenceEquals(assembly, null))
			{
				throw new EntryPointNotFoundException(
					$"No compatible implementation of {nameof(ServiceBase<T>)} can be found.");
			}

			builder.RegisterAssemblyModules<ServiceBase<T>>(assembly);

			builder.Register(ctx => {
				var service = ctx.Resolve<IServiceBase>();

				var cb = service.BuildConfiguration(new ConfigurationBuilder());
				var conf = new T {
					Service = service,
					Root = cb.Build()
				};

				conf.Root.Bind(conf);

				var logConf = new LoggerConfiguration();
				service.BuildLogger(logConf);

				lock (Log.Logger) {
					Log.CloseAndFlush();
					service.Logger = Log.Logger = logConf.CreateLogger();
				}

				var dllPath = conf.DllPath.Replace(AppDomain.CurrentDomain.BaseDirectory, "");
				service.Logger.Information("registered service from {DllPath}", dllPath);

				return conf;
			})
			.SingleInstance()
			.PropertiesAutowired(PropertyWiringOptions.PreserveSetValues)
			.AutoActivate()
			.AsImplementedInterfaces()
			.AsSelf();

			Container = builder.Build();
		}
	}
}
