using System;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;

namespace TwoBirds.iOS
{
	public class BasicMapAnnotation : MKAnnotation
	{
		public override CLLocationCoordinate2D Coordinate
		{
			get { return this.Coords; }
			set { this.Coords = value; }
		}

		public CLLocationCoordinate2D Coords;
		public string title, subtitle;
		public override string Title { get { return title; } }
		public override string Subtitle { get { return subtitle; } }
		public BasicMapAnnotation(CLLocationCoordinate2D coordinate, string title, string subtitle)
		{
			this.Coords = coordinate;
			this.title = title;
			this.subtitle = subtitle;
		}
	}
}

