using System.Windows.Forms;

namespace ClientKingMe
{
    public static class ErrorHandler
    {
        public static bool HandleServerResponse(string response)
        {
            if (response.Contains("ERRO"))
            {
                ShowError(response);
                return true;
            }
            return false;
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}