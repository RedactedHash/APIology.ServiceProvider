namespace APIology.Runner.Core
{
	using Autofac;
	using Autofac.Core;
	using Microsoft.Extensions.Configuration;
	using Serilog;
	using Topshelf;

	public interface IServiceBase : ServiceControl, IModule
	{
		ILogger Logger { get; set; }

		IConfigurationBuilder BuildConfiguration(IConfigurationBuilder configuration);

		void BuildLogger(LoggerConfiguration loggingSettings);

		void BuildDependencyContainer(ContainerBuilder builder);
	}
}
