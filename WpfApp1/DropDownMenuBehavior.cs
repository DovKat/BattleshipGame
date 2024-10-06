using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WpfApp1
{
    class DropDownMenuBehavior : Behavior<Button>
    {
        private bool IsContextMenuOpen;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnClick), true);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(OnClick));

        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            if (b != null && b.ContextMenu != null && !IsContextMenuOpen)
            {
                b.ContextMenu.AddHandler(ContextMenu.ClosedEvent, new RoutedEventHandler(OnContextMenuClosed), true);
                b.ContextMenu.IsOpen = true;
                IsContextMenuOpen = true;
            }
        }

        private void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            var menu = sender as ContextMenu;

            IsContextMenuOpen = false;

            if(menu != null)
            {
                menu.RemoveHandler(ContextMenu.ClosedEvent, new RoutedEventHandler(OnContextMenuClosed));
            }
        }
    }
}
