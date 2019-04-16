using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;

namespace SampleApp.Services
{
    public interface IRestService
    {
        Task<FactModel> GetFactAsync();
    }

    public class RestService : IRestService
    {
        private const string ApiUrl = "https://cat-fact.herokuapp.com";

        public async Task<FactModel> GetFactAsync()
        {
            var url = ApiUrl
                .AppendPathSegments("facts", "random");
            var fact = await url.GetJsonAsync<FactModel>();
            return fact;
        }
    }

    [JsonObject]
    public class FactModel
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
