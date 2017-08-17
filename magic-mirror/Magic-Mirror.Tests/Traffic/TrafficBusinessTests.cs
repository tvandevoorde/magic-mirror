﻿using MagicMirror.Business.Models;
using MagicMirror.Business.Services;
using MagicMirror.DataAccess.Repos;
using MagicMirror.Entities.Traffic;
using System.Threading.Tasks;
using Xunit;

namespace MagicMirror.Tests.Traffic
{
    public class TrafficBusinessTests
    {
        private readonly IRepo<TrafficEntity> _repo;
        private readonly IService<TrafficModel> _service;

        public TrafficBusinessTests()
        {
            var criteria = new SearchCriteria()
            {
                HomeAddress = "Heikant 51",
                HomeCity = "3390 Houwaart",
                WorkAddress = "Generaal Armstrongweg 1 Antwerpen",
            };

            _repo = new TrafficRepo(criteria.HomeAddress, criteria.WorkAddress);
            _service = new TrafficService(criteria);
        }

        [Fact]
        public async Task AutoMapped_Fields_Correct()
        {
            // Arrange
            TrafficEntity entity = await _repo.GetEntityAsync();

            // Act
            TrafficModel model = await _service.GetModelAsync();

            // Assert
            Assert.NotNull(model.Distance);
            Assert.NotEqual("0", model.Distance);
                
            Assert.NotNull(model.Minutes);
            Assert.NotEqual(0, model.Minutes);
            Assert.Equal(model.Minutes, entity.Routes[0].Legs[0].Duration.Value / 60);

            Assert.NotNull(model.MinutesText);
            Assert.NotEqual("", model.MinutesText);
            Assert.Equal(model.MinutesText, entity.Routes[0].Legs[0].Duration.Text);
        }

        [Fact]
        public void Calculated_Fields_Correct()
        {
            // Arrange
            var model = new TrafficModel
            {
                Minutes = 5400,
                NumberOfIncidents = 3,
            };

            // Act
            model = _service.CalculateMappedValues(model);

            // Assert
            Assert.Equal(90, model.Minutes);
            Assert.Equal(TrafficDensity.Heavy, model.TrafficDensity);
        }
    }
}