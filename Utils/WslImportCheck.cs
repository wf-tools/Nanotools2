using System.Windows;
namespace NanoTools2.Utils
{
    public static class WslImportCheck
    {

        public static bool IsNoInitialSettings()
        {
            // wsl 
            if (!WfComponent.CommandUtils.IsWslExist() ||
                !WfComponent.CommandUtils.IsUbuntuImported())
            {
                string text = "not setting WSL（Windows Subsystem for Linux）";
                string caption = "no wsl worning! ";

                var installer = System.IO.Path.Combine(
                                        System.AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                                        "WfComponentInstaller.exe");

                MessageBox.Show(text, caption + installer, MessageBoxButton.OK, MessageBoxImage.Warning);

                if (System.IO.File.Exists(installer))
                {
                    text += System.Environment.NewLine + "Do you start setup program? ";
                    // Display message box
                    if (MessageBoxResult.Yes.Equals(
                                        MessageBox.Show(text, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning)))
                    {
                        System.Diagnostics.Process.Start(installer);
                    }
                }
                else
                { // ファイルコピーし忘れた。
                  // Display message box
                    caption += " but not installer";
                    text += System.Environment.NewLine + "Please, contact support team." +
                                System.Environment.NewLine + installer;
                    MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                // WSLが無かった時点で。
                return true;
            }

            return false;
        }

    }
}
