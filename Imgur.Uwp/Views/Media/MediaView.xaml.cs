using Imgur.Contracts;
using Imgur.Uwp.Controls;
using Imgur.ViewModels.Media;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Imgur.Uwp.Views.Media
{
    public sealed partial class MediaView : Page
    {
        private Storyboard _upvoteFeedbackStoryboard;
        private DispatcherTimer _upvoteOverlayTimer;
        private VideoViewElementControl _compactSource;
        private bool _compactDismissed;
        private bool _compactActive;
        private bool _compactBusy;
        private bool _compactDragged;
        private const double CompactDragTapThreshold = 8;

        public MediaView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            _upvoteOverlayTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _upvoteOverlayTimer.Tick += (s, e) =>
            {
                _upvoteOverlayTimer.Stop();
                UpvoteFeedbackOverlay.Visibility = Visibility.Collapsed;
            };
        }

        public MediaViewModel ViewModel => (MediaViewModel)this.DataContext;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = e.Parameter as MediaViewModel;
            await ViewModel.InitializeAsync();

            App.Services.GetRequiredService<IShareService>().Initialize();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            DismissCompactPip(restorePlayer: true, markDismissed: false);
            base.OnNavigatingFrom(e);
        }

        public void ShowUpvoteFeedback()
        {
            if (ViewModel?.Vote != null && ViewModel.Vote.CanExecute("up:only"))
                ViewModel.Vote.Execute("up:only");

            UpvoteFeedbackOverlay.Visibility = Visibility.Visible;
            UpvoteFeedbackIconScale.ScaleX = 0.1;
            UpvoteFeedbackIconScale.ScaleY = 0.1;
            UpvoteFeedbackLabel.Opacity = 0;

            _upvoteFeedbackStoryboard?.Stop();

            var storyboard = new Storyboard();

            var scaleX = new DoubleAnimationUsingKeyFrames();
            scaleX.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(0), Value = 0.1 });
            scaleX.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(280),
                Value = 1.0,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
            Storyboard.SetTarget(scaleX, UpvoteFeedbackIconScale);
            Storyboard.SetTargetProperty(scaleX, "ScaleX");

            var scaleY = new DoubleAnimationUsingKeyFrames();
            scaleY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(0), Value = 0.1 });
            scaleY.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(280),
                Value = 1.0,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
            Storyboard.SetTarget(scaleY, UpvoteFeedbackIconScale);
            Storyboard.SetTargetProperty(scaleY, "ScaleY");

            var labelFade = new DoubleAnimationUsingKeyFrames();
            labelFade.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(0), Value = 0 });
            labelFade.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromMilliseconds(220), Value = 0 });
            labelFade.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = TimeSpan.FromMilliseconds(450),
                Value = 1.0,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
            Storyboard.SetTarget(labelFade, UpvoteFeedbackLabel);
            Storyboard.SetTargetProperty(labelFade, "Opacity");

            storyboard.Children.Add(scaleX);
            storyboard.Children.Add(scaleY);
            storyboard.Children.Add(labelFade);
            _upvoteFeedbackStoryboard = storyboard;
            storyboard.Begin();

            _upvoteOverlayTimer.Stop();
            _upvoteOverlayTimer.Start();
        }

        private void NavigateToTop_Click(object sender, RoutedEventArgs e)
        {
            ResetScrollOffset();
        }

        private void ResetScrollOffset()
        {
            if (RootScrollView.VerticalOffset > 0)
            {
                this.RootScrollView.ChangeView(0, 0, null);
            }
        }

        private void RootScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            try
            {
                if (this.RootScrollView.VerticalOffset > 0 && !ViewModel.CanScrollToTop)
                {
                    ViewModel.CanScrollToTop = true;
                }
                else if (this.RootScrollView.VerticalOffset == 0 && ViewModel.CanScrollToTop)
                {
                    ViewModel.CanScrollToTop = false;
                }

                // Evita thrash/crash a meio do fling
                if (!e.IsIntermediate)
                    UpdateCompactPip();
            }
            catch
            {
                Debug.WriteLine("Erro ScrollView ViewChanged");
            }
        }

        private void UpdateCompactPip()
        {
            if (_compactBusy)
                return;

            try
            {
                if (_compactActive && _compactSource != null)
                {
                    if (!_compactSource.IsActivelyPlaying)
                    {
                        // Vídeo terminou no PiP: mantém o PiP e mostra replay
                        if (_compactSource.IsPlaybackFinished)
                        {
                            CompactPipReplayOverlay.Visibility = Visibility.Visible;
                            return;
                        }

                        DismissCompactPip(restorePlayer: true, markDismissed: false);
                        return;
                    }

                    CompactPipReplayOverlay.Visibility = Visibility.Collapsed;

                    if (IsElementVisibleInScrollViewer(_compactSource, RootScrollView))
                        DismissCompactPip(restorePlayer: true, markDismissed: false);

                    return;
                }

                if (_compactDismissed)
                {
                    // Só reativa o compact depois de o user voltar a ver o vídeo na listagem
                    // (ou após replay — ver NotifyMainPlayerPlaybackStarted).
                    if (IsAnyVideoVisibleInScrollViewer())
                        _compactDismissed = false;
                    else
                        return;
                }

                var playing = FindPlayingVideo();
                if (playing == null)
                    return;

                if (!IsElementVisibleInScrollViewer(playing, RootScrollView))
                    ShowCompactPip(playing);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateCompactPip: " + ex.Message);
            }
        }

        /// <summary>
        /// Chamado quando o user dá play no player principal — permite o compact voltar a aparecer.
        /// </summary>
        public void NotifyMainPlayerPlaybackStarted()
        {
            _compactDismissed = false;
            UpdateCompactPip();
        }

        /// <summary>
        /// Vídeo terminou enquanto estava no compact pip — mostra replay.
        /// </summary>
        public void NotifyCompactPipPlaybackEnded()
        {
            if (!_compactActive)
                return;

            CompactPipReplayOverlay.Visibility = Visibility.Visible;
        }

        private async void CompactPipReplay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (_compactSource == null)
                return;

            if (await _compactSource.ReplayFromStartAsync())
            {
                CompactPipReplayOverlay.Visibility = Visibility.Collapsed;
                _compactDismissed = false;
            }
        }

        private void ShowCompactPip(VideoViewElementControl source)
        {
            if (source == null || _compactActive || _compactBusy)
                return;

            _compactBusy = true;
            try
            {
                var offsetY = RootScrollView.VerticalOffset;
                var offsetX = RootScrollView.HorizontalOffset;

                if (!source.MovePlayerToHost(CompactPipSurface))
                    return;

                _compactSource = source;
                _compactActive = true;
                CompactPipTransform.X = 0;
                CompactPipTransform.Y = 0;
                _compactDragged = false;
                CompactPipReplayOverlay.Visibility = Visibility.Collapsed;
                CompactPipHost.Visibility = Visibility.Visible;

                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    RootScrollView.ChangeView(offsetX, offsetY, null, disableAnimation: true);
                });
            }
            finally
            {
                _compactBusy = false;
            }
        }

        private void DismissCompactPip(bool restorePlayer, bool markDismissed)
        {
            if (_compactBusy)
                return;

            if (!_compactActive && CompactPipHost.Visibility != Visibility.Visible)
            {
                if (markDismissed)
                    _compactDismissed = true;
                return;
            }

            _compactBusy = true;
            try
            {
                if (restorePlayer && _compactSource != null)
                    _compactSource.RestorePlayerFromHost();

                CompactPipSurface.Children.Clear();
                CompactPipReplayOverlay.Visibility = Visibility.Collapsed;
                CompactPipHost.Visibility = Visibility.Collapsed;
                CompactPipTransform.X = 0;
                CompactPipTransform.Y = 0;
                _compactDragged = false;
                _compactSource = null;
                _compactActive = false;

                if (markDismissed)
                    _compactDismissed = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DismissCompactPip: " + ex.Message);
            }
            finally
            {
                _compactBusy = false;
            }
        }

        private void CompactPipClose_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Impede que o tap do X propague para CompactPipMedia_Tapped (rolar até a mídia).
            e.Handled = true;

            var offsetY = RootScrollView.VerticalOffset;
            var offsetX = RootScrollView.HorizontalOffset;

            DismissCompactPip(restorePlayer: true, markDismissed: true);

            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                RootScrollView.ChangeView(offsetX, offsetY, null, disableAnimation: true);
            });
        }

        private void CompactPipHost_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _compactDragged = false;
        }

        private void CompactPipHost_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            CompactPipTransform.X += e.Delta.Translation.X;
            CompactPipTransform.Y += e.Delta.Translation.Y;

            if (Math.Abs(e.Cumulative.Translation.X) > CompactDragTapThreshold
                || Math.Abs(e.Cumulative.Translation.Y) > CompactDragTapThreshold)
            {
                _compactDragged = true;
            }

            ClampCompactPipTransform();
        }

        private void CompactPipHost_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            ClampCompactPipTransform();
        }

        private void ClampCompactPipTransform()
        {
            // CompactPipHost is Right+Bottom aligned; transform offsets from that corner.
            var parent = CompactPipHost.Parent as FrameworkElement ?? RootGrid;
            var maxX = Math.Max(0, parent.ActualWidth - CompactPipHost.ActualWidth - CompactPipHost.Margin.Right);
            var maxY = Math.Max(0, parent.ActualHeight - CompactPipHost.ActualHeight - CompactPipHost.Margin.Bottom);

            // X negativo = para a esquerda; Y negativo = para cima
            CompactPipTransform.X = Math.Max(-maxX, Math.Min(CompactPipHost.Margin.Right, CompactPipTransform.X));
            CompactPipTransform.Y = Math.Max(-maxY, Math.Min(CompactPipHost.Margin.Bottom, CompactPipTransform.Y));
        }

        private void CompactPipMedia_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_compactDragged)
            {
                _compactDragged = false;
                e.Handled = true;
                return;
            }

            var source = _compactSource;
            if (source == null)
                return;

            e.Handled = true;
            // Só o tap no meio da mídia rola de volta até ela.
            DismissCompactPip(restorePlayer: true, markDismissed: false);
            ScrollToElement(source);
        }

        private void ScrollToElement(FrameworkElement element)
        {
            try
            {
                var transform = element.TransformToVisual((UIElement)RootScrollView.Content);
                var position = transform.TransformPoint(new Point(0, 0));
                var target = Math.Max(0, position.Y - 24);
                RootScrollView.ChangeView(null, target, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ScrollToElement: " + ex.Message);
            }
        }

        private static bool IsElementVisibleInScrollViewer(FrameworkElement element, ScrollViewer scrollViewer)
        {
            if (element == null || scrollViewer == null)
                return false;

            if (element.ActualWidth <= 0 || element.ActualHeight <= 0)
                return false;

            try
            {
                var transform = element.TransformToVisual(scrollViewer);
                var bounds = transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                var viewport = new Rect(0, 0, scrollViewer.ActualWidth, scrollViewer.ActualHeight);
                bounds.Intersect(viewport);

                if (bounds.IsEmpty)
                    return false;

                var minVisible = Math.Min(64, element.ActualHeight * 0.2);
                return bounds.Height >= minVisible;
            }
            catch
            {
                return true;
            }
        }

        private VideoViewElementControl FindPlayingVideo()
        {
            foreach (var video in FindVisualChildren<VideoViewElementControl>(RootScrollView))
            {
                if (video.IsActivelyPlaying)
                    return video;
            }

            if (_compactActive && _compactSource != null && _compactSource.IsActivelyPlaying)
                return _compactSource;

            return null;
        }

        private bool IsAnyVideoVisibleInScrollViewer()
        {
            foreach (var video in FindVisualChildren<VideoViewElementControl>(RootScrollView))
            {
                if (IsElementVisibleInScrollViewer(video, RootScrollView))
                    return true;
            }
            return false;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                yield break;

            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match)
                    yield return match;

                foreach (var nested in FindVisualChildren<T>(child))
                    yield return nested;
            }
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (VisualStateManager.GetVisualStateGroups(this.RootGrid).First().CurrentState.Name == "Extended")
                {
                    RootScrollGrid.MaxHeight = RootGrid.ActualHeight;
                }
                else
                {
                    RootScrollGrid.MaxHeight = double.PositiveInfinity;
                }
            }
            catch
            {
                Debug.WriteLine("Erro RootGrid SizeChanged");
            }
        }

        private void ScrollToComments_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var transform = CommentsPlacement.TransformToVisual((UIElement)RootScrollView.Content);
                var position = transform.TransformPoint(new Point(0, 0));
                RootScrollView.ChangeView(null, position.Y, null);
            }
            catch
            {
                Debug.WriteLine("Erro ScrollView To Comments");
            }
        }
    }
}
