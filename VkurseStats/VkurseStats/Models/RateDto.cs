using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VkurseStats.Models
{
    public class RateDto
    {
        public double Buy { get; set; }
        public double Sell { get; set; }
        public double ChangePercentBuy { get; set; }
        public double ChangePercentSell { get; set; }
    }
}