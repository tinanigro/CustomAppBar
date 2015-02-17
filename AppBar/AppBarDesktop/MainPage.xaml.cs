using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace AppBarDesktop
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (Test.PrimaryCommands.Any())
            {
                var tt = new List<ICommandBarElement>();
                Test.PrimaryCommands = tt;
            }
            else
            {
                var v = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                var allProperties = v.GetType().GetRuntimeProperties();
                var titleBar = allProperties.FirstOrDefault(x => x.Name == "TitleBar");
                if (titleBar != null)
                {
                    dynamic bb = titleBar.GetMethod.Invoke(v, null);
                    bb.ButtonBackgroundColor = ((SolidColorBrush)App.Current.Resources["ApplicationPageBackgroundThemeBrush"]).Color;
                    bb.ButtonForegroundColor = Colors.RoyalBlue;
                    bb.ButtonHoverBackgroundColor = Colors.RoyalBlue;
                    bb.ButtonHoverForegroundColor = Colors.WhiteSmoke;
                    bb.ButtonPressedBackgroundColor = Colors.DodgerBlue;
                    Debug.WriteLine(titleBar.Name);
                    bb.ExtendViewIntoTitleBar = true;
                }

                var tt = new List<ICommandBarElement>();
                tt.Add(new AppBarButton()
                {
                    Label = "test",
                    Icon = new SymbolIcon(Symbol.Accept)
                });
                tt.Add(new AppBarButton()
                {
                    Label = "test",
                    Icon = new SymbolIcon(Symbol.NewFolder)
                });
                tt.Add(new AppBarButton()
                {
                    Label = "test",
                    Icon = new SymbolIcon(Symbol.BackToWindow)
                });
                tt.Add(new AppBarButton()
                {
                    Label = "test",
                    Icon = new SymbolIcon(Symbol.Remote)
                });
                Test.PrimaryCommands = tt;
                Test.SecondaryCommands = new List<AppBarButton>()
                {
                    new AppBarButton()
                    {
                        Label = "coucou test",
                    },
                    new AppBarButton()
                    {
                        Label = "coucou test",
                    },
                    new AppBarButton()
                    {
                        Label = "coucou test",
                    }
                };
            }
            Test.HomeButtonVisible = true;
        }
    }
}
