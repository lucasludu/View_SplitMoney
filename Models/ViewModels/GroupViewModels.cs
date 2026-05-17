using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SplitMoney.Client.Models.ViewModels
{
    public class GroupSpendingBreakdownViewModel
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public decimal TotalGroupExpense { get; set; }
        public List<MemberSpendingViewModel> Members { get; set; } = new();
    }

    public class MemberSpendingViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal NetBalance { get; set; }
    }

    public class CreateGroupFormModel
    {
        [Required(ErrorMessage = "El nombre del grupo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
        public string GroupName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "La descripción no puede tener más de 100 caracteres.")]
        public string Description { get; set; } = string.Empty;
    }
}
