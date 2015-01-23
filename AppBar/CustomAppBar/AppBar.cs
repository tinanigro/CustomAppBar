using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace CustomAppBar
{
    public delegate void HomeButtonTapped(Button button, EventArgs e);

    [TemplatePart(Name = FadeInPropertyName, Type = typeof(Storyboard))]
    [TemplatePart(Name = FadeOutPropertyName, Type = typeof(Storyboard))]
    [TemplatePart(Name = PrimaryCommandsName, Type = typeof(ListView))]
    [TemplatePart(Name = SecondaryCommandsName, Type = typeof(ListView))]
    [TemplatePart(Name = IsOpenPropertyName, Type = typeof(bool))]
    public sealed class AppBar : Control
    {
        // Events
        public event HomeButtonTapped HomeButtonClicked;

        // Storyboards
        private const string FadeInPropertyName = "FadeInProperty";
        private const string FadeOutPropertyName = "FadeOutProperty";


        private const string PrimaryCommandsName = "PrimaryCommandsProperty";
        private const string SecondaryCommandsName = "SecondaryCommandsProperty";
        private const string IsOpenPropertyName = "IsOpenProperty";
        private const string ToggleAppBarButtonName = "ToggleAppBarButton";
        private const string HomeAppBarButtonName = "HomeAppBarButton";
        private const string EllipseLessAppBarButtonStyleName = "EllipseLessAppBarButtonStyle";
        private const string MenuAppBarButtonStyleName = "MenuAppBarButtonStyle";
        private const string CompositeTransformName = "CompositeTransform";
        private const string TapRowDefinitionName = "TapRowDefinition";
        private const string MainRegionRowDefinitionName = "MainRegionRowDefinition";
        private const string TapGridName = "TapGrid";
        private const string DotsTextBlockName = "DotsTextBlock";

        // Storyboard
        private Storyboard _fadeInProperty;
        private Storyboard _fadeOutProperty;

        private ListView _primaryCommands;
        private ListView _secondaryCommands;
        private Button _toggleAppBarButton;
        private Button _homeAppBarButton;
        private bool _isOpen;
        private static Style _ellipseLessAppBarButtonStyle;
        private static Style _menuAppBarButtonStyle;
        private PlaneProjection _compositeTransform;
        private RowDefinition _tapRowDefinition;
        private RowDefinition _mainRegionRowDefinition;
        private Grid _tapGrid;
        private TextBlock _dotsTextBlock;
        private Grid _primaryGrid;

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
                        if (_mainRegionRowDefinition != null)
                            _mainRegionRowDefinition.Height = new GridLength(60, GridUnitType.Pixel);
                        if (_homeAppBarButton != null) 
                            _homeAppBarButton.Opacity = 1;
                    }
                    else
                    {
                        if (_mainRegionRowDefinition != null)
                            _mainRegionRowDefinition.Height = new GridLength(20, GridUnitType.Pixel);
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
                    (appBarButton as AppBarButton).Foreground= new SolidColorBrush(Colors.White);
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
            //Storyboards
            _fadeInProperty = GetTemplateChild(FadeInPropertyName) as Storyboard;
            _fadeOutProperty = GetTemplateChild(FadeOutPropertyName) as Storyboard;


            _toggleAppBarButton = GetTemplateChild(ToggleAppBarButtonName) as Button;
            _homeAppBarButton = GetTemplateChild(HomeAppBarButtonName) as Button;

            var rowHeight = (GetTemplateChild("MenuRowDefinition") as RowDefinition).Height;
            (GetTemplateChild("FadeOutHeightProperty") as EasingDoubleKeyFrame).Value = rowHeight.Value;
            _ellipseLessAppBarButtonStyle = GetTemplateChild(EllipseLessAppBarButtonStyleName) as Style;
            _menuAppBarButtonStyle = GetTemplateChild(MenuAppBarButtonStyleName) as Style;
            SecondaryCommands = UpdateMenuButtonStyles(SecondaryCommands, this);
            PrimaryCommands = UpdateMainButtonStyles(PrimaryCommands);
            _compositeTransform = GetTemplateChild(CompositeTransformName) as PlaneProjection;
            _tapRowDefinition = GetTemplateChild(TapRowDefinitionName) as RowDefinition;
            _mainRegionRowDefinition = GetTemplateChild(MainRegionRowDefinitionName) as RowDefinition;
            _tapGrid = GetTemplateChild(TapGridName) as Grid;
            _dotsTextBlock = GetTemplateChild(DotsTextBlockName) as TextBlock;

            _toggleAppBarButton.Loaded += (sender, args) =>
            {
                _toggleAppBarButton.Tapped += ToggleAppBarButtonOnTap;
                _toggleAppBarButton.ManipulationMode = ManipulationModes.TranslateY;
                _toggleAppBarButton.ManipulationDelta += ToggleAppBarButtonOnManipulationDelta;
                _toggleAppBarButton.ManipulationCompleted += ToggleAppBarButtonOnManipulationCompleted;
            };
            _tapGrid.Tapped += ToggleAppBarButtonOnTap;
            HardwareButtons.BackPressed += (sender, args) =>
            {
                if (_isOpen)
                {
                    args.Handled = true;
                    Hide();
                }
            };

            _homeAppBarButton.Loaded += (sender, args) =>
            {
                _homeAppBarButton.Click += HomeAppBarButtonOnClick;
            };
        }

        private void HomeAppBarButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (HomeButtonClicked != null && (PrimaryCommands.Any() || _isOpen))
            {
                HomeButtonClicked(sender as Button, new EventArgs());
                Hide();
            }
        }

        private void ToggleAppBarButtonOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs manipulationCompletedRoutedEventArgs)
        {
            if (manipulationCompletedRoutedEventArgs.Velocities.Linear.Y < 0)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void ToggleAppBarButtonOnManipulationDelta(object sender,
            ManipulationDeltaRoutedEventArgs manipulationDeltaRoutedEventArgs)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (_compositeTransform.GlobalOffsetY > -1 && _compositeTransform.GlobalOffsetY < 151)
                {
                    _compositeTransform.GlobalOffsetY += manipulationDeltaRoutedEventArgs.Delta.Translation.Y;
                }
                if (_compositeTransform.GlobalOffsetY < 0)
                    _compositeTransform.GlobalOffsetY = 0;
                else if (_compositeTransform.GlobalOffsetY > 150)
                    _compositeTransform.GlobalOffsetY = 150;
            });
            Debug.WriteLine(manipulationDeltaRoutedEventArgs.Delta.Translation.Y + " control : " + _compositeTransform.GlobalOffsetY);
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
            if (_fadeInProperty != null)
            {
                _mainRegionRowDefinition.Height = new GridLength(60, GridUnitType.Pixel);
                _tapRowDefinition.Height = new GridLength(height - 210, GridUnitType.Pixel);
                _fadeInProperty.Begin();
                _isOpen = true;
            }
            if (_homeAppBarButton != null)
            {
                _homeAppBarButton.Opacity = 1;
            }
        }

        public void Hide()
        {
            if (_fadeOutProperty != null)
            {
                _mainRegionRowDefinition.Height = PrimaryCommands.Any() ? new GridLength(60, GridUnitType.Pixel) : new GridLength(20, GridUnitType.Pixel);
                _fadeOutProperty.Begin();
                _isOpen = false;
                _tapRowDefinition.Height = new GridLength(0, GridUnitType.Pixel);
            }
            if (_homeAppBarButton != null)
            {
                _homeAppBarButton.Opacity = PrimaryCommands.Any() ? 1 : 0;
            }
        }

    }
}

