using System;

namespace TwoBirds.iOS
{
	public class Coordinates
	{
		public double Lat { get; set; }

		public double Long { get; set; }

		public string Title { get; set;}

		public string Subtitle { get; set;}

		public Coordinates(double lat, double lon, string title, string subtitle)
		{
			this.Lat = lat;
			this.Long = lon;
			this.Title = title;
			this.Subtitle = subtitle;
		}

		public Coordinates()
		{
		}
	}
}

