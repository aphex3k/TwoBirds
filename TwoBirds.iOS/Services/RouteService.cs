using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TwoBirds.iOS
{
	public class RouteService
	{
		static double shortestDistance = 0.0;

		public List<Coordinates> GetOptimizedRoute (Coordinates origin, List<List<Coordinates>> errands)
		{
			List<Coordinates> errandA = new List<Coordinates> ();
			List<Coordinates> errandB = new List<Coordinates> ();
			List<Coordinates> errandC = new List<Coordinates> ();

			Coordinates locA = new Coordinates ();
			Coordinates locB = new Coordinates ();
			Coordinates locC = new Coordinates ();

			if (errands.Count >= 1) {
				errandA = errands [0];
			}
			if (errands.Count >= 2) {
				errandB = errands [1];
			}
			if (errands.Count >= 3) {
				errandC = errands [2];
			}

			List<Coordinates> locations = new List<Coordinates>();

			int bestRoute_errandA = 0;
			int bestRoute_errandB = 0;
			int bestRoute_errandC = 0;
			RouteTest routeTest = new RouteTest ();
			List<Coordinates> bestRouteCombo = new List<Coordinates>();
			List<int> bestRouteSequence = new List<int> ();
			List<Coordinates> bestRoute = new List<Coordinates> ();

			//errand_A (like "Target") list of 3 closest locations
			for (int i = 0; i < errandA.Count; i++)
			{
				locA = errandA[i];

				//errand_B, like "Target"
				for (int j = 0; j < errandB.Count; j++)
				{
					locB = errandB[j];

					if (errands.Count < 3) {
						locations.Add(origin);
						locations.Add(locA);
						locations.Add(locB);
						//will change to destination in further refactor
						locations.Add(origin);
						routeTest = TestRoute (locations);

						if (routeTest.IsBestRoute) {
							bestRoute_errandA = i;
							bestRoute_errandB = j;

							bestRouteSequence = routeTest.Sequence;
						}

						locations.Clear ();
					} else {
						//errand_C, like "CVS"
						for (int k = 0; k < errandC.Count; k++) {
							locC = errandC[k];

							locations.Add(origin);
							locations.Add(locA);
							locations.Add(locB);
							locations.Add(locC);
							//will change to destination in further refactor
							locations.Add(origin);

							routeTest = TestRoute (locations);

							if (routeTest.IsBestRoute) {
								bestRoute_errandA = i;
								bestRoute_errandB = j;
								bestRoute_errandC = k;

								bestRouteSequence = routeTest.Sequence;
							}

							locations.Clear ();
						}
					}
				}
			}

			bestRouteCombo.Add(errandA[bestRoute_errandA]);
			bestRouteCombo.Add(errandB[bestRoute_errandB]);
			if (errands.Count > 2) {
				bestRouteCombo.Add (errandC[bestRoute_errandC]);
			}

			//Remove the first and last int from bestRouteSequence
			//because that's the origin and destination
			bestRouteSequence.RemoveAt(0);
			bestRouteSequence.RemoveAt(bestRouteSequence.Count - 1);

			foreach(int n in bestRouteSequence)
			{
				for (int i = 0; i < bestRouteCombo.Count; i++) {
					int errandPosition = i + 1;
					if (n == errandPosition) {
						//Add to _errandLocations list in optimized order
						bestRoute.Add (bestRouteCombo[i]);
					}
				}
			}

			return bestRoute;
		}

		public RouteTest TestRoute(List<Coordinates> locations)
		{
			RouteTest routeTest = new RouteTest ();
			routeTest.IsBestRoute = false;
			string param = "";
			string distanceInMiles_string = "";
			double distanceInMiles = 0.0;
			//double totalDistance = 0.0;
			List<int> locationSequence = new List<int>();

			string uri = "http://open.mapquestapi.com/directions/v2/optimizedroute?key=Fmjtd%7Cluu829u8l1%2Cb2%3Do5-9w1wgy&json={locations:[";

			for (int idx = 0; idx < locations.Count; idx++) {
				if (idx == locations.Count - 1) {
					param += "{latLng:{lat:" + locations[idx].Lat + ",lng:" + locations[idx].Long + "}}]}";
				} else {
					param += "{latLng:{lat:" + locations[idx].Lat + ",lng:" + locations[idx].Long + "}},";
				}
			}
				
			uri = uri + param;
			WebClient webpage = new WebClient();
			string source = webpage.DownloadString(uri);
			JToken t = JToken.Parse(source);


			if (t["route"] != null && t["route"]["legs"] != null)
			{
				double totalMiles = 0.0;
				foreach (var leg in t["route"]["legs"])
				{
					if (leg != null && leg["distance"] != null)
					{
						distanceInMiles_string = leg["distance"].ToString();
						distanceInMiles = Double.Parse(distanceInMiles_string);

						totalMiles += distanceInMiles;
					}
				}

				if (totalMiles < shortestDistance || shortestDistance <= 0)
				{
					shortestDistance = totalMiles;
					routeTest.IsBestRoute = true;

					//Get location sequence
					string location_string;
					int location_int;

					if (t["route"] != null && t["route"]["locationSequence"] != null) {
						//locationSequence is zero indexed
						foreach (var locationInt in t["route"]["locationSequence"]) 
						{
							location_string = locationInt.ToString ();
							location_int = int.Parse (location_string);
							locationSequence.Add (location_int);
						}
					}
				}
			}

			routeTest.Sequence = locationSequence;

			return routeTest;
		}
	}
}

