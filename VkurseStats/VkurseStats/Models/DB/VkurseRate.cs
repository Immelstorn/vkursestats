﻿using System;

namespace VkurseStats.Models.DB
{
    public class VkurseRate
    {
        public VkurseRate()
        {
            Timestamp = DateTime.UtcNow;
        }

        public int Id { get; set; }
        public double UsdBuy { get; set; }
        public double UsdSell { get; set; }
        public double EurBuy { get; set; }
        public double EurSell { get; set; }
        public DateTime Timestamp { get; set; }
    }
}