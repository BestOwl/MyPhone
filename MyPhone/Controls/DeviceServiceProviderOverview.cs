using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GoodTimeStudio.MyPhone.Controls
{
    [TemplateVisualState(Name = StateProviderIdle, GroupName = GroupProviderStates)]
    [TemplateVisualState(Name = StateProviderConnecting, GroupName = GroupProviderStates)]
    [TemplateVisualState(Name = StateProviderRetryScheduled, GroupName = GroupProviderStates)]
    [TemplateVisualState(Name = StateProviderConnected, GroupName = GroupProviderStates)]
    [TemplateVisualState(Name = StateProviderStopped, GroupName = GroupProviderStates)]
    [TemplateVisualState(Name = StateRetryButtonVisible, GroupName = GroupRetryButtonVisibilityStates)]
    [TemplateVisualState(Name = StateRetryButtonCollapsed, GroupName = GroupRetryButtonVisibilityStates)]
    [TemplatePart(Name = PartRetryButton, Type = typeof(Button))]
    public sealed class DeviceServiceProviderOverview : Control
    {
        #region Constants
        private const string PartRetryButton = "PART_RetryButton";

        private const string GroupProviderStates = "ProviderStates";
        private const string StateProviderIdle = "ProviderIdle";
        private const string StateProviderConnecting = "ProviderConnecting";
        private const string StateProviderRetryScheduled = "ProviderRetryScheduled";
        private const string StateProviderConnected = "ProviderConnected";
        private const string StateProviderStopped = "ProviderStopped";

        private const string GroupRetryButtonVisibilityStates = "RetryButtonVisibilityStates";
        private const string StateRetryButtonVisible = "RetryButtonVisible";
        private const string StateRetryButtonCollapsed = "RetryButtonCollapsed";
        #endregion

        [DisallowNull]
        private Button? _part_retryButton;

        #region Properties

        public string ServiceName
        {
            get { return (string)GetValue(ServiceNameProperty); }
            set { SetValue(ServiceNameProperty, value); }
        }
        public static readonly DependencyProperty ServiceNameProperty =
            DependencyProperty.Register(
                nameof(ServiceName), 
                typeof(string), 
                typeof(DeviceServiceProviderOverview), 
                new PropertyMetadata(string.Empty));

        public string GlyphIcon
        {
            get { return (string)GetValue(GlyphIconProperty); }
            set { SetValue(GlyphIconProperty, value); }
        }
        public static readonly DependencyProperty GlyphIconProperty =
            DependencyProperty.Register(
                nameof(GlyphIcon), 
                typeof(string), 
                typeof(DeviceServiceProviderOverview), 
                new PropertyMetadata(string.Empty));

        public DeviceServiceProviderState ProviderState
        {
            get { return (DeviceServiceProviderState)GetValue(ProviderStateProperty); }
            set { SetValue(ProviderStateProperty, value); }
        }
        public static readonly DependencyProperty ProviderStateProperty =
            DependencyProperty.Register(
                nameof(ProviderState), 
                typeof(DeviceServiceProviderState), 
                typeof(DeviceServiceProviderOverview), 
                new PropertyMetadata(DeviceServiceProviderState.Stopped, new PropertyChangedCallback(OnProviderStateChanged)));
        private static void OnProviderStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DeviceServiceProviderOverview? overview = d as DeviceServiceProviderOverview;
            Debug.Assert(overview != null);

            DeviceServiceProviderState previousState = (DeviceServiceProviderState)e.OldValue;
            DeviceServiceProviderState newState = (DeviceServiceProviderState)e.NewValue;

            if (previousState != newState)
            {
                overview.UpdateIndicatorVisualState();
                overview.UpdateRetryButtonVisualState();
            }
        }

        public string? StatusMessage
        {
            get { return (string)GetValue(StatusMessageProperty); }
            set { SetValue(StatusMessageProperty, value); }
        }
        public static readonly DependencyProperty StatusMessageProperty =
            DependencyProperty.Register(
                nameof(StatusMessage), 
                typeof(string), 
                typeof(DeviceServiceProviderOverview), 
                new PropertyMetadata(null));

        public event RoutedEventHandler RetryClicked
        {
            add { _part_retryButton!.Click += value; }
            remove { _part_retryButton!.Click -= value; }
        }

        public ICommand? RetryCommand
        {
            get { return (ICommand)GetValue(RetryCommandProperty); }
            set { SetValue(RetryCommandProperty, value); }
        }
        public static readonly DependencyProperty RetryCommandProperty =
            DependencyProperty.Register(
                nameof(RetryCommand),
                typeof(ICommand),
                typeof(DeviceServiceProviderOverview),
                new PropertyMetadata(null));
        #endregion

        public DeviceServiceProviderOverview()
        {
            DefaultStyleKey = typeof(DeviceServiceProviderOverview);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button? retryBtn = GetTemplateChild(PartRetryButton) as Button;
            if (retryBtn == null)
            {
                throw new KeyNotFoundException($"Template part {PartRetryButton} not available");
            }
            _part_retryButton = retryBtn;

            UpdateIndicatorVisualState();
            UpdateRetryButtonVisualState();
        }

        private void UpdateIndicatorVisualState()
        {
            switch (ProviderState)
            {
                case DeviceServiceProviderState.Idle:
                    VisualStateManager.GoToState(this, StateProviderIdle, true);
                    break;
                case DeviceServiceProviderState.Connecting:
                    VisualStateManager.GoToState(this, StateProviderConnecting, true);
                    break;
                case DeviceServiceProviderState.RetryScheduled:
                    VisualStateManager.GoToState(this, StateProviderRetryScheduled, true);
                    break;
                case DeviceServiceProviderState.Stopped:
                    VisualStateManager.GoToState(this, StateProviderStopped, true);
                    break;
                case DeviceServiceProviderState.Connected:
                    VisualStateManager.GoToState(this, StateProviderConnected, true);
                    break;
            }
        }

        private void UpdateRetryButtonVisualState()
        {
            if (ProviderState == DeviceServiceProviderState.Stopped)
            {
                VisualStateManager.GoToState(this, StateRetryButtonVisible, true);
            }
            else
            {
                VisualStateManager.GoToState(this, StateRetryButtonCollapsed, true);
            }
        }
    }
}
