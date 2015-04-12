using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace TwoBirds
{
	/// <summary>
	/// This Xamarin.Forms page will actually be rendered natively
	/// on iOS using a custom UIViewController
	/// </summary>
	public class MyThirdPage : ContentPage
	{
		public String Heading;

		public List<String> Directions = new List<String>();

		//public Binding GroupTitle;

		public ListView MyListView;

		public MyThirdPage ()
		{
			Title = "Directions";

			Heading = "This is the directions page";

			MyListView = new ListView 
			{
				ItemsSource = Directions.ToArray()
				//, IsGroupingEnabled = true
				//, GroupDisplayBinding = GroupTitle //new Binding("LongTitle")
			};
			
			Content = MyListView;
		}
	}
}
