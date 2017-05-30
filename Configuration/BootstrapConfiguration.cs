namespace APIology.Runner.Configuration
{
	using Newtonsoft.Json;

	public class BootstrapConfiguration
	{
		[JsonProperty("ServiceDllPath")]
		public string DllPath { get; set; }
	}
}
