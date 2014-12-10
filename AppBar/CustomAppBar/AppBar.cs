using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Yolo
{
    [TemplatePart(Name = FadeInPropertyName, Type = typeof(Storyboard))]
    [TemplatePart(Name = FadeOutPropertyName, Type = typeof(Storyboard))]
    [TemplatePart(Name = PrimaryCommandsName, Type = typeof(ListView))]
    [TemplatePart(Name = SecondaryCommandsName, Type = typeof(ListView))]
    [TemplatePart(Name = IsOpenPropertyName, Type = typeof(bool))]
    public sealed class AppBar : Control
    {
        private const string FadeInPropertyName = "FadeInProperty";
        private const string FadeOutPropertyName = "FadeOutProperty";
        private const string PrimaryCommandsName = "PrimaryCommandsProperty";
        private const string SecondaryCommandsName = "SecondaryCommandsProperty";
        private const string IsOpenPropertyName = "IsOpenProperty";
        private const string ToggleAppBarButtonName = "ToggleAppBarButton";
        private const string EllipseLessAppBarButtonStyleName = "EllipseLessAppBarButtonStyle";
        private const string MenuAppBarButtonStyleName = "MenuAppBarButtonStyle";
        private const string CompositeTransformName = "CompositeTransform";

        private Storyboard _fadeInProperty;
        private Storyboard _fadeOutProperty;
        private ListView _primaryCommands;
        private ListView _secondaryCommands;
        private Button _toggleAppBarButton;
        private bool _isOpen;
        private static Style _ellipseLessAppBarButtonStyle;
        private static Style _menuAppBarButtonStyle;
        private static PlaneProjection _compositeTransform;

        #region AppBarPrimaryCommands Property
        public IList<ICommandBarElement> PrimaryCommands
        {
            get { return (IList<ICommandBarElement>)GetValue(PrimaryCommandsProperty); }
            set { SetValue(PrimaryCommandsProperty, UpdateMainButtonStyles(value)); }
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
                SetValue(SecondaryCommandsProperty, UpdateMenuButtonStyles(value));
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
                that._secondaryCommands.ItemsSource = UpdateMenuButtonStyles(secondComm);
            }
        }

        static IList<AppBarButton> UpdateMenuButtonStyles(IList<AppBarButton> menucommands)
        {
            if (_menuAppBarButtonStyle == null) return menucommands;
            foreach (var appBarButton in menucommands)
            {
                appBarButton.Style = _menuAppBarButtonStyle;
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
            _fadeInProperty = GetTemplateChild(FadeInPropertyName) as Storyboard;
            _fadeOutProperty = GetTemplateChild(FadeOutPropertyName) as Storyboard;
            _toggleAppBarButton = GetTemplateChild(ToggleAppBarButtonName) as Button;
            var rowHeight = (GetTemplateChild("MenuRowDefinition") as RowDefinition).Height;
            (GetTemplateChild("FadeOutHeightProperty") as EasingDoubleKeyFrame).Value = rowHeight.Value;
            _ellipseLessAppBarButtonStyle = GetTemplateChild(EllipseLessAppBarButtonStyleName) as Style;
            _menuAppBarButtonStyle = GetTemplateChild(MenuAppBarButtonStyleName) as Style;
            SecondaryCommands = UpdateMenuButtonStyles(SecondaryCommands);
            PrimaryCommands = UpdateMainButtonStyles(PrimaryCommands);
            _compositeTransform = GetTemplateChild(CompositeTransformName) as PlaneProjection;
            _toggleAppBarButton.Loaded += (sender, args) =>
            {
                _toggleAppBarButton.Tapped += ToggleAppBarButtonOnTap;
                _toggleAppBarButton.ManipulationMode = ManipulationModes.TranslateY;
                _toggleAppBarButton.ManipulationDelta += ToggleAppBarButtonOnManipulationDelta;
                _toggleAppBarButton.ManipulationCompleted += ToggleAppBarButtonOnManipulationCompleted;
            };
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
                if (_compositeTransform.GlobalOffsetY > -1 && _compositeTransform.GlobalOffsetY < 351)
                {
                    _compositeTransform.GlobalOffsetY += manipulationDeltaRoutedEventArgs.Delta.Translation.Y;
                }
                if (_compositeTransform.GlobalOffsetY < 0)
                    _compositeTransform.GlobalOffsetY = 0;
                else if (_compositeTransform.GlobalOffsetY > 350)
                    _compositeTransform.GlobalOffsetY = 350;
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
            if (_fadeInProperty != null)
            {
                _fadeInProperty.Begin();
                _isOpen = true;
            }

        }

        public void Hide()
        {
            if (_fadeOutProperty != null)
            {
                _fadeOutProperty.Begin();
                _isOpen = false;
            }
        }
    }
}
