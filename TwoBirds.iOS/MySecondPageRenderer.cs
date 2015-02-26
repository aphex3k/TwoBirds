using System;
using System.Drawing;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using Xamarin.Forms.Platform.iOS;
using MonoTouch.UIKit;
using Xamarin.Forms;

// This ExportRenderer command tells Xamarin.Forms to use this renderer
// instead of the built-in one for this page
[assembly:ExportRenderer(typeof(TwoBirds.MySecondPage), typeof(TwoBirds.MySecondPageRenderer))]

namespace TwoBirds
{
	/// <summary>
	/// Render this page using platform-specific UIKit controls
	/// </summary>
	public class MySecondPageRenderer : PageRenderer
	{
		MKMapView map;
		MKMapViewDelegate _mapDelegate;

		double lat_origin = 33.285287;
		double long_origin = -117.189992;
		string title_origin = "Melissa's House";
		string subtitle_origin = "Melissa lives here!";

		double lat_dest = 33.128718;
		double long_dest = -117.159529;
		string title_dest = "Cal State San Marcos";
		string subtitle_dest = "Central campus location";

		public override void LoadView()
		{
			map = new MKMapView(UIScreen.MainScreen.Bounds);

			//Create new MapDelegate Instance
			_mapDelegate = new MapDelegate();

			//Add delegate to map
			map.Delegate = _mapDelegate;

			View = map;

			//New stuff for directions:
			CreateRoute();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// set map type and show user location
			map.MapType = MKMapType.Standard;
			map.ShowsUserLocation = true;

			// set map type and show user location
			map.MapType = MKMapType.Standard;
			map.ShowsUserLocation = true;
			map.ZoomEnabled = true;
			map.ScrollEnabled = true;

			// set map center and region
			CLLocationCoordinate2D mapCenter = new CLLocationCoordinate2D(lat_origin, long_origin);
			MKCoordinateRegion mapRegion = new MKCoordinateRegion(mapCenter, new MKCoordinateSpan(.25, .25));
			map.CenterCoordinate = mapCenter;
			map.Region = mapRegion;

			var annotation1 = new BasicMapAnnotation(new CLLocationCoordinate2D(lat_origin, long_origin), title_origin, subtitle_origin);
			map.AddAnnotation(annotation1);
			var annotation2 = new BasicMapAnnotation(new CLLocationCoordinate2D(lat_dest, long_dest), title_dest, subtitle_dest);
			map.AddAnnotation(annotation2);

			var buttonRect = UIButton.FromType(UIButtonType.DetailDisclosure);
			buttonRect.Center = new PointF(View.Frame.Right - 10, View.Frame.Top - 10);
			Console.WriteLine(View.Frame);
			View.AddSubview(buttonRect);
		}

		class BasicMapAnnotation : MKAnnotation
		{
			public override CLLocationCoordinate2D Coordinate
			{
				get { return this.Coords; }
				set { this.Coords = value; }
			}

			public CLLocationCoordinate2D Coords;
			string title, subtitle;
			public override string Title { get { return title; } }
			public override string Subtitle { get { return subtitle; } }
			public BasicMapAnnotation(CLLocationCoordinate2D coordinate, string title, string subtitle)
			{
				this.Coords = coordinate;
				this.title = title;
				this.subtitle = subtitle;
			}
		}

		public void CreateRoute()
		{
			//Create Origin and Dest Place Marks and Map Items to use for directions
			var emptyDict = new NSDictionary();

			var orignPlaceMark = new MKPlacemark(new CLLocationCoordinate2D(lat_origin, long_origin), emptyDict);
			var sourceItem = new MKMapItem(orignPlaceMark);

			var destPlaceMark = new MKPlacemark(new CLLocationCoordinate2D(lat_dest, long_dest), emptyDict);
			var destItem = new MKMapItem(destPlaceMark);

			var request = new MKDirectionsRequest
			{
				Source = sourceItem,
				Destination = destItem
				//removed alt routes for now because it's displaying the route for alt routes
				//on the map without any differentiation from main route
				//this will have to be designated on the polyline somehow
				//RequestsAlternateRoutes = true
			};

			var directions = new MKDirections(request);

			directions.CalculateDirections((response, error) =>
			{
				if (error != null)
				{
					Console.WriteLine(error.LocalizedDescription);
				}
				else
				{
					//Add each Polyline from route to map as overlay
					foreach (var route in response.Routes)
					{
						map.AddOverlay(route.Polyline, MKOverlayLevel.AboveRoads);

						foreach (var step in route.Steps)
						{
							//add the step-by-step instructions to the UI
							Console.WriteLine(step.Instructions);
							//We can extract:
							//Console.WriteLine(step.Distance);
						}
					}
				}
			});
		}

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

		// returns a MKCoordinateRegion that encompasses an array of MKAnnotations
		//        public MKCoordinateRegion getRegionForAnnotations(List<Coordinates> annotations) 
		//        {
		//
		//            CLLocationDegrees minLat = 90.0;
		//            CLLocationDegrees maxLat = -90.0;
		//            CLLocationDegrees minLon = 180.0;
		//            CLLocationDegrees maxLon = -180.0;
		//
		//            for (MKAnnotation annotation in annotations) {
		//                if (annotation.coordinates.latitude < minLat) {
		//                    minLat = annotation.coordinates.latitude;
		//                }        
		//                if (annotation.coordinates.longitude < minLon) {
		//                    minLon = annotation.coordinates.longitude;
		//                }        
		//                if (annotation.coordinates.latitude > maxLat) {
		//                    maxLat = annotation.coordinates.latitude;
		//                }        
		//                if (annotation.coordinates.longitude > maxLon) {
		//                    maxLon = annotation.coordinates.longitude;
		//                }
		//            }
		//
		//            MKCoordinateSpan span = MKCoordinateSpanMake(maxLat - minLat, maxLon - minLon);
		//
		//            CLLocationCoordinate2D center = CLLocationCoordinate2DMake((maxLat - span.latitudeDelta / 2), maxLon - span.longitudeDelta / 2);
		//
		//            return MKCoordinateRegionMake(center, span);
		//        }

	}
}

