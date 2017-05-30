namespace APIology.ServiceProvider.Configuration
{
	using Core;
	using Serilog;
	using System;
	using Microsoft.Extensions.Configuration;

	public class BaseServiceConfiguration : IServiceConfiguration
	{
		private ISystemServiceProvider _service;

		public ISystemServiceProvider Service {
			get {
				return _service;
			}
			set {
				_service = value;
				_dllPath = new Lazy<string>(() => Type.Assembly.Location);
				_lazyName = new Lazy<string>(() => Type.Assembly.FullName.Split(',')[0]);
			}
		}

		public Type Type => _service.GetType();

		private Lazy<string> _lazyName;

		public string Name {
			get { return _lazyName?.Value; }
			set { _lazyName = new Lazy<string>(() => value); }
		}

		private Lazy<string> _dllPath;

		public string DllPath {
			get { return _dllPath?.Value; }
			set { _dllPath = new Lazy<string>(() => value); }
		}

		public string Description { get; set; }

		public LoggerConfiguration LoggerConfiguration { get; set; }

		public IConfigurationRoot Root { get; set; }
	}
}
