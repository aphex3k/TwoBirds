using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using TwoBirds.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

// This ExportRenderer command tells Xamarin.Forms to use this renderer
// instead of the built-in one for this page
[assembly:ExportRenderer(typeof(TwoBirds.MySecondPage), typeof(MySecondPageRenderer))]

namespace TwoBirds.iOS
{
	/// <summary>
	/// Render this page using platform-specific UIKit controls
	/// </summary>
	public class MySecondPageRenderer : PageRenderer
	{
		private MySecondPage _page;
		static MKMapView map;
		private MKRoute[] _myRoutes;
		MKMapViewDelegate _mapDelegate;

		CLLocationManager _iPhoneLocationManager;
		private CLGeocoder _iPhoneGeocoder;

		private bool _recenterMap = true;

		double lat_origin = 33.285287;
		double long_origin = -117.189992;
		string title_origin = "My Location";
		string subtitle_origin = "Starting Point";

		double lat_dest = 33.128718;
		double long_dest = -117.159529;
		string title_dest = "Cal State San Marcos";
		string subtitle_dest = "Central campus location";

		private BasicMapAnnotation _myLocation;
		private List<BasicMapAnnotation> _errandLocationList = new List<BasicMapAnnotation>();

		private bool _routeHasBeenCreated = false;

		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			_page = e.NewElement as MySecondPage;

			var hostViewController = ViewController;

			//var viewController = new UIViewController();

			//hostViewController.AddChildViewController(viewController);
			//hostViewController.View.Add(viewController.View);

			//viewController.DidMoveToParentViewController(hostViewController);
		}

		public override void LoadView()
		{
			map = new MKMapView(UIScreen.MainScreen.Bounds);

			//Create new MapDelegate Instance
			_mapDelegate = new MapDelegate();

			//Add delegate to map
			map.Delegate = _mapDelegate;

			View = map;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			InitializeLocationManager();

			_recenterMap = true;
			_routeHasBeenCreated = false;

			// set map type and show user location
			map.MapType = MKMapType.Standard;
			map.ShowsUserLocation = true;

			// set map type and show user location
			map.MapType = MKMapType.Standard;
			map.ShowsUserLocation = true;
			map.ZoomEnabled = true;
			map.ScrollEnabled = true;

			// set map center and region
			//CLLocationCoordinate2D mapCenter = new CLLocationCoordinate2D(lat_origin, long_origin);
			//MKCoordinateRegion mapRegion = new MKCoordinateRegion(mapCenter, new MKCoordinateSpan(.25, .25));
			//map.CenterCoordinate = mapCenter;
			//map.Region = mapRegion;

			//_locationAnnotationList.Add(new BasicMapAnnotation(new CLLocationCoordinate2D(lat_origin, long_origin), title_origin, subtitle_origin));
			//map.AddAnnotation(_locationAnnotationList[0]);
			if (!string.IsNullOrEmpty(MyFirstPage.ErrandsList[0].Text))
			{
				_iPhoneGeocoder.GeocodeAddress(MyFirstPage.ErrandsList[0].Text, (placemarks, error) =>
				{
					if ((placemarks != null) && (placemarks.Length > 0))
					{
						_errandLocationList.Add(new BasicMapAnnotation(placemarks[0].Location.Coordinate, "Errand 1", placemarks[0].Name));
						map.AddAnnotation(_errandLocationList[0]);
						CreateRoute();
					}
				});
			}

			var buttonRect = UIButton.FromType(UIButtonType.DetailDisclosure);
			buttonRect.Center = new PointF(View.Frame.Right - 20, View.Frame.Top + 20);
			View.AddSubview(buttonRect);
			buttonRect.TouchUpInside += _page.HandleTouchUpInside;
		}

		void InitializeLocationManager()
		{
			// initialize our location manager and callback handler
			_iPhoneLocationManager = new CLLocationManager { DesiredAccuracy = 1000 };

			_iPhoneGeocoder = new CLGeocoder();

			// uncomment this if you want to use the delegate pattern:
			//locationDelegate = new LocationDelegate (mainScreen);
			//iPhoneLocationManager.Delegate = locationDelegate;

			// you can set the update threshold and accuracy if you want:
			//iPhoneLocationManager.DistanceFilter = 10; // move ten meters before updating
			//iPhoneLocationManager.HeadingFilter = 3; // move 3 degrees before updating

			// you can also set the desired accuracy:
			// 1000 meters/1 kilometer
			// you can also use presets, which simply evaluate to a double value:
			//iPhoneLocationManager.DesiredAccuracy = CLLocation.AccuracyNearestTenMeters;

			// handle the updated location method and update the UI
			if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
			{
				_iPhoneLocationManager.LocationsUpdated += (sender, e) =>
				{
					UpdateLocation(e.Locations[e.Locations.Length - 1]);
				};
			}
			else
			{
#pragma warning disable 618
				// this won't be called on iOS 6 (deprecated)
				_iPhoneLocationManager.UpdatedLocation += (sender, e) =>
				{
					UpdateLocation(e.NewLocation);
				};
#pragma warning restore 618
			}

			//iOS 8 requires you to manually request authorization now - Note the Info.plist file has a new key called requestWhenInUseAuthorization added to.
			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				_iPhoneLocationManager.RequestWhenInUseAuthorization();
			}

			// handle the updated heading method and update the UI
			_iPhoneLocationManager.UpdatedHeading += (sender, e) =>
			{
			};

			// start updating our location, et. al.
			if (CLLocationManager.LocationServicesEnabled)
				_iPhoneLocationManager.StartUpdatingLocation();
			if (CLLocationManager.HeadingAvailable)
				_iPhoneLocationManager.StartUpdatingHeading();
		}

		private void UpdateLocation(CLLocation newLocation)
		{
			if (_myLocation == null)
			{
				_myLocation = new BasicMapAnnotation(newLocation.Coordinate, "My Location", "My Starting Location");
				map.AddAnnotation(_myLocation);
			}
			_myLocation.Coordinate = newLocation.Coordinate;

//			if (!string.IsNullOrEmpty(MyFirstPage.MyLocation.Text))
			{
				_iPhoneGeocoder.ReverseGeocodeLocation(newLocation, (CLPlacemark[] placemarks, NSError error) =>
				{
					if ((placemarks != null) && (placemarks.Length > 0))
					{
						MyFirstPage.MyLocation.Text = placemarks[0].Name;
						_myLocation.title = "My Location";
						_myLocation.subtitle = placemarks[0].Name;

						//New stuff for directions:
						CreateRoute();
					}
				});
			}

			if (_recenterMap)
			{
				// set map center and region
				CLLocationCoordinate2D mapCenter = new CLLocationCoordinate2D(newLocation.Coordinate.Latitude, newLocation.Coordinate.Longitude);
				MKCoordinateRegion mapRegion = new MKCoordinateRegion(mapCenter, new MKCoordinateSpan(.025, .025));
				map.CenterCoordinate = mapCenter;
				map.Region = mapRegion;

				_recenterMap = false;
			}

			//_startLocationAnnotation = new BasicMapAnnotation(new CLLocationCoordinate2D(lat_origin, long_origin), title_origin, subtitle_origin);

			//MyFirstPage.MyLocation.Text = newLocation.Coordinate.Longitude.ToString();
			//ms.LblAltitude.Text = newLocation.Altitude.ToString() + " meters";
			//ms.LblLongitude.Text = newLocation.Coordinate.Longitude.ToString() + "º";
			//ms.LblLatitude.Text = newLocation.Coordinate.Latitude.ToString() + "º";
			//ms.LblCourse.Text = newLocation.Course.ToString() + "º";
			//ms.LblSpeed.Text = newLocation.Speed.ToString() + " meters/s";

			//// get the distance from here to paris
			//ms.LblDistanceToParis.Text = (newLocation.DistanceFrom(new CLLocation(48.857, 2.351)) / 1000).ToString() + " km";
		}

		class BasicMapAnnotation : MKAnnotation
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

		public void CreateRoute()
		{
			//if (_routeHasBeenCreated)
			//	return;

			if (_myLocation == null || _errandLocationList.Count == 0)
				return;

			//Create Origin and Dest Place Marks and Map Items to use for directions
			var emptyDict = new NSDictionary();

			var orignPlaceMark = new MKPlacemark(_myLocation.Coordinate, emptyDict);
			var sourceItem = new MKMapItem(orignPlaceMark);

			var destPlaceMark = new MKPlacemark(_errandLocationList[0].Coordinate, emptyDict);
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
					MyThirdPage thirdPage = _page.ThirdPage as MyThirdPage;
					thirdPage.Directions.Clear();

					if (_myRoutes != null)
						foreach (var route in _myRoutes)
							map.RemoveOverlay(route.Polyline);

					_myRoutes = response.Routes;

					//Add each Polyline from route to map as overlay
					foreach (var route in response.Routes)
					{
						map.AddOverlay(route.Polyline, MKOverlayLevel.AboveRoads);

						foreach (var step in route.Steps)
						{
							thirdPage.Directions.Add(step.Instructions);
							//add the step-by-step instructions to the UI
							Console.WriteLine(step.Instructions);
							//We can extract:
							//Console.WriteLine(step.Distance);
						}
					}
					thirdPage.MyListView.ItemsSource = thirdPage.Directions.ToArray();
					_routeHasBeenCreated = true;
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

