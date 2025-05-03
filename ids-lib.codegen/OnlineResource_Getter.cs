
namespace IdsLib.codegen
{
	internal class OnlineResource_Getter
	{
		internal static string Execute(string url)
		{
			try
			{
				var _httpClient = new HttpClient
				{
					Timeout = new TimeSpan(0, 0, 30)
				};
				_httpClient.DefaultRequestHeaders.Clear();
				using var response = _httpClient.GetAsync(url).Result;
				response.EnsureSuccessStatusCode();
				var stream = response.Content.ReadAsStream();
				response.Content.ReadAsStream();
				using var reader = new StreamReader(stream);
				var content = reader.ReadToEnd();
				return content;
			}
			catch (Exception)
			{
				return "";
			}
			
		}
	}
}