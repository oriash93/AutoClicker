using System;
using System.Collections.Generic;
using System.Windows.Controls;
using AutoClicker.Enums;
using AutoClicker.Models;
using AutoClicker.Utils;

namespace AutoClicker.Views
{
    public class SystemTrayMenu : ContextMenu
    {
        private readonly List<object> contextMenu = new List<object>();

        public SystemTrayMenu()
        {
            ItemsSource = contextMenu;

            MenuItem minimizeMenuItem = new MenuItem
            {
                Header = Constants.SYSTEM_TRAY_MENU_MINIMIZE
            };
            minimizeMenuItem.Click += OnMinimizeMenuItemClick;

            MenuItem showMenuItem = new MenuItem
            {
                Header = Constants.SYSTEM_TRAY_MENU_SHOW,
                Visibility = System.Windows.Visibility.Collapsed
            };
            showMenuItem.Click += OnShowMenuItemClick;

            MenuItem exitMenuItem = new MenuItem
            {
                Header = Constants.SYSTEM_TRAY_MENU_EXIT
            };
            exitMenuItem.Click += OnExitMenuItemClick;

            contextMenu.Add(minimizeMenuItem);
            contextMenu.Add(showMenuItem);
            contextMenu.Add(new Separator());
            contextMenu.Add(exitMenuItem);
        }

        public event EventHandler<SystemTrayMenuActionEventArgs> SystemTrayMenuActionEvent;

        private void OnShowMenuItemClick(object sender, System.Windows.RoutedEventArgs e)
        {
            InvokeSystemTrayMenuActionEvent(SystemTrayMenuAction.Show);
            ToggleMenuItemsVisibility(false);
        }

        private void OnMinimizeMenuItemClick(object sender, System.Windows.RoutedEventArgs e)
        {
            InvokeSystemTrayMenuActionEvent(SystemTrayMenuAction.Hide);
            ToggleMenuItemsVisibility(true);
        }

        private void OnExitMenuItemClick(object sender, System.Windows.RoutedEventArgs e)
        {
            InvokeSystemTrayMenuActionEvent(SystemTrayMenuAction.Exit);
        }

        private void InvokeSystemTrayMenuActionEvent(SystemTrayMenuAction action)
        {
            SystemTrayMenuActionEventArgs args = new SystemTrayMenuActionEventArgs
            {
                Action = action
            };
            SystemTrayMenuActionEvent.Invoke(this, args);
        }

        public void ToggleMenuItemsVisibility(bool minimized)
        {
            MenuItem minimizeMenuItem = contextMenu[0] as MenuItem;
            minimizeMenuItem.Visibility = minimized ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            contextMenu[0] = minimizeMenuItem;

            MenuItem showMenuItem = contextMenu[1] as MenuItem;
            showMenuItem.Visibility = minimized ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            contextMenu[1] = showMenuItem;
        }
    }
}
