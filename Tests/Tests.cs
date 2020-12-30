using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Inkybot;
using Inkybot.Api;
using Inkybot.Contracts;
using Inkybot.Design;
using Inkybot.Domain;
using Inkybot.Domain.Repositories;
using Inkybot.Services;
using NUnit.Framework;
using Tests.Design;
using DofusMagingAI = Inkybot.Services.DofusMagingAI;
using DofusMagingJobContract = Inkybot.Contracts.DofusMagingJob;
using ServiceContainer = Inkybot.Design.ServiceContainer;
using StatisticsManager = Inkybot.Services.StatisticsManager;
using StatisticsManagerContract = Inkybot.Contracts.StatisticsManager;
using DofusMagingAIContract = Inkybot.Contracts.DofusMagingAI;

namespace Tests
{
    [TestFixture]
    public class Tests : SingleImageScreenReaderTest
    {
        protected override string Path => "./Resources/Screenshots/1.png";

        [Test]
        public void Test1() {
            DataProvider.FetchData();
            var item = DataProvider.Item();
            var expected = new Item(new ItemStatRepository(new [] {
                new ItemStat("vitality", 243, 201, 250), 
                new ItemStat("strength", 52, 41, 60), 
                new ItemStat("wisdom", 13, 16, 20), 
                new ItemStat("critical", 7, 4, 7), 
                new ItemStat("neutral_damage", 11, 9, 12), 
                new ItemStat("earth_damage", 11, 9, 12), 
                new ItemStat("per_water_resistance", 8, 7, 10), 
                new ItemStat("ap_parry", -5, -5, -5), 
                new ItemStat("critical_damage", 8, 7, 10), 
            }));
            
            Assert.True(expected == item);
        }
    }
}
