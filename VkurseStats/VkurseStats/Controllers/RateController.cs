using System;
using System.Globalization;
using System.Linq;
using System.Web.Http;

using RestSharp;

using VkurseStats.Models;
using VkurseStats.Models.DB;

namespace VkurseStats.Controllers
{
    public class RateController : ApiController
    {
        private const string Uri = "http://vkurse.dp.ua/course.json";

        public IHttpActionResult Get()
        {
            using (var db = new DataContext())
            {
                var last = db.VkurseRates.OrderByDescending(r => r.Timestamp).Take(2).ToList();
                if (!last.Any() || last.First().Timestamp.AddMinutes(30) <= DateTime.UtcNow)
                {
                    var client = new RestClient(Uri);
                    var request = new RestRequest();
                    var response = client.Execute<VkurseDto>(request);
                    var content = response.Data;

                    var newRate = new VkurseRate {
                        UsdBuy = Convert.ToDouble(content.Dollar.Buy, CultureInfo.InvariantCulture),
                        UsdSell = Convert.ToDouble(content.Dollar.Sale, CultureInfo.InvariantCulture),
                        EurBuy = Convert.ToDouble(content.Euro.Buy, CultureInfo.InvariantCulture),
                        EurSell = Convert.ToDouble(content.Euro.Sale, CultureInfo.InvariantCulture),
                    };
                    db.VkurseRates.Add(newRate);
                    db.SaveChanges();
                    last.Add(newRate);
                    last = last.OrderByDescending(r => r.Timestamp).ToList();
                }

                var result = new ReturnDto {
                    Eur = new RateDto {
                        Buy = last[0].EurBuy,
                        ChangePercentBuy = last.Count == 2 ? Math.Round((last[0].EurBuy / last[1].EurBuy - 1) * 100, 2) : 0,
                        Sell = last[0].EurSell,
                        ChangePercentSell = last.Count == 2 ? Math.Round((last[0].EurSell / last[1].EurSell - 1) * 100, 2) : 0,
                    },
                    Usd = new RateDto {
                        Buy = last[0].UsdBuy,
                        ChangePercentBuy = last.Count == 2 ? Math.Round((last[0].UsdBuy / last[1].UsdBuy - 1) * 100, 2) : 0,

                        Sell = last[0].UsdSell,
                        ChangePercentSell = last.Count == 2 ? Math.Round((last[0].UsdSell / last[1].UsdSell - 1) * 100, 2) : 0,

                    }
                };

                return Ok(result);
            }
        }
    }
}
