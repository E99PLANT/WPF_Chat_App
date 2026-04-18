using System.Windows;
using System.Windows.Controls;
using Chat_Group_System.ViewModels;

namespace Chat_Group_System
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelfMessageTemplate { get; set; } = null!;
        public DataTemplate OtherMessageTemplate { get; set; } = null!;
        public DataTemplate SystemMessageTemplate { get; set; } = null!;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MessageViewModel message && App.CurrentUser != null)
            {
                if (message.Type == Models.Entities.MessageType.System)
                {
                    return SystemMessageTemplate;
                }

                if (message.SenderId == App.CurrentUser.Id)
                {
                    return SelfMessageTemplate;
                }
                return OtherMessageTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}