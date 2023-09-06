using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace DSPi
{
    public class Views
    {
        public static async Task ShowTestModeDialogAsync(Window parent)
        {
            var dialog = new Dialog("Test Mode Check", 60, 10);

            var label = new Label(1, 1, "Test Mode is NOT-ENABLED, You should ENABLE it first.");
            dialog.Add(label);

            var tcs = new TaskCompletionSource<bool>();

            var button = new Button("Enable");
            button.Clicked += async () =>
            {
                label.Text = "ENABLING...Please wait......";
                Application.Refresh();

                // Enable action
                TestModeCheck.EnableTestMode();

                Thread.Sleep(1000);

                tcs.SetResult(true); // Set the result when the button is clicked

                parent.Remove(dialog);

                await ShowRebootDialogAsync(parent);
            };
            dialog.AddButton(button);

            var closeButton = new Button("Exit");
            closeButton.Clicked += () =>
            {
                Environment.Exit(0);
            };
            dialog.AddButton(closeButton);

            parent.Add(dialog);

            Application.Run(); // Run the application loop

            await tcs.Task; // Wait for the button click
        }
        public static async Task<bool> ShowDispatcherDialogAsync(Window parent, string driverName)
        {
            var dialog = new Dialog("Dispatcher Check", 80, 10);

            var label = new Label(1, 1, $"You have installed some version of Dispatcher. You should uninstall it first.");
            dialog.Add(label);

            var tcs = new TaskCompletionSource<bool>();

            var button = new Button("Uninstall");
            button.Clicked += async () =>
            {
                label.Text="Uninstalling...Please wait......";
                Application.Refresh();

                DriverInstaller.UninstallDriver(driverName);
                DriverInstaller.RestorePowerCfgCommands();
                Thread.Sleep(300);

                label.Text = "Uninstall success!";
                Application.Refresh();

                Thread.Sleep(500);

                tcs.SetResult(true); // Set the result to true when the uninstall button is clicked

                parent.Remove(dialog);

                await ShowRebootDialogAsync(parent);
            };
            dialog.AddButton(button);

            var closeButton = new Button("Exit");
            closeButton.Clicked += () =>
            {
                Environment.Exit(0);
            };
            dialog.AddButton(closeButton);

            parent.Add(dialog);

            Application.Run(); // Run the application loop

            await tcs.Task; // Wait for the button click
            return tcs.Task.Result;
        }
        public static async Task ShowRebootDialogAsync(Window parent)
        {
            var dialog = new Dialog("Reboot Required", 60, 10);

            var label = new Label(1, 1, "You need to reboot to make the changes take effect!");
            dialog.Add(label);

            var button3 = new Button("Reboot");
            var tcs = new TaskCompletionSource<bool>();

            button3.Clicked += () =>
            {
                Process.Start("shutdown", "/r /t 0");
                Thread.Sleep(300);
                Environment.Exit(0);
            };
            dialog.AddButton(button3);

            var closeButton = new Button("Exit");
            closeButton.Clicked += () =>
            {
                Environment.Exit(0);
            };
            dialog.AddButton(closeButton);

            parent.Add(dialog);

            Application.Run(); // Run the application loop

            await tcs.Task; // Wait for the button click
        }
        public static async Task<bool> ShowNetworkDialogAsync(Window parent)
        {
            var tcs = new TaskCompletionSource<bool>();

            var dialog = new Dialog("Network Check", 60, 10);

            var label = new Label(1, 1, "Time out for requesting...Please check your network!");
            dialog.Add(label);

            var closeButton = new Button("Exit");
            closeButton.Clicked += () =>
            {
                Environment.Exit(0);
            };
            dialog.AddButton(closeButton);

            parent.Add(dialog);

            Application.Run(); // Run the application loop

            await tcs.Task; // Wait for the button click
            return tcs.Task.Result;
        }

    }
}
