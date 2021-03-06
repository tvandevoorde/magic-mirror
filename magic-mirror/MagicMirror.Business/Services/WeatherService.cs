﻿using Acme.Generic;
using Acme.Generic.Extensions;
using MagicMirror.Business.Models;
using MagicMirror.DataAccess.Entities.Entities;
using MagicMirror.DataAccess.Repos;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MagicMirror.Business.Services
{
    public class WeatherService : ServiceBase<WeatherModel, WeatherEntity>, IWeatherService
    {
        private const string OFFLINEMODELNAME = "WeatherOfflineModel.json";

        public WeatherService()
        {
        }

        public async Task<WeatherModel> GetModelAsync(string homeCity, int precision, TemperatureUOM temperatureUOM)
        {
            try
            {
                _repo = new WeatherRepo(homeCity);

                // Get entity from Repository.
                WeatherEntity entity = await _repo.GetEntityAsync();

                // Map entity to model.
                WeatherModel model = MapEntityToModel(entity);

                // Calculate non-mappable values
                model = CalculateUnMappableValues(model, precision, temperatureUOM);

                // Todo: Implement bool if user wants metro or openweather icons
                if (true)
                {
                    model.Icon = ConvertWeatherIcon(model.Icon);
                }

                return model;
            }
            catch (HttpRequestException ex) { throw ex; }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to retrieve Weather Model", ex);
            }
        }

        public WeatherModel GetOfflineModel(string path)
        {
            try
            {
                //Try reading Json object
                string json = FileWriter.ReadFromFile(path, OFFLINEMODELNAME);
                WeatherModel model = JsonConvert.DeserializeObject<WeatherModel>(json);

                return model;
            }
            catch (FileNotFoundException)
            {
                // Object does not exist. Create a new one
                WeatherModel offlineModel = GenerateOfflineModel();
                SaveOfflineModel(offlineModel, path);

                return GetOfflineModel(path);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not read offline Weathermodel", e);
            }
        }

        public void SaveOfflineModel(WeatherModel model, string path)
        {
            try
            {
                string json = model.ToJson();
                FileWriter.WriteJsonToFile(json, OFFLINEMODELNAME, path);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Could not save offline Weather Model", e);
            }
        }

        private WeatherModel GenerateOfflineModel()
        {
            return new WeatherModel
            {
                Icon = "01d",
                Location = "Mechelen",
                SunRise = "06:44",
                SunSet = "19:42",
                TemperatureCelsius = 13,
                TemperatureFahrenheit = 55.40,
                TemperatureKelvin = 286.15,
                WeatherType = "Sunny"
            };
        }

        protected WeatherModel CalculateUnMappableValues(WeatherModel model, int precision, TemperatureUOM temperatureUOM)
        {
            model.TemperatureCelsius = TemperatureHelper.KelvinToCelsius(model.TemperatureKelvin, precision);
            model.TemperatureFahrenheit = TemperatureHelper.KelvinToFahrenheit(model.TemperatureKelvin, precision);

            switch (temperatureUOM)
            {
                case TemperatureUOM.Celsius:
                    model.Temperature = model.TemperatureCelsius;
                    break;

                case TemperatureUOM.Fahrenheit:
                    model.Temperature = model.TemperatureFahrenheit;
                    break;

                case TemperatureUOM.Kelvin:
                    model.Temperature = model.TemperatureKelvin;
                    break;

                default:
                    model.Temperature = model.TemperatureCelsius;
                    break;
            }

            DateTime sunrise = model.SunRiseMilliseconds.ConvertFromUnixTimestamp();
            DateTime sunset = model.SunSetMilliSeconds.ConvertFromUnixTimestamp();
            model.SunRise = sunrise.ToString("HH:mm");
            model.SunSet = sunset.ToString("HH:mm");

            return model;
        }

        /// <summary>
        /// Convert an OpenWeatherMap icon to a Metro Style weather icon
        /// </summary>
        /// <param name="icon">OpenweatherMap icon to convert</param>
        /// <param name="theme">The colour scheme. Choice between light and dark</param>
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
                        res = "010.png";
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
            catch (Exception)
            {
                return "";
            }
        }
    }
}