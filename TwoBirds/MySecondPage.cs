using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace TwoBirds
{
	/// <summary>
	/// This Xamarin.Forms page will actually be rendered natively
	/// on iOS and Android. There is a Heading property that is set
	/// here which will be accessible when rendering natively.
	/// </summary>
	public class MySecondPage : ContentPage
	{
		public String Heading;
		public Page ThirdPage;

		public MySecondPage ()
		{
			Title = "Map";

			Heading = "This is the map page";

			// rendering of this page is done natively on each platform

			ThirdPage = (Page)Activator.CreateInstance(typeof(MyThirdPage));
		}

		public void HandleTouchUpInside(object sender, EventArgs ea)
		{
			this.Navigation.PushAsync(ThirdPage);
		}
	}
}
