using FluentAssertions;
using IdsLib.IdsSchema.IdsNodes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace idsTool.tests
{
	public class IdentificationLogger : ILogger
	{
		public IdentificationLogger()
		{
		}

		public IdentificationLogger(LogLevel logLevel)
		{
			_logLevel = logLevel;
		}		

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		private LogLevel _logLevel = LogLevel.Trace;

		internal IList<NodeIdentification> Identifications { get; set; } = [];

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (logLevel < _logLevel)
				return;
			if (state is IReadOnlyList<KeyValuePair<string, object>> vals)
			{
				var vls = vals.Where(x => x.Value is NodeIdentification).Select(sel => sel.Value as NodeIdentification);
				foreach (var item in vls)
				{
					Identifications.Add(item!);
				}
			}
		}
	}
}
