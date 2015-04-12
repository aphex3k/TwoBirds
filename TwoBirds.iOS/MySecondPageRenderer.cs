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
	/// <summary>
	/// Render this page using platform-specific UIKit controls
	/// </summary>
	public class MySecondPageRenderer : PageRenderer
	{
		private MySecondPage _page;
		static MKMapView map;
		private MKRoute[] _myRoutes;
		MKMapViewDelegate _mapDelegate;

		protected LoadingOverlay _loadPop = null;

		CLLocationManager _iPhoneLocationManager;
		private CLGeocoder _iPhoneGeocoder;

		private bool _recenterMap = true;

		private BasicMapAnnotation _myLocation;
		//private List<BasicMapAnnotation> _errandLocationList = new List<BasicMapAnnotation>();

		//hardcoded for now:
		private bool _isRoundTrip = true;

		static NearbySearch l = new NearbySearch();

		static List<ErrandResults> locationResults = new List<ErrandResults> ();

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

			DisplayLoadingState ();

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

			//Note: 
			//radius max allowed = 50000, which is 31 miles
			//can remove radius param and instead set "rankby=distance"
			//--or- only if results come by empty, then search again with "rankby=distance"
			//but may be bad on performance
			//var radius = "&radius=7500";

			var lastStringPart = "&rankby=distance&sensor=false&key=AIzaSyDouP4A3_XqFdHn05S0u-f6CxBX0256ZtU";

			StringBuilder listOfTerms = new StringBuilder();

			listOfTerms.Append("&name=" + errand);

			//string queryString = googlePlaces + radius + listOfTerms + lastStringPart;

			string queryString = googlePlaces + listOfTerms + lastStringPart;

			WebClient webRequest = new WebClient();
			string request = webRequest.DownloadString(queryString);
			
			return JsonConvert.DeserializeObject<NearbySearch>(request);
		}

		//Get location info and map route
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
			Coordinates origin = new Coordinates (_myLocation.Coordinate.Latitude, _myLocation.Coordinate.Longitude, "My Location", "");
			locations.Add(origin);

			//If static locationResults is not null or empty, clear it out 
			//because we don't want to have a duplicate set of errands added
			locationResults.Clear();
			List<List<Coordinates>> closestLocationsPerErrand = new List<List<Coordinates>> (); 

			for (int i = 0; i < MyFirstPage.ErrandsList.Count; i++)
			{
				l = GetNearbySearch(latlng, MyFirstPage.ErrandsList[i].Text);

				//Add to global results variable so that we can grab more locations from the list 
				//if a user rejects a location
				//Reject location functionality still in progress
				locationResults.Add(new ErrandResults(l, null));

				List<Coordinates> closestLocations = new List<Coordinates> ();

				if(l.results != null)
				{
					//send first 3 results of each errand to optimized route method
					int maxResults = 3;
					if (l.results.Count < 3) {
						maxResults = l.results.Count;
					}

					for(int j = 0; j < maxResults; j++)
					{
						closestLocations.Add(new Coordinates(
							l.results[j].geometry.location.lat,
							l.results[j].geometry.location.lng,
							l.results[j].name,
							l.results[j].vicinity));
					}
				}

				closestLocationsPerErrand.Add(closestLocations);
			}

			//Only hit up mapquest api for optimized route if there are 2 or more errands
			if(closestLocationsPerErrand.Count > 1){
				//Hit up mapquest api here to get optimized route:
				RouteService routeService = new RouteService();
				locations.AddRange(routeService.GetOptimizedRoute(origin, closestLocationsPerErrand));
			} else {
				locations.Add(new Coordinates(
					l.results[0].geometry.location.lat,
					l.results[0].geometry.location.lng,
					l.results[0].name,
					l.results[0].vicinity));
			}

			foreach (var location in locations) {
				_errandLocations.Add (new BasicMapAnnotation (new CLLocationCoordinate2D (location.Lat, location.Long), location.Title, location.Subtitle));
			}

			foreach (var location in _errandLocations) {
				map.AddAnnotation (location);
			}
		
			CreateRoute();

			var buttonRect = UIButton.FromType(UIButtonType.DetailDisclosure);
			buttonRect.Center = new PointF(View.Frame.Right - 20, View.Frame.Top + 20);
			View.AddSubview(buttonRect);
			buttonRect.TouchUpInside += _page.HandleTouchUpInside;

			this._loadPop.Hide ();
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

							CreateRoute();
						}
					});
			}

			if (_recenterMap)
			{
				// set map center and region
				CLLocationCoordinate2D mapCenter = new CLLocationCoordinate2D(newLocation.Coordinate.Latitude, newLocation.Coordinate.Longitude);
				MKCoordinateRegion mapRegion = new MKCoordinateRegion(mapCenter, new MKCoordinateSpan(.025, .025));
				//MKCoordinateRegion mapRegion = getRegionForAnnotations(_errandLocations, mapCenter);
				map.CenterCoordinate = mapCenter;
				map.Region = mapRegion;

				_recenterMap = false;
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
						//To do:
						//Separate directions to each location into "Groupings"
						//We are working with a ListView on Page 3
						//See: http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/listview/
						MyThirdPage thirdPage = _page.ThirdPage as MyThirdPage;
						thirdPage.Directions.Clear ();

						//if (_myRoutes != null)
						//    foreach (var route in _myRoutes)
						//        map.RemoveOverlay (route.Polyline);

						_myRoutes = response.Routes;

						//Add each Polyline from route to map as overlay
						foreach (var route in response.Routes) {
							map.AddOverlay (route.Polyline, MKOverlayLevel.AboveRoads);

							//thirdPage.GroupTitle = new Binding("Some Title");

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

			//Need to move region code to a new method:
			CLLocationCoordinate2D mapCenter = new CLLocationCoordinate2D(_errandLocations[0].Coordinate.Latitude, _errandLocations[0].Coordinate.Longitude);
			MKCoordinateRegion mapRegion = getRegionForAnnotations(_errandLocations, mapCenter);
			map.CenterCoordinate = mapCenter;
			map.Region = mapRegion;
		}

		private string GetLatLng(string address)
		{
			address = Uri.EscapeUriString(address);
			//DirectionsResponse response = new DirectionsResponse();

			string uri = "https://maps.googleapis.com/maps/api/geocode/json?address=%s" + address;

			WebClient webpage = new WebClient();
			string source = webpage.DownloadString(uri);

			//JToken t = JToken.Parse(source);
	
			return source;
		}
	

		//This method appears to still need some work to make the region large enough:
		// returns a MKCoordinateRegion that encompasses an array of MKAnnotations
		public MKCoordinateRegion getRegionForAnnotations(List<BasicMapAnnotation> annotations, CLLocationCoordinate2D center) 
        {

			double minLat = 90.0;
			double maxLat = -90.0;
			double minLon = 180.0;
			double maxLon = -180.0;

			foreach (BasicMapAnnotation annotation in annotations) {
                if (annotation.Coords.Latitude < minLat) {
					minLat = annotation.Coords.Latitude;
                }        
				if (annotation.Coords.Longitude < minLon) {
					minLon = annotation.Coords.Longitude;
                }        
				if (annotation.Coords.Latitude > maxLat) {
					maxLat = annotation.Coords.Latitude;
                }        
				if (annotation.Coords.Longitude > maxLon) {
					maxLon = annotation.Coords.Longitude;
                }
            }

            MKCoordinateSpan span = new MKCoordinateSpan(maxLat - minLat, maxLon - minLon);

            //CLLocationCoordinate2D center = new CLLocationCoordinate2D((maxLat - span.LatitudeDelta / 2), maxLon - span.LongitudeDelta / 2);

            return new MKCoordinateRegion(center, span);
        }

		public void DisplayLoadingState()
		{	
			// Determine the correct size to start the overlay (depending on device orientation)
			var bounds = UIScreen.MainScreen.Bounds; // portrait bounds
//			if (UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight) {
//				bounds.Size = new CGSize(bounds.Size.Height, bounds.Size.Width);
//			}
			// show the loading overlay on the UI thread using the correct orientation sizing
			this._loadPop = new LoadingOverlay (bounds);
			this.View.Add ( this._loadPop );
		}
	}
}



