using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Services
{
    public interface ITipService
    {
        TipViewModel GetMonthlyTip();
    }

    public class TipService : ITipService
    {
        private readonly List<TipViewModel> _tips = new()
        {
            new TipViewModel { 
                Content = "Intenta saldar tus deudas pequeñas al principio del mes para tener una visión más clara de tu presupuesto.", 
                FooterMessage = "La clave es la organización.", 
                Tag = "Finanzas" 
            },
            new TipViewModel { 
                Content = "¿Sabías que los grupos con más de 3 integrantes suelen dividir gastos un 20% más seguido? ¡Invita a más amigos!", 
                FooterMessage = "Compartir es ahorrar.", 
                Tag = "Social" 
            },
            new TipViewModel { 
                Content = "Revisa tu historial de cambios para detectar gastos duplicados o errores de carga rápidamente.", 
                FooterMessage = "Tus cuentas, siempre claras.", 
                Tag = "App" 
            },
            new TipViewModel { 
                Content = "Los usuarios Premium ahorran tiempo usando la exportación a Excel para sus declaraciones mensuales.", 
                FooterMessage = "Optimiza tu tiempo.", 
                Tag = "Premium",
                Icon = "🌟"
            },
            new TipViewModel { 
                Content = "Establece un día a la semana para revisar quién te debe y enviar recordatorios amigables.", 
                FooterMessage = "Cuentas claras conservan la amistad.", 
                Tag = "Hábitos" 
            }
        };

        public TipViewModel GetMonthlyTip()
        {
            // Pick a tip based on the current month to ensure it changes monthly
            int monthIndex = DateTime.Now.Month % _tips.Count;
            return _tips[monthIndex];
        }
    }
}
