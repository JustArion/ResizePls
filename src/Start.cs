using System;
using Dawn.Resize;
using MelonLoader;

[assembly: MelonInfo(typeof(Start), "ResizePls", "1.0", "arion#1223")]
[assembly: MelonGame]
[assembly: MelonColor(ConsoleColor.White)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]
namespace Dawn.Resize
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using MelonLoader;
    using UnityEngine;

    public sealed class Start : MelonMod
    {

        private static void ShowWindow(nShowWindow window)
        {
            _ = Task.Run(() =>
            {
                TryRefreshCache();

                foreach (var intptr in Native.CachedWindowPointers) 
                    Native.ShowWindow(intptr, (int) window);
            });
            if (MelonDebug.IsEnabled())
                MelonDebug.Msg($"[GameWindowState] - {window.ToString()}");

        }

        //This should return a bool if we follow standards. Though should we if we never use it?
        private static void TryRefreshCache()
        {
            if (Native.CachedWindowPointers.Count >= 2) return;

            var processPointers = Native.GetProcessWindows(Process.GetCurrentProcess().Id);
            foreach (var processPointer in processPointers)
            {
                var windowText = Native.GetWindowText(processPointer);
                if (windowText.StartsWith("GDI+ Window") || windowText.Contains("IME")) continue;
                // if (windowText == "VRChat" || windowText.StartsWith("MelonLoader")) // Inverted Search to be Universal.

                if (windowText.StartsWith("MelonLoader"))
                {
                    if (IncludeMelonConsole.Value) 
                        Native.CachedWindowPointers.Add(processPointer);
                    continue;
                }
                Native.CachedWindowPointers.Add(processPointer);
            }
        }

        private static void TryRemoveConsolePointer()
        {
            foreach (var cachedWindowPointer in Native.CachedWindowPointers)
            {
                if (!Native.GetWindowText(cachedWindowPointer).StartsWith("MelonLoader")) continue;
                Native.CachedWindowPointers.Remove(cachedWindowPointer);
                return;
            }
        }

        /// <summary>
        /// The User is Advanced enough to know that hiding the application has consequences.
        /// Consequences are:
        ///     The MainWindowHandle will be null, preventing other mods / VRC itself from utilizing the Current Process' MainWindowHandle.
        ///     The Window is not able to normally be opened again.
        ///     The instance of VRC will still be running and can be seen in Ctrl + Shift + Esc (Task Manager).
        /// </summary>
        private static bool AdvancedUser;

        //Pointer is public in-case some mods need this.
        // ReSharper disable once MemberCanBePrivate.Global

        private static MelonPreferences_Entry<bool> Enabled;
        private static MelonPreferences_Entry<string> Enum;
        private static MelonPreferences_Entry<bool> IncludeMelonConsole;

        private enum nShowWindow //Manually resolving the enum is faster but this won't get called often.
        {
            SW_HIDE= 0, 
            SW_SHOWNORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            //We don't need the rest.
            // SW_SHOWNOACTIVATE = 4,
            // SW_SHOW = 5,
            // SW_MINIMIZE = 6,
            // SW_SHOWMINNOACTIVE = 7,
            // SW_SHOWNA = 8,
            // SW_RESTORE = 9,
            // SW_SHOWDEFAULT = 10,
            // SW_FORCEMINIMIZE = 11	
        }

        private static nShowWindow CachedWindowEnum;

        private static void SetWindowEnum(string enumVal)
        {
            CachedWindowEnum = System.Enum.TryParse<nShowWindow>(enumVal, out var value)
                ? value
                : nShowWindow.SW_SHOWNORMAL;
        }

        public override void OnApplicationStart()
        {
            CheckUser();
            RegisterPreferences();

            SetWindowEnum(Enum.Value);
            if (Enabled.Value) MelonCoroutines.Start(DelayByFrame(2, () => ShowWindow(CachedWindowEnum)));
                Enabled.OnValueChanged += (_, b1) => { ShowWindow(b1 ? CachedWindowEnum : nShowWindow.SW_SHOWNORMAL); };
            Enum.OnValueChanged += (_, s1) =>
            {
                if (!Enabled.Value) return; 
                SetWindowEnum(s1);
                ShowWindow(CachedWindowEnum);
            };
            IncludeMelonConsole.OnValueChanged += (_, b1) =>
            {
                if (b1 && Enabled.Value) ShowWindow(CachedWindowEnum);
                else TryRemoveConsolePointer();
            };

            if (MelonHandler.Mods.Any(m => m.Info.Name == "UI Expansion Kit")) 
                ExpansionKitStart();
        }

        private static IEnumerator DelayByFrame(int frameDelay, Action act)
        {
            for (int i = 0; i < frameDelay; i++)
            {
                yield return new WaitForEndOfFrame();
            }
            act();
        }

        /// <summary>
        /// TO FORCE A RESET:
        /// Close Game.
        /// Go to <Your-Game>\UserData\MelonPreferences.cfg
        /// Find: "[Resize]"
        /// Change "WindowState" to "SW_SHOWNORMAL".
        /// </summary>
        private static void CheckUser() => AdvancedUser = Environment.CommandLine.Contains("--Dawn.AdvancedResize");

        private const string MelonPrefsName = "Resize";
        private static void RegisterPreferences()
        {
            var cat = MelonPreferences.CreateCategory(MelonPrefsName);
            Enabled = cat.CreateEntry("Enabled", true, "Enabled", "Allows the changing of the Window Size");
            Enum = cat.CreateEntry("WindowState", nameof(nShowWindow.SW_SHOWNORMAL), "Window State", "Sets the Window State of VRChat");
            IncludeMelonConsole = cat.CreateEntry("IncludeMelonConsole", false, "Include MelonConsole",
                "Option to include the MelonConsole in the Resize search.");
        }
        private static void ExpansionKitStart()
        {
            var stringEnum = new List<(string SettingsValue, string DisplayName)>();
            if (AdvancedUser) 
                stringEnum.Add((nameof(nShowWindow.SW_HIDE), "Hide"));
            stringEnum.AddRange(new []
            {
                (nameof(nShowWindow.SW_SHOWNORMAL), "Normal"),
                (nameof(nShowWindow.SW_SHOWMAXIMIZED), "Maximize"),
                (nameof(nShowWindow.SW_SHOWMINIMIZED), "Minimize"),
            });

            UIExpansionKit.API.ExpansionKitApi.RegisterSettingAsStringEnum(MelonPrefsName, "WindowState", stringEnum);
        }
    }
}
