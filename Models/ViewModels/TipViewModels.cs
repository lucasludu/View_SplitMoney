namespace SplitMoney.Client.Models.ViewModels
{
    public class TipViewModel
    {
        public string Title { get; set; } = "Tip del Mes";
        public string Icon { get; set; } = "💡";
        public string Content { get; set; } = string.Empty;
        public string FooterMessage { get; set; } = "¡Buen trabajo!";
        public string Tag { get; set; } = "Ahorro";
    }
}
