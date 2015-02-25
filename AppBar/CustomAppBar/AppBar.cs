using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace CustomAppBarDesktop
{
    public delegate void HomeButtonTapped(Button button, EventArgs e);

    [TemplatePart(Name = PrimaryCommandsName, Type = typeof(ListView))]
    [TemplatePart(Name = SecondaryCommandsName, Type = typeof(ListView))]
    [TemplatePart(Name = IsOpenPropertyName, Type = typeof(bool))]
    [TemplatePart(Name = ExtraLeftContentName, Type = typeof(object))]
    public sealed class AppBar : Control
    {
        // Events
        public event HomeButtonTapped HomeButtonClicked;

        private const string AppBarName = "AppBar";
        private const string PrimaryCommandsName = "PrimaryCommandsProperty";
        private const string SecondaryCommandsName = "SecondaryCommandsProperty";
        private const string IsOpenPropertyName = "IsOpenProperty";
        private const string ToggleAppBarButtonName = "ToggleAppBarButton";
        private const string HomeAppBarButtonName = "HomeAppBarButton";
        private const string EllipseLessAppBarButtonStyleName = "EllipseLessAppBarButtonStyle";
        private const string MenuAppBarButtonStyleName = "MenuAppBarButtonStyle";
        private const string DotsTextBlockName = "DotsTextBlock";
        private const string ExtraLeftContentName = "ExtraLeftContent";

        private Grid _appBar;
        private ListView _primaryCommands;
        private ListView _secondaryCommands;
        private Button _toggleAppBarButton;
        private Button _homeAppBarButton;
        private bool _isOpen;
        private static Style _ellipseLessAppBarButtonStyle;
        private static Style _menuAppBarButtonStyle;
        private TextBlock _dotsTextBlock;
        private Grid _primaryGrid;
        private ContentControl _extraLeftContent;
        private Flyout _menuFlyout;

        public object ExtraLeftContent
        {
            get
            { return (object)GetValue(ExtraLeftContentProperty); }
            set { SetValue(ExtraLeftContentProperty, value); }
        }

        public static readonly DependencyProperty ExtraLeftContentProperty =
            DependencyProperty.Register(ExtraLeftContentName, typeof(object), typeof(AppBar),
                new PropertyMetadata(null, ExtraLeftContentPropertyChangedCallback));

        private static void ExtraLeftContentPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (AppBar)dependencyObject;
            if (that._extraLeftContent != null)
            {
                that._extraLeftContent.Content = (object)dependencyPropertyChangedEventArgs.NewValue;
                that.ExtraLeftContent = (object)dependencyPropertyChangedEventArgs.NewValue;
            }
        }

        public bool HomeButtonVisible
        {
            get { return (bool)GetValue(HomeButtonVisibleProperty); }
            set { SetValue(HomeButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty HomeButtonVisibleProperty =
            DependencyProperty.Register("HomeButtonVisible", typeof(bool), typeof(AppBar),
                new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (AppBar)dependencyObject;
            if (that._homeAppBarButton != null)
            {
                if ((bool)dependencyPropertyChangedEventArgs.NewValue)
                {
                    that._homeAppBarButton.Visibility = Visibility.Visible;
                }
                else
                {
                    that._homeAppBarButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        #region AppBarPrimaryCommands Property
        public IList<ICommandBarElement> PrimaryCommands
        {
            get { return (IList<ICommandBarElement>)GetValue(PrimaryCommandsProperty); }
            set
            {
                SetValue(PrimaryCommandsProperty, UpdateMainButtonStyles(value));
                if (!_isOpen)
                {
                    if (PrimaryCommands.Any())
                    {
                        if (_homeAppBarButton != null)
                            _homeAppBarButton.Opacity = 1;
                    }
                    else
                    {
                        if (_homeAppBarButton != null)
                            _homeAppBarButton.Opacity = 0;
                    }
                }
            }
        }


        public static readonly DependencyProperty PrimaryCommandsProperty = DependencyProperty.Register(
            "PrimaryCommands", typeof(IList<ICommandBarElement>), typeof(AppBar), new PropertyMetadata(default(IList<ICommandBarElement>), PrimaryCommandsPropertyChangedCallback));


        private static void PrimaryCommandsPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (AppBar)dependencyObject;
            if (that._primaryCommands != null)
            {
                that._primaryCommands.ItemsSource = UpdateMainButtonStyles(dependencyPropertyChangedEventArgs.NewValue as IList<ICommandBarElement>);
            }
        }

        static IList<ICommandBarElement> UpdateMainButtonStyles(IList<ICommandBarElement> menucommands)
        {
            if (_ellipseLessAppBarButtonStyle == null) return menucommands;
            foreach (var appBarButton in menucommands)
            {
                if (appBarButton is AppBarButton)
                {
                    (appBarButton as AppBarButton).Style = _ellipseLessAppBarButtonStyle;
                    (appBarButton as AppBarButton).Foreground = new SolidColorBrush(Colors.White);
                }
            }
            while (menucommands.Count > 5)
            {
                menucommands.Remove(menucommands.Last());
            }
            return menucommands;
        }
        #endregion

        #region AppBarSecondaryCommands Property
        public IList<AppBarButton> SecondaryCommands
        {
            get { return (IList<AppBarButton>)GetValue(SecondaryCommandsProperty); }
            set
            {
                SetValue(SecondaryCommandsProperty, UpdateMenuButtonStyles(value, this));
                if (_toggleAppBarButton == null) return;
                if (value.Any())
                    _toggleAppBarButton.Visibility = Visibility.Visible;
                else
                    _toggleAppBarButton.Visibility = Visibility.Collapsed;
            }
        }


        public static readonly DependencyProperty SecondaryCommandsProperty = DependencyProperty.Register(
            "SecondaryCommands", typeof(IList<AppBarButton>), typeof(AppBar), new PropertyMetadata(default(IList<AppBarButton>), SecondaryCommandsPropertyChangedCallback));


        private static void SecondaryCommandsPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (AppBar)dependencyObject;
            if (that._secondaryCommands != null)
            {
                var secondComm = dependencyPropertyChangedEventArgs.NewValue as IList<AppBarButton>;
                that._secondaryCommands.ItemsSource = UpdateMenuButtonStyles(secondComm, that);
            }
        }

        static IList<AppBarButton> UpdateMenuButtonStyles(IList<AppBarButton> menucommands, AppBar appBar)
        {
            if (_menuAppBarButtonStyle == null) return menucommands;
            foreach (var appBarButton in menucommands)
            {
                appBarButton.Style = _menuAppBarButtonStyle;
                appBarButton.Foreground = appBar.Background;
                appBarButton.Click += (sender, args) => appBar.Hide();
            }
            return menucommands;
        }
        #endregion

        public AppBar()
        {
            DefaultStyleKey = typeof(AppBar);
            PrimaryCommands = new List<ICommandBarElement>();
            SecondaryCommands = new List<AppBarButton>();
            if (!DesignMode.DesignModeEnabled)
            {

            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _appBar = GetTemplateChild(AppBarName) as Grid;
            _toggleAppBarButton = GetTemplateChild(ToggleAppBarButtonName) as Button;
            _homeAppBarButton = GetTemplateChild(HomeAppBarButtonName) as Button;

            _ellipseLessAppBarButtonStyle = GetTemplateChild(EllipseLessAppBarButtonStyleName) as Style;
            _menuAppBarButtonStyle = GetTemplateChild(MenuAppBarButtonStyleName) as Style;
            SecondaryCommands = UpdateMenuButtonStyles(SecondaryCommands, this);
            PrimaryCommands = UpdateMainButtonStyles(PrimaryCommands);
            _dotsTextBlock = GetTemplateChild(DotsTextBlockName) as TextBlock;
            _extraLeftContent = (GetTemplateChild(ExtraLeftContentName) as ContentControl);


            if (_appBar != null)
                _appBar.Loaded += (sender, args) =>
                {
#if WINDOWS_APP
                _appBar.Height = 50;
#else
                    _appBar.Height = 60;
#endif
                };
            _toggleAppBarButton.Loaded += (sender, args) =>
            {
                _toggleAppBarButton.Tapped += ToggleAppBarButtonOnTap;
            };
            _homeAppBarButton.Loaded += (sender, args) =>
            {
                _homeAppBarButton.Click += HomeAppBarButtonOnClick;
            };
            _extraLeftContent.Content = ExtraLeftContent;
            _menuFlyout = (GetTemplateChild("MenuFlyout") as Flyout);
            if (_menuFlyout != null)
            {
                var grid = _menuFlyout.Content as Grid;
                grid.Tapped += Grid_Tapped;
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _menuFlyout.Hide();
        }

        private void HomeAppBarButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (HomeButtonClicked != null && (PrimaryCommands.Any() || _isOpen))
            {
                HomeButtonClicked(sender as Button, new EventArgs());
                Hide();
            }
        }

        private void ToggleAppBarButtonOnTap(object sender, RoutedEventArgs routedEventArgs)
        {
            ShowOrHide();
        }

        public void ShowOrHide()
        {
            if (_isOpen)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void Show()
        {
            var height = Window.Current.Bounds.Height;
            _isOpen = true;
            if (_homeAppBarButton != null)
            {
                _homeAppBarButton.Opacity = 1;
            }
        }

        public void Hide()
        {
            _isOpen = false;
            if (_homeAppBarButton != null)
            {
                _homeAppBarButton.Opacity = PrimaryCommands.Any() ? 1 : 0;
            }
            if(_menuFlyout != null)
                _menuFlyout.Hide();
        }
    }
}

