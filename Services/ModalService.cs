using System;

namespace SplitMoney.Client.Services
{
    public interface IModalService
    {
        event Action OnShow;
        event Action OnHide;
        string Title { get; }
        string Message { get; }
        ModalType Type { get; }
        void ShowModal(string title, string message, ModalType type);
        void HideModal();
    }

    public class ModalService : IModalService
    {
        public event Action OnShow;
        public event Action OnHide;
        
        public string Title { get; private set; }
        public string Message { get; private set; }
        public ModalType Type { get; private set; }

        public void ShowModal(string title, string message, ModalType type)
        {
            Title = title;
            Message = message;
            Type = type;
            OnShow?.Invoke();
        }

        public void HideModal()
        {
            OnHide?.Invoke();
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
