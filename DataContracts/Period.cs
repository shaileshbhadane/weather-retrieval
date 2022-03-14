using System;

namespace weather_retrieval.DataContracts
{
    public class Period
    {
        public int number;
        public string startTime;
        public string endTime;
        public int temperature;
        public string temperatureUnit;
        public string windSpeed;
        public string windDirection;
        public Uri icon;

    }
}
