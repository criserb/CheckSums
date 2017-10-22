using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace ST1_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum Algorithm
        {
            parity,
            modulo,
            crc
        }

        public byte[] FileData { get; set; }
        public byte[] ToSaveCrc { get; set; }
        public byte ToSaveOthers { get; set; }
        public Algorithm Choice { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Otwieranie pliku
        /// </summary>
        private void OpenFile()
        {
            FileData = null;
            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog
            {
                // Set initial directory
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            // if (result == true)
            // {
            // Open document
            //}
            try
            {
                using (FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        FileData = binaryReader.ReadBytes((int)fileStream.Length);
                    }
                }
                AppendToTextBox("Pomyślnie otwarto plik o nazwie: " + dlg.SafeFileName);
                AppendToTextBox("Rozmiar pliku: " + FileData.Length + " bajtów");
                lbl_file_in_memory.Content = dlg.SafeFileName;
                MakeButtonsVisible();
            }
            catch (Exception ex)
            {
                AppendToTextBox(ex.Message);
            }
        }
        /// <summary>
        /// Dodawanie wpisów do textboxa (wyświetlanie informacji o aktualnych operacjach w programie)
        /// </summary>
        /// <param name="s"> Wiadomość </param>
        protected void AppendToTextBox(string s)
        {
            textBox.Text += $"{DateTime.Now.ToLongTimeString()}: {s} \n";
        }
        /// <summary>
        /// Asynchroniczny zapis danych do pliku
        /// </summary>
        private async void Button_save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                lbl_progress.Content = "Processing. Please wait . . .";
                using (FileStream fileStream = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate))
                {
                    await fileStream.WriteAsync(FileData, 0, FileData.Length);
                    switch (Choice)
                    {
                        case Algorithm.parity:
                            fileStream.WriteByte(ToSaveOthers);
                            break;
                        case Algorithm.modulo:
                            fileStream.WriteByte(ToSaveOthers);
                            break;
                        case Algorithm.crc:
                            await fileStream.WriteAsync(ToSaveCrc, 0, ToSaveCrc.Length);
                            break;
                        default:
                            break;
                    }
                }
            }
            lbl_progress.Content = String.Empty;
            AppendToTextBox("Pomyślnie zapisano plik o nazwie: " + saveFileDialog.SafeFileName);
        }
        /// <summary>
        /// Włączenie widoczności przycisków do wyboru algorytmu
        /// </summary>
        private void MakeButtonsVisible()
        {
            button_parity.IsEnabled = true;
            button_crc.IsEnabled = true;
            button_modulo.IsEnabled = true;
            button_save.IsEnabled = true;
            button_makeError.IsEnabled = true;
        }
        /// <summary>
        /// Przycisk odpowiadający za zaburzanie błędów
        /// </summary>
        private async void Button_makeError_Click(object sender, RoutedEventArgs e)
        {
            // Ilość błędów (ile bitów będzie podlegało zmianie)
            int errors;
            // Otwarcie dialogu do wpisania błędu w procentach
            string s = Microsoft.VisualBasic.Interaction.InputBox("Wpisz ilość błędów ( 0 < błąd <= 100 ) [%]",
                                           "Wybierz błąd",
                                           "0,01",
                                           -1, -1);
            // Zmienna przechowująca próbę konwersji wpisanego tekstu do zmiennej double, która z kolei potrzebna
            //  jest do wyliczenia wartości procentowej
            bool result = Double.TryParse(s, out double percentError);
            if (result && (percentError > 0 && percentError <= 100))
            {
                // Błąd w procentach
                percentError /= 100;
                AppendToTextBox("Błąd: " + percentError * 100 + '%');

                errors = Convert.ToInt32(percentError * FileData.Length);
                // Otwarcie dialogu wyboru losowania błędów z powtórzeniami lub bez powtórzeń
                if (MessageBox.Show("Losowanie błędów z powtórzeniami? ",
                     "Losowanie błędów", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    lbl_progress.Content = "Proszę czekać. Obliczam . . .";
                    FileData = await MakeError.WRepeats(FileData, errors);
                }
                else
                {
                    lbl_progress.Content = "Proszę czekać. Obliczam . . .";
                    FileData = await MakeError.WNRepeats(FileData, errors);
                }
                lbl_progress.Content = String.Empty;
                AppendToTextBox("Operacja zakończona sukcesem!");
            }
            else
                AppendToTextBox("Błąd podczas wpisywania wartości procentowej błędu! Spróbuj jeszcze raz!");
        }

        private async void Button_parity_Click(object sender, RoutedEventArgs e)
        {
            lbl_progress.Content = "Proszę czekać. Obliczam . . .";
            int sum = await ParityBit.Check(FileData);
            lbl_progress.Content = String.Empty;
            ToSaveOthers = Convert.ToByte(sum % 2);
            AppendToTextBox($"Operacja zakończona sukcesem! Ilość jedynek w pliku: {sum}. " +
           $"Bit parzystości: {sum % 2}");
            Choice = Algorithm.parity;
        }

        private async void Button_modulo_Click(object sender, RoutedEventArgs e)
        {
            lbl_progress.Content = "Proszę czekać. Obliczam . . .";
            ToSaveOthers = await SumModulo.Check(FileData);
            lbl_progress.Content = String.Empty;
            AppendToTextBox($"Operacja zakończona sukcesem! Końcowy bajt: {ToSaveOthers}. " +
                $"Binarnie: {Convert.ToString(ToSaveOthers,2)}");
            Choice = Algorithm.modulo;
        }

        private async void Button_crc_Click(object sender, RoutedEventArgs e)
        {
            lbl_progress.Content = "Proszę czekać. Obliczam . . .";
            ToSaveCrc = await Crc.Check(FileData);
            lbl_progress.Content = String.Empty;
            AppendToTextBox($"Operacja zakończona sukcesem! Końcowy bajt: {ToSaveOthers}. " +
                $"Hex: {Convert.ToString(ToSaveOthers, 16)}");
            Choice = Algorithm.crc;
        }

        private void Button_open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }
    }
}
