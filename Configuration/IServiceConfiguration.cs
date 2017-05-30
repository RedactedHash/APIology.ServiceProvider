namespace APIology.ServiceProvider.Configuration
{
	using Microsoft.Extensions.Configuration;
	using Newtonsoft.Json;
	using System;

	public interface IServiceConfiguration
	{
		[JsonProperty("ServiceName")]
		string Name { get; set; }

		[JsonProperty("ServiceDescription")]
		string Description { get; set; }

		[JsonProperty("ServiceDllPath")]
		string DllPath { get; }

		[JsonIgnore]
		Type Type { get; }

		[JsonIgnore]
		IConfigurationRoot Root { get; }
	}
}
