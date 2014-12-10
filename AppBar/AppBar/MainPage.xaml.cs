using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=391641

namespace AppBar
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool isOut = false;
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoqué lorsque cette page est sur le point d'être affichée dans un frame.
        /// </summary>
        /// <param name="e">Données d’événement décrivant la manière dont l’utilisateur a accédé à cette page.
        /// Ce paramètre est généralement utilisé pour configurer la page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //var yolo = new ItemCollection();
            Test.PrimaryCommands.Add(new AppBarButton()
            {
                Label = "test",
            });
            //yolo.Add(new AppBarButton()
            //{
            //    Label = "test",
            //    Icon=new SymbolIcon(Symbol.Accept)
            //});
            //Test.PrimaryCommands = yolo;

            //var swag = new ObservableCollection<AppBarButton>();
            //swag.Add(new AppBarButton()
            //{
            //  Label = "some swag in here"
            //});
            //Test.SecondaryCommands = swag;
        }

        void ButtonBase_OnClick(object sender, RoutedEventArgs args)
        {
        }
    }
}
