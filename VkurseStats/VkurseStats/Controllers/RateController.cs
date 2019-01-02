using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;

using RestSharp;

using VkurseStats.Models;
using VkurseStats.Models.DB;

namespace VkurseStats.Controllers
{
    public class RateController : ApiController
    {
        private const string Uri = "http://vkurse.dp.ua/course.json";
        private const string UriNbu = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json&valcode=DKK";

        public JsonResult<List<VkurseRate>> Post(string period)
        {
            using (var db = new DataContext())
            {
                var ago = DateTime.UtcNow.AddMonths(-1);
                switch (period)
                {
                    case "day":
                        ago = DateTime.UtcNow.AddDays(-1);
                        break;
                    case "week":
                        ago = DateTime.UtcNow.AddDays(-7);
                        break;
                    case "month":
                        ago = DateTime.UtcNow.AddMonths(-1);
                        break;
                    case "3month":
                        ago = DateTime.UtcNow.AddMonths(-3);
                        break;
                    case "year":
                        ago = DateTime.UtcNow.AddYears(-1);
                        break;
                    case "all":
                        ago = DateTime.MinValue;
                        break;

                }
                var records = db.VkurseRates.Where(r => r.Timestamp > ago && r.DkkRate > 0).ToList();
                return Json(records);
            }
        }

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

                    if (!double.TryParse(content.Dollar.Buy.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var usdbuy))
                    {
                        usdbuy = lastToday.UsdBuy;
                    }
                    if (!double.TryParse(content.Dollar.Sale.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var usdsell))
                    {
                        usdsell = lastToday.UsdSell;
                    }
                    if (!double.TryParse(content.Euro.Buy.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var eurbuy))
                    {
                        eurbuy = lastToday.EurBuy;
                    }
                    if (!double.TryParse(content.Euro.Sale.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var eursell))
                    {
                        eursell = lastToday.EurSell;
                    }

                    client = new RestClient(UriNbu);
                    request = new RestRequest();
                    var nburesponse = client.Execute<List<Nbu>>(request);
                    var dkkRate = lastToday.DkkRate;

                    if (nburesponse?.Data != null && nburesponse.Data.Any())
                    {
                        double.TryParse(nburesponse.Data.First().Rate.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out dkkRate);
                    }
                    
                    var newRate = new VkurseRate {
                        UsdBuy = usdbuy,
                        UsdSell = usdsell,
                        EurBuy = eurbuy,
                        EurSell = eursell,
                        DkkRate = dkkRate
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
                    },
                    Dkk = new RateDto {
                        Buy = lastToday.DkkRate,
                        ChangePercentBuy = Math.Round((lastToday.DkkRate / yesterdayValue.DkkRate - 1) * 100, 2),
                    },
                    Salary = new RateDto {
                        Buy = lastToday.Salary,
                        ChangePercentBuy = Math.Round((lastToday.Salary / yesterdayValue.Salary - 1) * 100, 2),
                    }
                };

                return Ok(result);
            }
        }
    }
}
