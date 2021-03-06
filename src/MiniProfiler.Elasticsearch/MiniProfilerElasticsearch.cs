namespace StackExchange.Profiling.Elasticsearch
{
	using System;
	using System.Linq;
	using System.Text;
	using global::Elasticsearch.Net;
	using Profiling;

    /// <summary>
    /// <see cref="IElasticsearchResponse"/> handler class.
    /// </summary>
	internal static class MiniProfilerElasticsearch
	{
        /// <summary>
        /// Handles <see cref="IElasticsearchResponse"/> and pushes <see cref="CustomTiming"/> to current <see cref="MiniProfiler"/> session.
        /// </summary>
        /// <param name="response"><see cref="IElasticsearchResponse"/> to be handled.</param>
        /// <param name="profiler">Current <see cref="MiniProfiler"/> session instance.</param>
		internal static void HandleResponse(IElasticsearchResponse response, MiniProfiler profiler)
		{
			if (profiler == null || profiler.Head == null || response.Metrics == null)
				return;

			profiler.Head.AddCustomTiming("elasticsearch", new CustomTiming(profiler, BuildCommandString(response))
			{
				Id = Guid.NewGuid(),
				DurationMilliseconds = response.Metrics.Requests.Sum(c => c.EllapsedMilliseconds),
				ExecuteType = response.RequestMethod,
			});
		}

        /// <summary>
        /// Processes <see cref="IElasticsearchResponse"/> and builds command string for <see cref="CustomTiming"/> instance.
        /// </summary>
        /// <param name="response"><see cref="IElasticsearchResponse"/> to be processed.</param>
        /// <returns></returns>
		private static string BuildCommandString(IElasticsearchResponse response)
		{
			var commandTextBuilder = new StringBuilder();

			var url = response.RequestUrl;
			// Basic request information
			// HTTP GET - 200
			commandTextBuilder.AppendFormat("HTTP {0} - {1}\n", response.RequestMethod, response.HttpStatusCode);

			if (response.NumberOfRetries > 0)
			{
				commandTextBuilder.AppendFormat("Retries: {0}\n", response.NumberOfRetries);
			}

			// Request URL
			commandTextBuilder.AppendFormat("{0}\n\n", url);

			// Append query
			if (response.Request != null)
			{
				var request = Encoding.UTF8.GetString(response.Request);
				if (!String.IsNullOrWhiteSpace(request))
				{
					commandTextBuilder.AppendFormat("Request:\n{0}\n\n", request);
				}
			}

			// Append response
			if (response.ResponseRaw != null)
			{
				var responseRaw = Encoding.UTF8.GetString(response.ResponseRaw);
				if (!String.IsNullOrWhiteSpace(responseRaw))
				{
					commandTextBuilder.AppendFormat("Response:\n{0}\n\n", responseRaw);
				}
			}

			if (response.Success == false)
			{
				commandTextBuilder.AppendFormat("\nMessage:\n{0}", response.OriginalException.Message);
				if (!String.IsNullOrWhiteSpace(response.OriginalException.StackTrace))
				{
					commandTextBuilder.AppendFormat("Stack trace:\n{0}", response.OriginalException.StackTrace);
				}
			}
			
			// Set the command string to a formatted string
			return commandTextBuilder.ToString();
		}
	}
}