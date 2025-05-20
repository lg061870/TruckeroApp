using Microsoft.AspNetCore.Components;
using System.Timers;
using Timer = System.Timers.Timer;

namespace TruckeroApp.Services
{
    /// <summary>
    /// Interface for toast notification service
    /// </summary>
    public interface IToastService
    {
        /// <summary>
        /// Event that is triggered when a toast is shown
        /// </summary>
        event Action<ToastLevel, string, string?>? OnShow;

        /// <summary>
        /// Event that is triggered when a toast is hidden
        /// </summary>
        event Action? OnHide;

        /// <summary>
        /// Shows a success toast notification
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="heading">Optional heading for the toast</param>
        Task ShowSuccess(string message, string? heading = null);

        /// <summary>
        /// Shows an error toast notification
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="heading">Optional heading for the toast</param>
        Task ShowError(string message, string? heading = null);

        /// <summary>
        /// Shows a warning toast notification
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="heading">Optional heading for the toast</param>
        Task ShowWarning(string message, string? heading = null);

        /// <summary>
        /// Shows an information toast notification
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="heading">Optional heading for the toast</param>
        Task ShowInfo(string message, string? heading = null);
    }

    /// <summary>
    /// Toast notification level
    /// </summary>
    public enum ToastLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Implementation of the toast notification service
    /// </summary>
    public class ToastService : IToastService, IDisposable
    {
        private Timer? _countdown;

        /// <inheritdoc/>
        public event Action<ToastLevel, string, string?>? OnShow;

        /// <inheritdoc/>
        public event Action? OnHide;

        /// <inheritdoc/>
        public Task ShowSuccess(string message, string? heading = null)
        {
            ShowToast(ToastLevel.Success, message, heading);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task ShowError(string message, string? heading = null)
        {
            ShowToast(ToastLevel.Error, message, heading);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task ShowWarning(string message, string? heading = null)
        {
            ShowToast(ToastLevel.Warning, message, heading);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task ShowInfo(string message, string? heading = null)
        {
            ShowToast(ToastLevel.Info, message, heading);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Shows a toast notification
        /// </summary>
        private void ShowToast(ToastLevel level, string message, string? heading = null)
        {
            OnShow?.Invoke(level, message, heading);
            StartCountdown();
        }

        /// <summary>
        /// Starts the countdown timer for the toast
        /// </summary>
        private void StartCountdown()
        {
            SetCountdown();

            if (_countdown!.Enabled)
            {
                _countdown.Stop();
                _countdown.Start();
            }
            else
            {
                _countdown!.Start();
            }
        }

        /// <summary>
        /// Sets up the countdown timer
        /// </summary>
        private void SetCountdown()
        {
            if (_countdown == null)
            {
                _countdown = new Timer(5000);
                _countdown.Elapsed += HideToast;
                _countdown.AutoReset = false;
            }
        }

        /// <summary>
        /// Hides the toast when the timer elapses
        /// </summary>
        private void HideToast(object? source, ElapsedEventArgs args)
        {
            OnHide?.Invoke();
        }

        /// <summary>
        /// Disposes the timer
        /// </summary>
        public void Dispose()
        {
            _countdown?.Dispose();
        }
    }
}
