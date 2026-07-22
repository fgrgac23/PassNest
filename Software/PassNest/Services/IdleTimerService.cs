using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassNest.Services
{
    public class IdleTimerService : IIdleTimerService
    {
        private readonly DispatcherTimer timer = new();
        private Window? attachedWindow;
        private EventHandler<PointerEventArgs>? pointerHandler;
        private EventHandler<KeyEventArgs>? keyHandler;

        public event Action? TimedOut;

        public IdleTimerService()
        {
            timer.Tick += OnTick;
        }

        public void Start(TimeSpan? timeout)
        {
            Stop();

            if (timeout is null || timeout.Value <= TimeSpan.Zero) return;

            timer.Interval = timeout.Value;
            timer.Start();

            AttachToWindow();
        }

        public void Stop()
        {
            timer.Stop();
            DetachFromWindow();
        }

        private void AttachToWindow()
        {
            var window = GetMainWindow();
            if (window == null) return;

            attachedWindow = window;
            pointerHandler = (_, _) => RestartTimer();
            keyHandler = (_, _) => RestartTimer();

            window.AddHandler(InputElement.PointerMovedEvent, pointerHandler, RoutingStrategies.Tunnel);
            window.AddHandler(InputElement.PointerPressedEvent, pointerHandler, RoutingStrategies.Tunnel);
            window.AddHandler(InputElement.KeyDownEvent, keyHandler, RoutingStrategies.Tunnel);
        }

        private void DetachFromWindow()
        {
            if (attachedWindow == null) return;

            if (pointerHandler != null)
            {
                attachedWindow.RemoveHandler(InputElement.PointerMovedEvent, pointerHandler);
                attachedWindow.RemoveHandler(InputElement.PointerPressedEvent, pointerHandler);
            }

            if (keyHandler != null)
            {
                attachedWindow.RemoveHandler(InputElement.KeyDownEvent, keyHandler);
            }

            attachedWindow = null;
            pointerHandler = null;
            keyHandler = null;
        }

        private void RestartTimer()
        {
            if (!timer.IsEnabled) return;

            timer.Stop();
            timer.Start();
        }

        private void OnTick(object? sender, EventArgs e)
        {
            timer.Stop();
            TimedOut?.Invoke();
        }

        private static Window? GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) return desktop.MainWindow;

            return null;
        }
    }
}
