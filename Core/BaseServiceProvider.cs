// ReSharper disable StaticMemberInGenericType
namespace APIology.ServiceProvider.Core
{
	using Configuration;
	using Autofac;
	using Serilog;
	using System;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using Topshelf;
	using Microsoft.Extensions.Configuration;
	using System.Linq;

	using DI = DependencyInjection;
	using static System.Reflection.Assembly;

	[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
	public abstract class BaseServiceProvider<TConfiguration> : Module, ISystemServiceProvider
		where TConfiguration : BaseServiceConfiguration, new()
	{
		private static readonly string[] StartupMessagesToIgnore = {
			"Configuration Result:\n{0}",
			"{0} v{1}, .NET Framework v{2}"
		};

		public static string EnvironmentName { get; protected set; }

		public static void Run()
		{
			try
			{
				Log.Logger = new LoggerConfiguration()
					.WriteTo.LiterateConsole()
					.Filter.ByExcluding(lea => StartupMessagesToIgnore.Contains(lea.MessageTemplate.Text))
					.CreateLogger();

				HostFactory.Run(hostConfigurator => {
					hostConfigurator.UseSerilog();

					hostConfigurator.Service<ServiceControl>(serviceConfigurator => {
						serviceConfigurator.ConstructUsing(hostSettings => {
							DI.CreateContainerFor<TConfiguration>();
							return DI.Container.Resolve<ISystemServiceProvider>();
						});
						serviceConfigurator.WhenStarted((s, h) => s.Start(h));
						serviceConfigurator.WhenStopped((s, h) => s.Stop(h));
						serviceConfigurator.AfterStoppingService(hostStoppedContext => {
							DI.Container.Dispose();
						});
					});
					
					hostConfigurator.UseAssemblyInfoForServiceInfo(GetEntryAssembly());
				});
			}
			catch (Exception ex)
			{
				if (EnvironmentName == "Development") {
					Debugger.Break();
				}
				Log.Logger?.Fatal(ex, "A fatal error has occurred");
				if (Environment.UserInteractive) {
					Console.ReadKey();
				}
			}
		}

		private TConfiguration _config;
		public TConfiguration Config {
			get {
				if (!ReferenceEquals(_config, null))
					return _config;

				_config = DI.Container.Resolve<TConfiguration>();
				return _config;
			}
		}

		public Lazy<IContainer> LazyContainer => DI.LazyContainer;

		public ILogger Logger { get; set; }

		public abstract bool Start(HostControl hostControl);

		public abstract bool Stop(HostControl hostControl);
		
		public abstract IConfigurationBuilder BuildConfiguration(IConfigurationBuilder configuration);
		public abstract void BuildLogger(LoggerConfiguration loggingSettings);
		public abstract void BuildDependencyContainer(ContainerBuilder builder);

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterInstance(this)
				.PropertiesAutowired(PropertyWiringOptions.PreserveSetValues)
				.SingleInstance()
				.As<ISystemServiceProvider>()
				.AsSelf();

			BuildDependencyContainer(builder);
		}
	}
}
