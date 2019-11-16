using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Log4Net.Extensions.Configuration.Exceptions;
using Log4Net.Extensions.Configuration.Implementation;
using Microsoft.Win32;

namespace LogReader.Log4Net
{
    /// <summary>
    /// A tailored <see cref="OpenFileDialog"/> to interact with Log4Net Config Files.
    /// </summary>
    public class Log4NetOpenFileDialog
    {
        /// <summary>
        /// The underlying <see cref="OpenFileDialog"/>.
        /// </summary>
        private readonly OpenFileDialog _openFileDialog;

        /// <summary>
        /// The result of the last <see cref="OpenFileDialog"/> interaction.
        /// </summary>
        private bool? _openFileDialogResult;
        
        /// <summary>
        /// Represents if the User selected Ok from the <see cref="OpenFileDialog"/>.
        /// </summary>
        public bool UserSelectedOk => _openFileDialogResult.HasValue && _openFileDialogResult.Value;

        /// <summary>
        /// Represents if the User selected Cancel from the <see cref="OpenFileDialog"/>.
        /// </summary>
        public bool UserSelectedCancel => _openFileDialogResult.HasValue && !_openFileDialogResult.Value;

        /// <summary>
        /// Represents if the Log4Net Config File was successfully loaded.
        /// </summary>
        public bool Log4NetConfigFileSuccessfullyLoaded { get; private set; }

        /// <summary>
        /// The Log4NetConfig loaded by the Dialog.
        /// </summary>
        public Log4NetConfig Log4NetConfig { get; private set; }

        public Log4NetOpenFileDialog()
        {
            _openFileDialog = new OpenFileDialog
            {
                FileName = "Select a Log4Net Config File",
                Filter = "Config files (*.config)|*.config",
                Title = "Open Log4Net Config",
                CheckFileExists = true,
                CheckPathExists = true
            };

            _openFileDialogResult = null;
        }

        /// <summary>
        /// Displays the pre-configured <see cref="OpenFileDialog"/> and processes it's results.
        /// </summary>
        public void ShowDialog()
        {
            _openFileDialogResult = _openFileDialog.ShowDialog();

            if (UserSelectedOk
                && !UserSelectedCancel)
            {
                ProcessSelectedLog4NetConfigFile(_openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Processes the selected Log4Net Config File and attempts to load it.
        /// </summary>
        /// <param name="selectedFileName"></param>
        private void ProcessSelectedLog4NetConfigFile(string selectedFileName)
        {
            Log4NetConfigFileSuccessfullyLoaded = false;

            try
            {
                Log4NetConfig = Log4NetConfigLoader.CreateLog4NetConfig(selectedFileName);
            }
            catch (InvalidLog4NetConfigAttributeValueException e)
            {
                ShowErrorMessage("An error occured processing a value within the file.", "File Content Error", e);
            }
            catch (InvalidLog4NetConfigStructureException e)
            {
                ShowErrorMessage("An error occured while reading from the selected file.", "File Structure Error", e);
            }
            catch (Log4NetConfigFileAccessErrorException e)
            {
                ShowErrorMessage("An error occured while attempting to access the selected file.", 
                    "File Access Error", e);
            }
            catch (Exception e)
            {
                ShowErrorMessage("An unknown error occured while attempting to process the selected file.", 
                    "Unknown Error", e);
            }

            Log4NetConfigFileSuccessfullyLoaded = true;
        }

        /// <summary>
        /// Shows an on-screen error message box to the user.
        /// </summary>
        /// <param name="messageText">The body of the error message.</param>
        /// <param name="messageBoxCaption">The title text of the message box.</param>
        private void ShowErrorMessage(string messageText, string messageBoxCaption, Exception e = null)
        {
            string errorMessage = messageText;
            if (null != e)
            {
                errorMessage += $" The error message is:{Environment.NewLine}{e.Message}";
            }

            MessageBox.Show(errorMessage, messageBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
