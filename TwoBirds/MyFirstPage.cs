using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TwoBirds
{
	/// <summary>
	/// This is a Xamarin.Forms screen - the first one shown in the app
	/// </summary>
	public class MyFirstPage : ContentPage
	{
	    private TableView _tableView;
	    private Button _myButton;
		public static EntryCell MyLocation;
		public static List<EntryCell> ErrandsList = new List<EntryCell>();
		private bool _addButton = true;
		private Page _page;

        public MyFirstPage()
        {
            Label header = new Label
            {
                Text = "Two Birds",
				FontAttributes = FontAttributes.Bold,
				FontSize = 50,
                HorizontalOptions = LayoutOptions.Center
            };

		    _myButton = new Button
			{
				Text = "+",
				Font = Font.SystemFontOfSize(NamedSize.Large)
			};
	        _myButton.Clicked += OnMyButtonClicked;

	        ViewCell myViewCell = new ViewCell
	        {
		        View = _myButton
	        };

	        MyLocation = new EntryCell
	        {
		        Placeholder = "Current Location"
	        };

	        for (int i = 0; i < 3; i++)
	        {
		        EntryCell cell = new EntryCell {Placeholder = "Type text here"};
		        ErrandsList.Add(cell);
	        }

	        ErrandsList[0].Text = "1323 Highland Ave, Hollywood, CA 90028";

	        _tableView = new TableView
            {
                Intent = TableIntent.Form,
                Root = new TableRoot()
                {
					new TableSection("My Location")
					{
						MyLocation
					},
                    new TableSection("Errand 1")
                    {
						ErrandsList[0]
					},	
                    new TableSection("Errand 2")
                    {
						ErrandsList[1],
						myViewCell
					}
                }
            };


            Button button = new Button
            {
                Text = "Map It!",
                Font = Font.SystemFontOfSize(NamedSize.Large),
                BorderWidth = 1,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            button.Clicked += OnButtonClicked;

            // Build the page.
            this.Title = "Two Birds";

            if (Device.OS == TargetPlatform.WinPhone)
            {
                this.Content = new StackLayout
                {
					Children = 
                    {
                        header,
                        _tableView,
                        button
                    }
				};
            }
            else
            {
                this.Content = new StackLayout
                {
                    Children = 
                    {
                        _tableView,
                        button
                    }
                };
            }
			_page = (Page)Activator.CreateInstance(typeof(MySecondPage));
		}

	    void OnMyButtonClicked(object sender, EventArgs e)
	    {
		    if (_addButton)
		    {
				_tableView.Root.Add(
					new TableSection("Errand 3")
                    {
						ErrandsList[2]
                    }
				);
				_myButton.Text = "-";
				_addButton = false;
		    }
		    else
		    {
			    _tableView.Root.RemoveAt(3);
				_myButton.Text = "+";
				_addButton = true;
		    }
	    }

        void OnButtonClicked(object sender, EventArgs e)
        {
			//Page page = (Page)Activator.CreateInstance(typeof(MapViewController));
			this.Navigation.PushAsync(_page);
        }
	}
}
	