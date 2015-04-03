using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using TwoBirds.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

// This ExportRenderer command tells Xamarin.Forms to use this renderer
// instead of the built-in one for this page
using System.Text;


[assembly:ExportRenderer(typeof(TwoBirds.MySecondPage), typeof(MySecondPageRenderer))]

namespace TwoBirds.iOS
{
	public class Location
	{
		public double lat { get; set; }
		public double lng { get; set; }
	}

	public class Geometry
	{
		public Location location { get; set; }
	}

	public class OpeningHours
	{
		public bool open_now { get; set; }
		public List<object> weekday_text { get; set; }
	}

	public class Photo
	{
		public int height { get; set; }
		public List<object> html_attributions { get; set; }
		public string photo_reference { get; set; }
		public int width { get; set; }
	}

	public class Result
	{
		public Geometry geometry { get; set; }
		public string icon { get; set; }
		public string id { get; set; }
		public string name { get; set; }
		public OpeningHours opening_hours { get; set; }
		public List<Photo> photos { get; set; }
		public string place_id { get; set; }
		public double rating { get; set; }
		public string reference { get; set; }
		public string scope { get; set; }
		public List<string> types { get; set; }
		public string vicinity { get; set; }
		public int? price_level { get; set; }
	}

	public class NearbySearch
	{
		public List<object> html_attributions { get; set; }
		public List<Result> results { get; set; }
		public string status { get; set; }
	}

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

		//hardcoded for now:
		private bool _isRoundTrip = true;

		//v2 for incorporating multiple locations
		private List<BasicMapAnnotation> _errandLocations = new List<BasicMapAnnotation>();

		private bool _routeHasBeenCreated = false;

		private bool gotLocationInformation = false;

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
//
//			if (!string.IsNullOrEmpty(MyFirstPage.ErrandsList[0].Text))
//			{    
//				//This checks if it's an address that's been entered
//				//If it is, then we convert it to coordinates
//				//and don't send it through the yelp api
//				_iPhoneGeocoder.GeocodeAddress(MyFirstPage.ErrandsList[0].Text, (placemarks, error) =>
//					{
//						if ((placemarks != null) && (placemarks.Length > 0))
//						{
//							_errandLocationList.Add(new BasicMapAnnotation(placemarks[0].Location.Coordinate, "Errand 1", placemarks[0].Name));
//							//2 lines taken out by Melissa while adding new stuff muliple location mapping:
//							//map.AddAnnotation(_errandLocationList[0]);
//							//CreateRoute();
//					
//						}
//					});
//			}



				//                for (int i = 0; i < locationSequence.Count; i++) {
				//                    for (int j = 0; j < locations.Count; j++) {
				//                        if (locationSequence [i] == j) {
				//                            //Add to _errandLocations list in optimized order and add annotations
				//                            _errandLocations.Add (new BasicMapAnnotation (new CLLocationCoordinate2D (locations [j].Lat, locations [j].Long), locations [j].Title, locations [j].Subtitle));
				//                        }
				//                    }
				//                }

				//Add to _errandLocations list and add annotations 
				//                _errandLocations.Add(new BasicMapAnnotation (new CLLocationCoordinate2D(locations[0]), "Melissa's house", "6566 Camino Del Rey, Bonsall, CA 92003"));
				//                _errandLocations.Add(new BasicMapAnnotation (new CLLocationCoordinate2D(locations[1]), "Target", "1280 Auto Park Way, Escondido, CA 92029"));
				//                _errandLocations.Add(new BasicMapAnnotation (new CLLocationCoordinate2D(locations[2]), "Jo-Ann Fabric", "1680 E Valley Parkway, Escondido, CA 92027"));
				//                _errandLocations.Add(new BasicMapAnnotation (new CLLocationCoordinate2D(locations[3]), "Trader Joe's", "1885 S Centre City Pkwy\nEscondido, CA 92025"));
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
					if (!gotLocationInformation)
					{
						GetLocationInformation();
						gotLocationInformation = true;
					}
				};
			}
			else
			{
				#pragma warning disable 618
				// this won't be called on iOS 6 (deprecated)
				_iPhoneLocationManager.UpdatedLocation += (sender, e) =>
				{
					UpdateLocation(e.NewLocation);
					if (!gotLocationInformation)
					{
						GetLocationInformation();
						gotLocationInformation = true;
					}
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

		private NearbySearch GetNearbySearch(string latlng, string errand)
		{
			var googlePlaces = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={0}", latlng);

			var radius = "&radius=7500";

			var lastStringPart = "&sensor=false&key=AIzaSyDouP4A3_XqFdHn05S0u-f6CxBX0256ZtU";

			StringBuilder listOfTerms = new StringBuilder();

			listOfTerms.Append("&name=" + errand);

			string queryString = googlePlaces + radius + listOfTerms + lastStringPart;

			WebClient webRequest = new WebClient();
			string request = webRequest.DownloadString(queryString);
			
			return JsonConvert.DeserializeObject<NearbySearch>(request);
		}

		private void GetLocationInformation()
		{
			string latlng = null;
			if (string.IsNullOrWhiteSpace (MyFirstPage.MyLocation.Text)) {
				var lat = _myLocation.Coordinate.Latitude;
				var lng = _myLocation.Coordinate.Longitude;
				latlng = string.Format ("{0},{1}", lat, lng);
			} else {
				latlng= MyFirstPage.MyLocation.Text;
				latlng.Replace(" ", "+");
				string latlong = GetLatLng (latlng);
			}

			List<Coordinates> locations = new List<Coordinates>();
			locations.Add(new Coordinates(_myLocation.Coordinate.Latitude, _myLocation.Coordinate.Longitude, "My Location", ""));

			for (int i = 0; i < MyFirstPage.ErrandsList.Count; i++)
			{
				NearbySearch l = GetNearbySearch(latlng, MyFirstPage.ErrandsList[i].Text);
				locations.Add(new Coordinates(
					l.results[i].geometry.location.lat,
					l.results[i].geometry.location.lng,
					l.results[i].name,
					l.results[i].vicinity));
			}


			//when yelp api comes back, pass in the coordinates in for each of the 27 locations and set them in this list.NJK
			//New code after Brett - to map multiple locations (not just two)
			//This will be after it's been sent through the yelp api:


			//Hit up mapquest api here to get optimized route:
			string uri = "http://open.mapquestapi.com/directions/v2/optimizedroute?key=Fmjtd%7Cluu829u8l1%2Cb2%3Do5-9w1wgy&json={locations:[";

			string param = "";
			for (int i = 0; i < locations.Count; i++) {
				if (i == locations.Count - 1) {
					param += "{latLng:{lat:" + locations[i].Lat + ",lng:" + locations[i].Long + "}}]}";
				} else {
					param += "{latLng:{lat:" + locations[i].Lat + ",lng:" + locations[i].Long + "}},";
				}
			}

			uri = uri + param;
			WebClient webpage = new WebClient();
			string source = webpage.DownloadString(uri);
			JToken t = JToken.Parse(source);
			string location_string;
			int location_int;
			List<int> locationSequence = new List<int>();

			if (t["route"] != null && t["route"]["locationSequence"] != null) {
				//locationSequence is zero indexed
				foreach (var locationInt in t["route"]["locationSequence"]) 
				{
					location_string = locationInt.ToString ();
					location_int = int.Parse (location_string);
					locationSequence.Add (location_int);
					Console.WriteLine (location_int);

					foreach (var location in locations.Select((x,i) => new { Value = x, Index = i})) {
						if (location_int == location.Index) {
							//Add to _errandLocations list in optimized order and add annotations
							_errandLocations.Add (new BasicMapAnnotation (new CLLocationCoordinate2D (location.Value.Lat, location.Value.Long), location.Value.Title, location.Value.Subtitle));
						}
					}
				}

				foreach (var location in _errandLocations) {
					map.AddAnnotation (location);
				}
			}
		
			CreateRoute();

			var buttonRect = UIButton.FromType(UIButtonType.DetailDisclosure);
			buttonRect.Center = new PointF(View.Frame.Right - 20, View.Frame.Top + 20);
			View.AddSubview(buttonRect);
			buttonRect.TouchUpInside += _page.HandleTouchUpInside;
		}
	
		private void UpdateLocation(CLLocation newLocation)
		{
			if (_myLocation == null)
			{
				_myLocation = new BasicMapAnnotation(newLocation.Coordinate, "My Location", "My Starting Location");
				map.AddAnnotation(_myLocation);
			}
			_myLocation.Coordinate = newLocation.Coordinate;

			if (!string.IsNullOrEmpty(MyFirstPage.MyLocation.Text))
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

		class Coordinates
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
		}

		public void CreateRoute()
		{
			//if (_routeHasBeenCreated)
			//    return;

			//commented out by Melissa for a moment:
			//if (_myLocation == null || _errandLocationList.Count == 0)
			//    return;

			//clear out any previous polyline routes
			if (_myRoutes != null)
				foreach (var route in _myRoutes)
					map.RemoveOverlay (route.Polyline);

			//Create Origin and Dest Place Marks and Map Items to use for directions
			var emptyDict = new NSDictionary();

			if (_isRoundTrip) {
				_errandLocations.Add (_errandLocations [0]);
			}

			for (int i = 0; i < _errandLocations.Count - 1; i++) 
			{
				var orignPlaceMark = new MKPlacemark (_errandLocations [i].Coordinate, emptyDict);
				var sourceItem = new MKMapItem (orignPlaceMark);

				var destPlaceMark = new MKPlacemark (_errandLocations [i + 1].Coordinate, emptyDict);
				var destItem = new MKMapItem (destPlaceMark);

				var request = new MKDirectionsRequest {
					Source = sourceItem,
					Destination = destItem
						//removed alt routes for now because it's displaying the route for alt routes
						//on the map without any differentiation from main route
						//this will have to be designated on the polyline somehow
						//RequestsAlternateRoutes = true
				};

				var directions = new MKDirections (request);

				directions.CalculateDirections ((response, error) => {
					if (error != null) {
						Console.WriteLine (error.LocalizedDescription);
					} else {
						MyThirdPage thirdPage = _page.ThirdPage as MyThirdPage;
						thirdPage.Directions.Clear ();

						//if (_myRoutes != null)
						//    foreach (var route in _myRoutes)
						//        map.RemoveOverlay (route.Polyline);

						_myRoutes = response.Routes;

						//Add each Polyline from route to map as overlay
						foreach (var route in response.Routes) {
							map.AddOverlay (route.Polyline, MKOverlayLevel.AboveRoads);

							foreach (var step in route.Steps) {
								thirdPage.Directions.Add (step.Instructions);
								//add the step-by-step instructions to the UI
								Console.WriteLine (step.Instructions);
								//We can extract:
								//Console.WriteLine(step.Distance);
							}
						}
						thirdPage.MyListView.ItemsSource = thirdPage.Directions.ToArray ();
						_routeHasBeenCreated = true;
					}
				});
			}
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

		private string GetLatLng(string address)
		{
			address = Uri.EscapeUriString(address);
			//DirectionsResponse response = new DirectionsResponse();

			string uri = "https://maps.googleapis.com/maps/api/geocode/json?address=%s" + address;

			WebClient webpage = new WebClient();
			string source = webpage.DownloadString(uri);

			JToken t = JToken.Parse(source);
	

			return source;
		}
	
//		public string SearchDestinations(string[] terms, string ll)
//		{
//
//			List<object> Businesses = new List<object>();
//			YelpAPI.YelpAPIClient apiclient = new YelpAPI.YelpAPIClient();
//			try
//			{
//				foreach (string t in terms)
//				{
//					//string encodedLocation = HttpContext.Current.Server.UrlEncode(ll);
//					string encodedTerm = HttpContext.Current.Server.UrlEncode(t);
//					var businesses = apiclient.Search(encodedTerm, ll);
//					Businesses.Add(businesses);
//				}
//
//				string businessString = JsonConvert.SerializeObject(Businesses);
//
//				resp = Request.CreateResponse(HttpStatusCode.OK, businessString);
//				resp.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
//			}
//			catch (Exception ex)
//			{
//				resp = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
//			}
//			return resp;
//		}


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



