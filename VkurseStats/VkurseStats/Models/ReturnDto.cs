using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VkurseStats.Models
{
    public class ReturnDto
    {
        public RateDto Usd { get; set; }
        public RateDto Eur { get; set; }
    }
}