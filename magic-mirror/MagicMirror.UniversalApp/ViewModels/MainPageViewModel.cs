﻿using MagicMirror.Business.Models;
using MagicMirror.Business.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MagicMirror.UniversalApp.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        // Services from the Business Layer
        private readonly IService<WeatherModel> _weatherService;

        private readonly IService<TrafficModel> _trafficService;
        private readonly CommonService _commonService;

        // Timers to refresh individual components
        private DispatcherTimer timeTimer;

        private DispatcherTimer complimentTimer;
        private DispatcherTimer weatherTimer;
        private DispatcherTimer trafficTimer;

        public MainPageViewModel()
        {
            App appReference = Application.Current as App;
            SearchCriteria searchCriteria = appReference.Criteria;

            _commonService = new CommonService();
            _weatherService = new WeatherService(searchCriteria);
            _trafficService = new TrafficService(searchCriteria);

            timeTimer = new DispatcherTimer();
            complimentTimer = new DispatcherTimer();
            weatherTimer = new DispatcherTimer();
            trafficTimer = new DispatcherTimer();

            Initialize();
            SetRefreshTimers();
        }

        /// <summary>
        /// Call data immediately after app launch
        /// </summary>
        private void Initialize()
        {
            RefreshTime(null, null);
            RefreshCompliment(null, null);
            RefreshWeatherModel(null, null);
            RefreshTrafficModel(null, null);
        }

        /// <summary>
        /// Set timers at which data needs to be refreshed
        /// </summary>
        private void SetRefreshTimers()
        {
            timeTimer.Tick += RefreshTime;
            timeTimer.Interval = new TimeSpan(0, 1, 0);
            if (!timeTimer.IsEnabled) timeTimer.Start();

            complimentTimer.Tick += RefreshCompliment;
            complimentTimer.Interval = new TimeSpan(0, 1, 0);
            if (!complimentTimer.IsEnabled) complimentTimer.Start();

            weatherTimer.Tick += RefreshWeatherModel;
            weatherTimer.Interval = new TimeSpan(0, 15, 0);
            if (!weatherTimer.IsEnabled) weatherTimer.Start();

            trafficTimer.Tick += RefreshTrafficModel;
            trafficTimer.Interval = new TimeSpan(0, 10, 0);
            if (!trafficTimer.IsEnabled) trafficTimer.Start();
        }

        private void RefreshTime(object sender, object e)
        {
            try
            {
                Time = DateTime.Now.ToString("HH:mm");
            }
            catch (Exception ex)
            {
                DisplayErrorMessage("Cannot set Time", ex.Message);
            }
        }

        private void RefreshCompliment(object sender, object e)
        {
            try
            {
                Compliment = _commonService.GenerateCompliment();
            }
            catch (Exception ex)
            {
                DisplayErrorMessage("Cannot set Compliment", ex.Message);
            }
        }

        private async void RefreshWeatherModel(object sender, object e)
        {
            try
            {
                WeatherModel weatherModel =  await _weatherService.GetModelAsync();
                weatherModel.Icon = ConvertWeatherIcon(weatherModel.Icon);
                WeatherInfo = weatherModel;

                if (!weatherTimer.IsEnabled) weatherTimer.Start();
            }
            catch (Exception)
            {
                // Can't connect to server. Try again after waiting for a few minutes
                DisplayErrorMessage("Can't update weather information", "Check your internet connection and try again");
                if (weatherTimer.IsEnabled) weatherTimer.Stop();

                // Try to refresh data. If succesful, resume timer
                int minutes = 5;
                await Task.Delay((minutes * 60) * 1000);
                RefreshWeatherModel(null, null);
            }
        }

        private async void RefreshTrafficModel(object sender, object e)
        {
            try
            {
                TrafficModel result = await _trafficService.GetModelAsync();
                TrafficInfo = result;

                if (!trafficTimer.IsEnabled) trafficTimer.Start();
            }
            catch (Exception)
            {
                // Can't connect to server. Try again after waiting for a few minutes
                DisplayErrorMessage("Can't update traffic information", "Check your internet connection and try again");
                if (weatherTimer.IsEnabled) weatherTimer.Stop();

                // Try to refresh data immediately. If succesful, resume timer
                int minutes = 5;
                await Task.Delay((minutes * 60) * 1000);
                RefreshTrafficModel(null, null);
            }
        }

        // Todo: Refresh
        private string ConvertWeatherIcon(string icon)
        {
            try
            {
                string theme = "Dark";
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
                DisplayErrorMessage("Cannot set weather icon");
                return "";
            }
        }

        // Todo: Only one dialog can be open
        private async void DisplayErrorMessage(string title, string content = "")
        {
            var errorMessage = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = "Ok"
            };
            ContentDialogResult result = await errorMessage.ShowAsync();
        }

        #region Properties

        private WeatherModel _weather;

        public WeatherModel WeatherInfo
        {
            get => _weather;
            set
            {
                _weather = value;
                OnPropertyChanged();
            }
        }

        private TrafficModel _traffic;

        public TrafficModel TrafficInfo
        {
            get => _traffic;
            set
            {
                _traffic = value;
                OnPropertyChanged();
            }
        }

        public DateModel Date => new DateModel();

        private string _time;

        public string Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }

        private string _compliment;

        public string Compliment
        {
            get => _compliment;
            set
            {
                _compliment = value;
                OnPropertyChanged();
            }
        }

        public void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Properties
    }
}