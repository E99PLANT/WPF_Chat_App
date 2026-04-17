using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Chat_Group_System.Controllers;
using Chat_Group_System.Models.Entities;
using System.ComponentModel;

namespace Chat_Group_System.Views
{
    public partial class GroupSettingsWindow : Window, INotifyPropertyChanged
    {
        private readonly ChatController _chatController;
        private readonly UserController _userController;
        private Conversation? _conversation;

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                if (_isAdmin != value)
                {
                    _isAdmin = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAdmin)));
                }
            }
        }

        public GroupSettingsWindow(ChatController chatController, UserController userController)
        {
            InitializeComponent();
            _chatController = chatController;
            _userController = userController;

            DataContext = this;
            Loaded += GroupSettingsWindow_Loaded;
        }

        public void SetConversation(Conversation conversation)
        {
            _conversation = conversation;
        }

        private async void GroupSettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMembersAsync();
        }

        private async Task LoadMembersAsync()
        {
            if (_conversation == null) return;
            try
            {
                var members = await _chatController.GetGroupMembersAsync(_conversation.Id);
                var currentUserMember = members.FirstOrDefault(m => m.UserId == App.CurrentUser?.Id);
                
                if (currentUserMember != null)
                {
                    IsAdmin = currentUserMember.Role == "Admin";
                    LeaveDisbandButton.Content = IsAdmin ? "Disband Group" : "Leave Group";
                }

                MembersListView.ItemsSource = members;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}");
            }
        }

        private async void AddMember_Click(object sender, RoutedEventArgs e)
        {
            if (_conversation == null) return;
            if (!IsAdmin)
            {
                MessageBox.Show("Only admins can add members.");
                return;
            }

            var email = EmailTextBox.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter an email.");
                return;
            }

            if (App.CurrentUser == null) return;

            // Find user by email
            var userToAdd = await _userController.GetUserByEmailAsync(email);
            if (userToAdd == null)
            {
                MessageBox.Show("User not found.");
                return;
            }

            var result = await _chatController.AddMemberToGroupAsync(_conversation.Id, App.CurrentUser.Id, userToAdd.Id);
            if (result.Success)
            {
                MessageBox.Show("Member added successfully.");
                EmailTextBox.Text = string.Empty;
                await LoadMembersAsync();
            }
            else
            {
                MessageBox.Show($"Failed to add member: {result.Message}");
            }
        }

        private async void RemoveMember_Click(object sender, RoutedEventArgs e)
        {
            if (_conversation == null) return;
            if (!IsAdmin) return;
            if (App.CurrentUser == null) return;

            if (sender is Button button && button.Tag is int userIdToRemove)
            {
                if (userIdToRemove == App.CurrentUser.Id)
                {
                    MessageBox.Show("You cannot remove yourself. Use Leave Group instead.");
                    return;
                }

                var result = await _chatController.RemoveMemberFromGroupAsync(_conversation.Id, App.CurrentUser.Id, userIdToRemove);
                if (result.Success)
                {
                    MessageBox.Show("Member removed successfully.");
                    await LoadMembersAsync();
                }
                else
                {
                    MessageBox.Show($"Failed to remove member: {result.Message}");
                }
            }
        }

        private async void LeaveDisband_Click(object sender, RoutedEventArgs e)
        {
            if (_conversation == null) return;
            if (App.CurrentUser == null) return;

            var actionName = IsAdmin ? "disband" : "leave";
            var resultMsg = MessageBox.Show($"Are you sure you want to {actionName} this group?", "Confirm", MessageBoxButton.YesNo);
            
            if (resultMsg == MessageBoxResult.Yes)
            {
                var result = await _chatController.LeaveOrDisbandGroupAsync(_conversation.Id, App.CurrentUser.Id);
                if (result.Success)
                {
                    MessageBox.Show($"Successfully {actionName}ed group.");
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"Error: {result.Message}");
                }
            }
        }
    }
}