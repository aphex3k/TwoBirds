using System;
using MonoTouch.MapKit;
using MonoTouch.UIKit;

namespace TwoBirds.iOS
{
	class MapDelegate : MKMapViewDelegate
	{
		//Override OverLayRenderer to draw Polyline from directions
		public override MKOverlayRenderer OverlayRenderer(MKMapView mapView, IMKOverlay overlay)
		{
			if (overlay is MKPolyline)
			{
				var route = (MKPolyline)overlay;
				var renderer = new MKPolylineRenderer(route) { StrokeColor = UIColor.Blue, LineWidth = 5.0f };
				return renderer;
			}
			return null;
		}
	}
}

