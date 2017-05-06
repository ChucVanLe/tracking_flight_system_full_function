﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
//
//*********************************************************

using SDKTemplate;
using SecondaryViewsHelpers;
using System;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Core.AnimationMetrics;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using System.Linq;

namespace SDKTemplate
{
    // This page is shown in secondary views created by this app.
    // See Scenario 1 for details on how to create a secondary view
    public sealed partial class SecondaryViewPage : Page
    {
        const int ANIMATION_TRANSLATION_START = 100;
        const int ANIMATION_TRANSLATION_END = 0;
        const int ANIMATION_OPACITY_START = 0;
        const int ANIMATION_OPACITY_END = 1;

        const string EMPTY_TITLE = "<title cleared>";

        ViewLifetimeControl thisViewControl;
        int mainViewId;
        CoreDispatcher mainDispatcher;
        MainPage rootPage = MainPage.Current;
        //list com
        private ObservableCollection<DeviceInformation> listOfDevices;
        public SecondaryViewPage()
        {
            this.InitializeComponent();
            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
            //Window.Current.Bounds.Width = 100;
            //Application.Current. = 420;
            //Application.Current.MainWindow.Height = 420;
            //Application.Current.MainWindow = this;
            //ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(200, 200));
            ApplicationView.PreferredLaunchViewSize = new Size(600, 300);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            //Window.Current.Activate();
            //add baud rate
            //add baund rate to cb_bateRate
            cb_BaudRate.Items.Insert(0, "4800");
            cb_BaudRate.Items.Insert(1, "9600");
            cb_BaudRate.Items.Insert(2, "19200");
            cb_BaudRate.Items.Insert(3, "38400");
            cb_BaudRate.Items.Insert(4, "57600");
            cb_BaudRate.Items.Insert(5, "115200");
            cb_BaudRate.Items.Insert(6, "460800");

            Window.Current.CoreWindow.VisibilityChanged += CoreWindow_VisibilityChanged;
            //media 
            setup_media();
        }
        public async void setup_media()
        {
            //folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            //file = await folder.GetFileAsync("LacTroi-SonTungMTP.mp3");
            ////file = await folder.GetFileAsync("LẠC TRÔI - OFFICIAL MUSIC VIDEO - SƠN TÙNG M-TP.mp4");
            //var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            //media.SetSource(stream, file.ContentType);
        }

        void CoreWindow_VisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            if (!args.Visible)
            {
                // Action here
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            thisViewControl = (ViewLifetimeControl)e.Parameter;
            mainViewId = ((App)App.Current).MainViewId;
            mainDispatcher = ((App)App.Current).MainDispatcher;

            // When this view is finally release, clean up state
            thisViewControl.Released += ViewLifetimeControl_Released;
        }

        MediaElement media = new MediaElement();
        Windows.Storage.StorageFolder folder;
        Windows.Storage.StorageFile file;
        private async void GoToMain_Click(object sender, RoutedEventArgs e)
        {
            // Switch to the main view without explicitly requesting
            // that this view be hidden

            //-------------------------------------
            thisViewControl.StartViewInUse();
            await ApplicationViewSwitcher.SwitchAsync(mainViewId);
            thisViewControl.StopViewInUse();
            //----test------------------


        }

        private async void HideView_Click(object sender, RoutedEventArgs e)
        {
            // Switch to main and hide this view entirely from the user
            thisViewControl.StartViewInUse();
            await ApplicationViewSwitcher.SwitchAsync(mainViewId,
                ApplicationView.GetForCurrentView().Id,
                ApplicationViewSwitchingOptions.ConsolidateViews);
            thisViewControl.StopViewInUse();
        }

        private void ClearTitle_Click(object sender, RoutedEventArgs e)
        {
            // Clear the title by setting it to blank
            SetTitle("");
            //TitleBox.Text = "";
        }

        private async void SetTitle(string newTitle)
        {
            var thisViewId = ApplicationView.GetForCurrentView().Id;
            ApplicationView.GetForCurrentView().Title = newTitle;
            thisViewControl.StartViewInUse();

            // The title contained in the ViewLifetimeControl object is bound to
            // UI elements on the main thread. So, updating the title
            // in this object must be done on the main thread
            await mainDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Setting the title on ApplicationView to blank will clear the title in
                // the system switcher. It would be good to still have a title in the app's UI.
                if (newTitle == "")
                {
                    newTitle = EMPTY_TITLE;
                }

                ((App)App.Current).UpdateTitle(newTitle, thisViewId);
            });
            thisViewControl.StopViewInUse();
        }

        private async void ViewLifetimeControl_Released(Object sender, EventArgs e)
        {
            ((ViewLifetimeControl)sender).Released -= ViewLifetimeControl_Released;
            // The ViewLifetimeControl object is bound to UI elements on the main thread
            // So, the object must be removed from that thread
            await mainDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ((App)App.Current).SecondaryViews.Remove(thisViewControl);
            });

            // The released event is fired on the thread of the window
            // it pertains to.
            //
            // It's important to make sure no work is scheduled on this thread
            // after it starts to close (no data binding changes, no changes to
            // XAML, creating new objects in destructors, etc.) since
            // that will throw exceptions
            Window.Current.Close();
        }

        public void HandleProtocolLaunch(Uri uri)
        {
            // This code should only get executed if DisableSystemActivationPolicy
            // has not been called.
            //ProtocolText.Visibility = Visibility.Visible;
            //ProtocolText.Text = uri.AbsoluteUri;
        }

        private async void ProtocolLaunch_Click(object sender, RoutedEventArgs e)
        {
            // The activation will always end up on the same page unless you 
            // create an external protocol activation.
            thisViewControl.StartViewInUse();
            await Launcher.LaunchUriAsync(new Uri("multiple-view-sample://basiclaunch/"));
            thisViewControl.StopViewInUse();
        }

        public async void SwitchAndAnimate(int fromViewId)
        {
            // This continues the flow from Scenario 3
            thisViewControl.StartViewInUse();

            // Calculate the entrance animation. Recalculate this every time,
            // because the animation description can vary (for example,
            // if the user changes accessibility settings).
            Storyboard enterAnimation = CreateEnterAnimation(LayoutRoot);

            // Before switching, make this view match the outgoing window
            // (go to a blank background)
            enterAnimation.Begin();
            enterAnimation.Pause();
            enterAnimation.Seek(TimeSpan.FromMilliseconds(0));

            // Bring this view onto screen. Since the two view are drawing
            // the same visual, the user will not be able to perceive the switch
            await ApplicationViewSwitcher.SwitchAsync(ApplicationView.GetForCurrentView().Id,
                fromViewId,
                ApplicationViewSwitchingOptions.SkipAnimation);

            // Now that this window is on screen, animate in its contents
            enterAnimation.Begin();
            thisViewControl.StopViewInUse();
        }

        private Storyboard CreateEnterAnimation(Windows.UI.Xaml.Controls.Panel layoutRoot)
        {
            var enterAnimation = new Storyboard();

            // Use the AnimationDescription object if available. Otherwise, return an empty storyboard (no animation).
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Core.AnimationMetrics.AnimationDescription"))
            {
                Storyboard.SetTarget(enterAnimation, layoutRoot);

                var ad = new AnimationDescription(AnimationEffect.EnterPage, AnimationEffectTarget.Primary);
                for (int i = 0; i < layoutRoot.Children.Count; i++)
                {
                    // Add a render transform to the existing one just for animations
                    var element = layoutRoot.Children[i];
                    var tg = new TransformGroup();
                    tg.Children.Add(new TranslateTransform());
                    tg.Children.Add(element.RenderTransform);
                    element.RenderTransform = tg;

                    // Calculate the stagger for each animation. Note that this has a max
                    var delayMs = Math.Min(ad.StaggerDelay.TotalMilliseconds * i * ad.StaggerDelayFactor, ad.DelayLimit.Milliseconds);
                    var delay = TimeSpan.FromMilliseconds(delayMs);

                    foreach (var description in ad.Animations)
                    {
                        var animation = new DoubleAnimationUsingKeyFrames();

                        // Start the animation at the right offset
                        var startSpline = new SplineDoubleKeyFrame();
                        startSpline.KeyTime = TimeSpan.FromMilliseconds(0);
                        Storyboard.SetTarget(animation, element);

                        // Hold at that offset until the stagger delay is hit
                        var middleSpline = new SplineDoubleKeyFrame();
                        middleSpline.KeyTime = delay;

                        // Animation from delayed time to last time
                        var endSpline = new SplineDoubleKeyFrame();
                        endSpline.KeySpline = new KeySpline() { ControlPoint1 = description.Control1, ControlPoint2 = description.Control2 };
                        endSpline.KeyTime = description.Duration + delay;

                        // Do the translation
                        if (description.Type == PropertyAnimationType.Translation)
                        {
                            startSpline.Value = ANIMATION_TRANSLATION_START;
                            middleSpline.Value = ANIMATION_TRANSLATION_START;
                            endSpline.Value = ANIMATION_TRANSLATION_END;

                            Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(TransformGroup.Children)[0].X");
                        }
                        // Opacity
                        else if (description.Type == PropertyAnimationType.Opacity)
                        {
                            startSpline.Value = ANIMATION_OPACITY_START;
                            middleSpline.Value = ANIMATION_OPACITY_START;
                            endSpline.Value = ANIMATION_OPACITY_END;

                            Storyboard.SetTargetProperty(animation, "Opacity");
                        }
                        else
                        {
                            throw new Exception("Encountered an unexpected animation type.");
                        }

                        // Put the final animation together
                        animation.KeyFrames.Add(startSpline);
                        animation.KeyFrames.Add(middleSpline);
                        animation.KeyFrames.Add(endSpline);
                        enterAnimation.Children.Add(animation);
                    }
                }
            }
            return enterAnimation;
        }

        private void TestLinkMultiPage(object sender, RoutedEventArgs e)
        {
            //rootPage.link_multi_page();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rootPage.link_multi_page_connect_device(Convert.ToUInt32(cb_BaudRate.SelectedItem.ToString()), cb_list_com.SelectedItem.ToString());
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                //Application.Current.cl();
                thisViewControl.StartViewInUse();
                await ApplicationViewSwitcher.SwitchAsync(mainViewId,
                    ApplicationView.GetForCurrentView().Id,
                    ApplicationViewSwitchingOptions.ConsolidateViews);
                thisViewControl.StopViewInUse();
            }
            catch { }

            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
        }

        //list COM
        private async void ListAvailablePorts()
        {
            //test get name of pc

            var hostNames = NetworkInformation.GetHostNames();
            var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));
            var computerName = localName.DisplayName.Replace(".local", "");
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                //status.Text = "Select a device and connect";
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Remove(dis[i]);
                    cb_list_com.Items.Remove(dis[i].Name);
                }
                for (int i = 0; i < dis.Count; i++)
                {
                    {
                        listOfDevices.Add(dis[i]);
                        cb_list_com.Items.Add(dis[i].Name);
                    }

                }

                //DeviceListSource.Source = listOfDevices;
                //comPortInput.IsEnabled = true;
                //ConnectDevices.SelectedIndex = -1;
            }
            catch
            {
                //status.Text = ex.Message;
            }
        }

        private async void DisConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                rootPage.link_multi_page_dis_connect_device();
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                //Application.Current.cl();
                thisViewControl.StartViewInUse();
                await ApplicationViewSwitcher.SwitchAsync(mainViewId,
                    ApplicationView.GetForCurrentView().Id,
                    ApplicationViewSwitchingOptions.ConsolidateViews);
                thisViewControl.StopViewInUse();
            }
            catch { }

        }

        private void Test_pause_audio_Click(object sender, RoutedEventArgs e)
        {
            media.Pause();

        }
    }
}
