using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SplitMoney.Client.Services;
using SplitMoney.Client.Models;
using SplitMoney.Client.Models.ViewModels;

namespace SplitMoney.Client.Components.Pages
{
    public partial class CreateGroup
    {
        [Inject] public NavigationManager Navigation { get; set; } = default!;
        [Inject] public IExpenseService ExpenseService { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;
        [Inject] public IModalService ModalService { get; set; } = default!;
        [Inject] public IAuthService AuthService { get; set; } = default!;
        [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        private CreateGroupFormModel formModel = new();
        private string newMemberEmail = string.Empty;
        private string userEmail = string.Empty;
        private List<MemberSpendRecordViewModel> members = new();
        private bool submitting = false;
        private bool isPremium = false;

        [Parameter]
        public Guid? GroupId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            isPremium = await AuthService.IsPremiumAsync();
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();

            userEmail = authState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
            
            if (GroupId.HasValue)
            {
                var groupsResult = await ExpenseService.GetUserGroupsAsync();
                var userGroups = groupsResult.Succeeded && groupsResult.Data != null ? groupsResult.Data : new List<GroupSummaryViewModel>();
                var group = userGroups.FirstOrDefault(g => g.Id == GroupId.Value.ToString());
                if (group != null)
                {
                    formModel.GroupName = group.Name;
                    var currentMembersResult = await ExpenseService.GetGroupMembersAsync(group.Id);
                    var currentMembers = currentMembersResult.Succeeded && currentMembersResult.Data != null ? currentMembersResult.Data : new List<GroupMemberResponse>();
                    members = currentMembers.Select(m => new MemberSpendRecordViewModel { 
                        Email = m.Email,
                        AmountSpent = 0 
                    }).ToList();
                }
            }
            else if (!string.IsNullOrEmpty(userEmail))
            {
                members.Add(new MemberSpendRecordViewModel { Email = userEmail, AmountSpent = 0 });
            }
        }

        private void AddMember()
        {
            if (string.IsNullOrWhiteSpace(newMemberEmail) || !newMemberEmail.Contains("@") || members.Any(m => m.Email == newMemberEmail))
            {
                return;
            }

            if (!isPremium && members.Count >= 5)
            {
                ModalService.ShowModal("Límite de Invitados", "Los círculos estándar tienen un límite de 5 personas. Pásate a Premium para grupos sin límites.", ModalType.Warning);
                return;
            }

            members.Add(new MemberSpendRecordViewModel { Email = newMemberEmail, AmountSpent = 0 });
            newMemberEmail = string.Empty;
        }

        private void GoBack() => Navigation.NavigateTo("/groups");

        private async Task HandleSubmit()
        {
            submitting = true;
            ApiResult result;
            if (GroupId.HasValue) result = await ExpenseService.UpdateGroupAsync(GroupId.Value, formModel.GroupName, members);
            else result = await ExpenseService.CreateGroupAsync(formModel.GroupName, members);
            
            submitting = false;

            if (result.Succeeded)
            {
                ToastService.ShowToast(GroupId.HasValue ? "Grupo actualizado" : "¡Círculo creado!", ToastLevel.Success);
                Navigation.NavigateTo("/groups");
            }
            else
            {
                ModalService.ShowModal("Error", result.Message ?? "No se pudo procesar la solicitud.", ModalType.Error);
            }
        }
    }
}
