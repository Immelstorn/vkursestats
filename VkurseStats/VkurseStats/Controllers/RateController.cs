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
                var yesterdayStart = DateTime.UtcNow.Date.AddDays(-1);
                var yesterdayFinish = DateTime.UtcNow.Date.AddSeconds(-1);
                var yesterdayValues = db.VkurseRates
                    .Where(r => r.Timestamp > yesterdayStart && r.Timestamp < yesterdayFinish)
                    .OrderByDescending(r=>r.Timestamp)
                    .ToList();

                var yesterdayValue = yesterdayValues.Last();
                for (var i = 0; i < yesterdayValues.Count - 1; i++)
                {
                    if (yesterdayValues[i].UsdBuy == yesterdayValues[i + 1].UsdBuy &&
                        yesterdayValues[i].UsdSell == yesterdayValues[i + 1].UsdSell &&
                        yesterdayValues[i].EurBuy == yesterdayValues[i + 1].EurBuy &&
                        yesterdayValues[i].EurSell == yesterdayValues[i + 1].EurSell)
                    {
                        continue;
                    }
                    yesterdayValue = yesterdayValues[i];
                    break;
                }

                var lastToday = db.VkurseRates.OrderByDescending(r => r.Timestamp).First();
                if (lastToday.Timestamp.AddMinutes(30) <= DateTime.UtcNow)
                {
                    var client = new RestClient(Uri);
                    var request = new RestRequest();
                    var response = client.Execute<VkurseDto>(request);
                    var content = response.Data;

                    var newRate = new VkurseRate {
                        UsdBuy = Convert.ToDouble(content.Dollar.Buy.Replace(',','.'), CultureInfo.InvariantCulture),
                        UsdSell = Convert.ToDouble(content.Dollar.Sale.Replace(',', '.'), CultureInfo.InvariantCulture),
                        EurBuy = Convert.ToDouble(content.Euro.Buy.Replace(',', '.'), CultureInfo.InvariantCulture),
                        EurSell = Convert.ToDouble(content.Euro.Sale.Replace(',', '.'), CultureInfo.InvariantCulture),
                    };
                    db.VkurseRates.Add(newRate);
                    db.SaveChanges();
                    lastToday = newRate;
                }

                var result = new ReturnDto {
                    Eur = new RateDto {
                        Buy = lastToday.EurBuy,
                        ChangePercentBuy = Math.Round((lastToday.EurBuy / yesterdayValue.EurBuy - 1) * 100, 2),
                        Sell = lastToday.EurSell,
                        ChangePercentSell = Math.Round((lastToday.EurSell / yesterdayValue.EurSell - 1) * 100, 2)
                    },
                    Usd = new RateDto {
                        Buy = lastToday.UsdBuy,
                        ChangePercentBuy = Math.Round((lastToday.UsdBuy / yesterdayValue.UsdBuy - 1) * 100, 2),

                        Sell = lastToday.UsdSell,
                        ChangePercentSell = Math.Round((lastToday.UsdSell / yesterdayValue.UsdSell - 1) * 100, 2),
                    }
                };

                return Ok(result);
            }
        }
    }
}
