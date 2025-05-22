using idsTool.tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common.Configuration;
using Xunit;
using static idsTool.tests.IfcFilesTests;

namespace idsTool.tests.Helpers
{
	
	public class XbimFixture : IDisposable
	{
		public ILogger<XbimFixture> Logger {  get; }

		public  XbimFixture() 
		{
			// ... initialize xbim context ...
			Logger = Substitute.For<ILogger<XbimFixture>>();

			XbimServices.Current.ConfigureServices(s =>
				s.AddXbimToolkit(opt => opt.AddMemoryModel())
				.AddLogging(c => configure(c, Logger))
			);
		}
		private void configure(ILoggingBuilder loggingBuilder, ILogger<XbimFixture> lg)
		{
			loggingBuilder.AddProvider(new MyLoggerProvider(lg));
		}

		public void Dispose()
		{
			// ... clean up test data from the database ...
		}
	}


	public class MyLoggerProvider : ILoggerProvider
	{
		ILogger ret;

		public MyLoggerProvider(ILogger val)
		{
			ret = val;
		}

		public void Dispose()
		{
			// Dispose any resources if needed
		}

		public ILogger CreateLogger(string categoryName)
		{
			return ret;
		}
	}



	[CollectionDefinition("xbim collection")]
	public class DatabaseCollection : ICollectionFixture<XbimFixture>
	{
		// This class has no code, and is never created. Its purpose is simply
		// to be the place to apply [CollectionDefinition] and all the
		// ICollectionFixture<> interfaces.
	}

}
