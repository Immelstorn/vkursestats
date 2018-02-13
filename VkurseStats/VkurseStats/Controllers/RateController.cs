using System;
using System.Globalization;
using System.Web.Http;

using RestSharp;

using VkurseStats.Models;

namespace VkurseStats.Controllers
{
    public class RateController : ApiController
    {
        private const string Uri = "http://vkurse.dp.ua/course.json";

        public IHttpActionResult Get()
        {
            var client = new RestClient(Uri);
            var request = new RestRequest();
            var response = client.Execute<VkurseDto>(request);
            var content = response.Data;

            var result = new ReturnDto {
                Eur = new RateDto {
                    Buy = Convert.ToDouble(content.Euro.Buy, CultureInfo.InvariantCulture),
                    Sell = Convert.ToDouble(content.Euro.Sale, CultureInfo.InvariantCulture),
                },
                Usd = new RateDto
                {
                    Buy = Convert.ToDouble(content.Dollar.Buy, CultureInfo.InvariantCulture),
                    Sell = Convert.ToDouble(content.Dollar.Sale, CultureInfo.InvariantCulture),
                }
            };
            return Ok(result);
        }
    }
}
