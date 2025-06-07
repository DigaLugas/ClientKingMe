// ============================
// File: ErrorHandling.cs (refatorado)
// ============================
using System;
using System.Windows.Forms;

namespace ClientKingMe
{
    public static class ErrorHandler
    {
        public static bool HandleServerResponse(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                Show("Erro: Resposta do servidor está vazia.", "Erro", MessageBoxIcon.Error);
                return true;
            }

            if (response.StartsWith("ERRO"))
            {
                Show(response, "Erro", MessageBoxIcon.Error);
                return true;
            }

            return false;
        }

        public static void ShowError(string message) => Show(message, "Erro", MessageBoxIcon.Error);

        public static void ShowWarning(string message) => Show(message, "Aviso", MessageBoxIcon.Warning);

        public static void ShowInfo(string message) => Show(message, "Informação", MessageBoxIcon.Information);

        private static void Show(string message, string title, MessageBoxIcon icon)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }
    }
}
