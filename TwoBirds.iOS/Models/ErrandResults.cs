using System;
using System.Collections.Generic;

namespace TwoBirds.iOS
{
	public class ErrandResults
	{
		public NearbySearch LocationSearchResults { get; set;}

		public List<int> Rejects { get; set; }

		public ErrandResults(NearbySearch searchResults, List<int> rejects)
		{
			this.LocationSearchResults = searchResults;
			this.Rejects = rejects;
		}
	}
}

