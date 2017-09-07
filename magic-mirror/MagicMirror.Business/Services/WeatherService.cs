﻿using Acme.Generic;
using MagicMirror.Business.Models;
using MagicMirror.DataAccess.Entities;
using MagicMirror.DataAccess.Entities.Weather;
using MagicMirror.DataAccess.Repos;
using System;
using System.Threading.Tasks;

namespace MagicMirror.Business.Services
{
    public class WeatherService : ServiceBase, IService<WeatherModel>
    {
        private IRepo<WeatherEntity> _repo;
        private SearchCriteria _criteria;

        public WeatherService(SearchCriteria criteria)
        {
            // Defensive coding
            if (criteria == null) throw new ArgumentNullException("No search criteria provided", nameof(criteria));
            if (string.IsNullOrWhiteSpace(criteria.HomeCity)) throw new ArgumentException("A city has to be provided");

            _criteria = criteria;
        }

        public async Task<WeatherModel> GetModelAsync()
        {
            // Get entity from Repository.
            _repo = new WeatherRepo(_criteria.HomeCity);
            WeatherEntity entity = await _repo.GetEntityAsync();

            // Map entity to model.
            WeatherModel model = MapEntityToModel(entity);

            // Calculate non-mappable values
            model = CalculateMappedValues(model);

            // Set icon
            model.Icon = ConvertWeatherIcon(model.Icon);

            return model;
        }

        /// <summary>
        /// Convert an OpenWeatherMap icon to a Metro Style weather icon
        /// </summary>
        /// <param name="icon">OpenweatherMap icon to convert</param>
        /// <param name="theme">The colour scheme. Choice between light and dark</param>
        /// <returns></returns>
        private string ConvertWeatherIcon(string icon, string theme = "Dark")
        {
            try
            {
                string prefix = "ms-appx:///Assets/Weather";
                string res;

                switch (icon)
                {
                    case "01d":
                        res = "01d.png";
                        break;

                    case "01n":
                        res = "01n.png";
                        break;

                    case "02d":
                        res = "02d.png";
                        break;

                    case "02n":
                        res = "02n.png";
                        break;

                    case "03d":
                    case "03n":
                    case "04d":
                    case "04n":
                        res = "03or4.png";
                        break;

                    case "09n":
                    case "09d":
                        res = "09.png";
                        break;

                    case "10d":
                    case "10n":
                        res = "09.png";
                        break;

                    case "11d":
                        res = "11d.png";
                        break;

                    case "11n":
                        res = "11n.png";
                        break;

                    case "13d":
                    case "13n":
                        res = "13.png";
                        break;

                    case "50n":
                    case "50d":
                    default:
                        res = "50.png";
                        break;
                }
                return $"{prefix}/{theme}/{res}";
            }
            catch (Exception ex)
            {
                //DisplayErrorMessage("Cannot set weather icon");
                return "";
            }
        }

        /// <summary>
        /// Calculate the model's fields which cannot be resolved using Automapper.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private WeatherModel CalculateMappedValues(WeatherModel model)
        {
            switch (_criteria.TemperatureUOM)
            {
                case TemperatureUOM.Celsius:
                    model.TemperatureCelsius = TemperatureHelper.KelvinToCelsius(model.TemperatureKelvin, _criteria.Precision);
                    break;

                case TemperatureUOM.Fahrenheit:
                    model.TemperatureFahrenheit = TemperatureHelper.KelvinToFahrenheit(model.TemperatureKelvin, _criteria.Precision);
                    break;

                default:
                    break;
            }

            DateTime sunrise = DateHelper.ConvertFromUnixTimestamp(model.SunRiseMilliseconds);
            model.SunRise = sunrise.ToString("HH:mm");

            DateTime sunset = DateHelper.ConvertFromUnixTimestamp(model.SunSetMilliSeconds);
            model.SunSet = sunset.ToString("HH:mm");
            return model;
        }

        /// <summary>
        /// Map Entity to Business Model using AutoMapper
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private WeatherModel MapEntityToModel(Entity entity)
        {
            WeatherModel model = Mapper.Map<WeatherModel>(entity);
            return model;
        }
    }
}