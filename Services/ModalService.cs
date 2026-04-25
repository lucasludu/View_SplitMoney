using System;
using System.Threading.Tasks;

namespace SplitMoney.Client.Services
{
    public interface IModalService
    {
        event Action OnShow;
        event Action OnHide;
        string Title { get; }
        string Message { get; }
        ModalType Type { get; }
        bool IsConfirmation { get; }
        void ShowModal(string title, string message, ModalType type);
        Task<bool> ShowConfirmAsync(string title, string message, ModalType type = ModalType.Warning);
        void Confirm(bool result);
        void HideModal();
    }

    public class ModalService : IModalService
    {
        public event Action OnShow;
        public event Action OnHide;
        
        public string Title { get; private set; }
        public string Message { get; private set; }
        public ModalType Type { get; private set; }
        public bool IsConfirmation { get; private set; }

        private TaskCompletionSource<bool> _tcs;

        public void ShowModal(string title, string message, ModalType type)
        {
            Title = title;
            Message = message;
            Type = type;
            IsConfirmation = false;
            OnShow?.Invoke();
        }

        public Task<bool> ShowConfirmAsync(string title, string message, ModalType type = ModalType.Warning)
        {
            Title = title;
            Message = message;
            Type = type;
            IsConfirmation = true;
            
            _tcs = new TaskCompletionSource<bool>();
            OnShow?.Invoke();
            return _tcs.Task;
        }

        public void Confirm(bool result)
        {
            _tcs?.TrySetResult(result);
            HideModal();
            _tcs = null;
        }

        public void HideModal()
        {
            OnHide?.Invoke();
            if (_tcs != null)
            {
                _tcs.TrySetResult(false);
                _tcs = null;
            }
        }
    }

    public enum ModalType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
