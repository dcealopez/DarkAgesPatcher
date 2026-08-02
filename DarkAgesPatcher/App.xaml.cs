using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace DarkAgesPatcher
{
    /// <summary>
    /// Main WPF Application class
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Fired on startup
        /// </summary>
        /// <param name="e">startup event args</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Used for command line option parsing
            bool performUpdate = false;
            string filePath = string.Empty;

            // Parse command line arguments
            if (e.Args != null & e.Args.Length > 0)
            {
                for (var i = 0; i < e.Args.Length; i++)
                {
                    if (e.Args[i].Equals("--patch", StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrEmpty(filePath))
                    {
                        if (i + 1 < e.Args.Length)
                        {
                            filePath = e.Args[i + 1];
                            continue;
                        }
                    }
                    else if (e.Args[i].Equals("--update", StringComparison.InvariantCultureIgnoreCase) && !performUpdate)
                    {
                        performUpdate = true;
                        continue;
                    }
                }
            }

            // Only show the UI when not using the command-line version
            if (string.IsNullOrEmpty(filePath) && !performUpdate)
            {
                var window = new MainWindow();
                window.ShowDialog();
            }
            else
            {
                // Allocate a console
                AllocConsole();

                // Update first if required
                try
                {
                    if (performUpdate)
                    {
                        Console.WriteLine("Checking for updates...");

                        if (Patcher.AnyUpdateAvailable())
                        {
                            Console.WriteLine("Downloading latest patch definitions...");
                            Patcher.DownloadLatestPatchDefinitions();
                            Console.WriteLine("Done.");
                        }
                        else
                        {
                            Console.WriteLine("No updates available.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occured while checking for updates: {ex}");
                    Application.Current.Shutdown(Util.BuildExitCode(1, 0, 0));
                    return;
                }

                // Stop here if no path was specified
                if (string.IsNullOrEmpty(filePath))
                {
                    Console.Error.WriteLine($"No executable to patch was given. Specify its path using the --patch <path> argument.");
                    Application.Current.Shutdown(Util.BuildExitCode(2, 0, 0));
                    return;
                }

                // Load the patch definitions file
                try
                {
                    Console.WriteLine("Loading patch definitions file...");
                    Patcher.LoadPatchDefinitions();
                    Console.WriteLine("Done.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occured while loading the patch definitions file: {ex}");
                    Application.Current.Shutdown(Util.BuildExitCode(3, 0, 0));
                    return;
                }

                // Stop if there are no patches loaded
                if (!Patcher.AnyPatchesLoaded())
                {
                    Console.Out.WriteLine($"Unable to patch: 0 patches loaded");
                    Application.Current.Shutdown(Util.BuildExitCode(3, 0, 0));
                    return;
                }

                // Check game build
                GameBuild gameBuild = null;

                try
                {
                    Console.WriteLine("Checking game build...");
                    gameBuild = Patcher.GetGameBuild(filePath);

                    if (gameBuild == null)
                    {
                        Console.Out.WriteLine($"Unable to apply patches: unsupported game build detected");
                        Application.Current.Shutdown(Util.BuildExitCode(4, 0, 0));
                        return;
                    }

                    Console.WriteLine($"{gameBuild.Id} detected.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occured while checking the game build: {ex}");
                    Application.Current.Shutdown(Util.BuildExitCode(5, 0, 0));
                    return;
                }

                if (gameBuild.PatchGroupIds.Contains("__patched_exe__"))
                {
                    Console.Out.WriteLine($"The executable is already patched");
                    Application.Current.Shutdown(Util.BuildExitCode(6, 0, 0));
                    return;
                }

                // Patch the specified file
                int successes = 0;

                try
                {
                    Console.WriteLine("Applying patches...");

                    foreach (var patchResult in Patcher.ApplyPatches(filePath, gameBuild.Patches))
                    {
                        if (patchResult.Success)
                        {
                            successes++;
                        }

                        Console.WriteLine($"{patchResult.Patch.Description} : {(patchResult.Success ? "Success" : "Failure")}");
                    }

                    Console.WriteLine($"\n{successes} out of {gameBuild.Patches.Count} applied.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occured while patching the game executable: {ex}");
                    Application.Current.Shutdown(Util.BuildExitCode(7, (byte)successes, (byte)(gameBuild.Patches.Count - successes)));
                    return;
                }

                Application.Current.Shutdown(Util.BuildExitCode(0, (byte)successes, (byte)(gameBuild.Patches.Count - successes)));
            }
        }

        /// <summary>
        /// Allocates a console for the current process
        /// </summary>
        /// <returns>true if successful, false when not</returns>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool AllocConsole();
    }
}
