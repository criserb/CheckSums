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
        // Algorytmy do wyboru (potrzebne przy zapisywaniu do pliku)
        public enum Algorithm
        {
            parity,
            modulo,
            crc
        }

        public byte[] FileData { get; set; }
        public byte[] ToSaveCrc { get; set; }
        public byte ToSaveOthers { get; set; }
        public Algorithm? Choice { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }
        // Otwieranie pliku
        private void OpenFile()
        {
            // Configure open file dialog box
            OpenFileDialog dlg = new OpenFileDialog
            {
                // Ustawiania katalogu inicjalizującego
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            // Pokazanie dialogu wyboru pliku
            bool? result = dlg.ShowDialog();

            try
            {
                using (FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        FileData = binaryReader.ReadBytes((int)fileStream.Length);
                    }
                }
                AppendToConsole("Pomyślnie otwarto plik o nazwie: " + dlg.SafeFileName);
                AppendToConsole("Rozmiar pliku: " + FileData.Length + " bajtów");
                lbl_file_in_memory.Content = dlg.SafeFileName;
                MakeButtonsVisible();
            }
            catch (Exception ex)
            {
                AppendToConsole(ex.Message);
            }
        }
        /// <summary>
        /// Dodawanie wpisów do konsoli textBox (wyświetlanie informacji o aktualnych operacjach w programie)
        /// </summary>
        /// <param name="s"> Wiadomość </param>
        protected void AppendToConsole(string s)
        {
            textBoxConsole.Text += $"{DateTime.Now.ToLongTimeString()}: {s} \n";
        }
        /// <summary>
        /// Asynchroniczny zapis danych do pliku
        /// </summary>
        private async void Button_save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                // Ustawianie filtrów zapisu plików (będzie do wyboru zapis do .txt i dowolnego formatu)
                Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*"
            };
            // Jeśli stworzymy plik do zapisu, czyli klikniemy 'zapisz'
            if (saveFileDialog.ShowDialog() == true)
            {
                lblProgress.Content = "Processing. Please wait . . .";
                using (FileStream fileStream = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate))
                {
                    // Zapisywanie ciągu danych do pliku
                    await fileStream.WriteAsync(FileData, 0, FileData.Length);
                    // Zapisywanie wyniku danej operacji
                    switch (Choice)
                    {
                        case Algorithm.parity:
                        case Algorithm.modulo:
                            fileStream.WriteByte(ToSaveOthers);
                            break;
                        case Algorithm.crc:
                            await fileStream.WriteAsync(ToSaveCrc, 0, ToSaveCrc.Length);
                            break;
                        default:
                            break;
                    }
                    Choice = null;
                }
            }
            lblProgress.Content = String.Empty;
            AppendToConsole("Pomyślnie zapisano plik o nazwie: " + saveFileDialog.SafeFileName);
        }

        // Włączenie widoczności przycisków do wyboru algorytmu po wczytaniu pliku
        private void MakeButtonsVisible()
        {
            buttonParity.IsEnabled = true;
            buttonCrc.IsEnabled = true;
            buttonModulo.IsEnabled = true;
            buttonSave.IsEnabled = true;
            buttonMakeError.IsEnabled = true;
        }

        // Obliczanie bitu parzystości
        private async void Button_parity_Click(object sender, RoutedEventArgs e)
        {
            lblProgress.Content = "Proszę czekać. Obliczam . . .";
            int sum = await ParityBit.Check(FileData);
            lblProgress.Content = String.Empty;
            ToSaveOthers = Convert.ToByte(sum % 2);
            AppendToConsole($"Operacja zakończona sukcesem! Ilość jedynek w pliku: {sum}. " +
           $"Bit parzystości: {sum % 2}");
            Choice = Algorithm.parity;
        }

        // Obliczanie sumy modulo
        private async void Button_modulo_Click(object sender, RoutedEventArgs e)
        {
            lblProgress.Content = "Proszę czekać. Obliczam . . .";
            ToSaveOthers = await SumModulo.Check(FileData);
            lblProgress.Content = String.Empty;
            AppendToConsole($"Operacja zakończona sukcesem! Końcowy bajt: {ToSaveOthers}. " +
                $"Binarnie: {Convert.ToString(ToSaveOthers, 2)}");
            Choice = Algorithm.modulo;
        }

        // Przycisk włączający menu do zaburzania błędów
        private void Button_makeError_Click(object sender, RoutedEventArgs e)
        {
            Operations.Visibility = Visibility.Hidden;
            Errors.Visibility = Visibility.Visible;
        }

        private async void Button_continue_error_Click(object sender, RoutedEventArgs e)
        {
            // Ilość błędów (ile bitów będzie podlegało zmianie)
            int errors;
            // Otwarcie dialogu do wpisania błędu w procentach
            string s = textBoxError.Text;
            // Zmienna przechowująca próbę konwersji wpisanego tekstu do zmiennej double, która z kolei potrzebna
            //  jest do wyliczenia wartości procentowej
            bool result = Double.TryParse(s, out double percentError);
            if (result && (percentError > 0 && percentError <= 100))
            {
                // Błąd w procentach
                percentError /= 100;

                errors = Convert.ToInt32(percentError * FileData.Length);
                // Wybór błędów z powtórzeniami lub bez powtórzeń
                if (radio_button_repeats.IsChecked == true)
                {
                    lblProgress.Content = "Proszę czekać. Obliczam . . .";
                    // Obliczanie błędów z powtórzeniami
                    FileData = await MakeError.WRepeats(FileData, errors);
                }
                else
                {
                    lblProgress.Content = "Proszę czekać. Obliczam . . .";
                    // Obliczanie błędów bez powtórzeń
                    FileData = await MakeError.WNRepeats(FileData, errors);
                }
                Operations.Visibility = Visibility.Visible;
                Errors.Visibility = Visibility.Hidden;
                lblProgress.Content = String.Empty;
                AppendToConsole("Błąd: " + percentError * 100 + '%' + ". Operacja zakończona sukcesem!");
            }
            else
                AppendToConsole("Błąd podczas wpisywania wartości procentowej błędu! Spróbuj jeszcze raz!");
        }

        private void Button_back_error_Click(object sender, RoutedEventArgs e)
        {
            Operations.Visibility = Visibility.Visible;
            Errors.Visibility = Visibility.Hidden;
        }

        private UInt64 BinToDec(string s)
        {
            int j = 0;
            UInt64 dec = 0;
            for (int i = textBoxPolynomial.Text.Length - 1; i >= 0; i--)
            {
                if (textBoxPolynomial.Text[i] == '1')
                {
                    dec += (UInt64)Math.Pow(2, j);
                }
                j++;
            }
            return dec;
        }

        private async void Button_continue_crc_Click(object sender, RoutedEventArgs e)
        {
            CrcGrid.Visibility = Visibility.Hidden;
            AlgorithmGrid.Visibility = Visibility.Visible;
            string binPol = textBoxPolynomial.Text;
            UInt64 pol = BinToDec(binPol);

            AppendToConsole("BinToDecPol: " + pol.ToString());

            // zeby zapisac UInt64 do byte[]

            lblProgress.Content = "Proszę czekać. Obliczam . . .";
            UInt64 crc = await Crc.Check(FileData, pol);
            // Długość ciągu binarnego ostatnie bajtu
            int lastByteLength = 8 - (Convert.ToString(FileData[FileData.Length - 1], 2).Length);
            // Długość całego ciągu danych (binarnie)
            int dataLength = (FileData.Length * 8) - lastByteLength;

            // znalezc zwiazek ile razy sie przesunie
            AppendToConsole($"Długość FileData: {dataLength}. " +
                $"Długość wielomanu: {binPol.Length}");
            AppendToConsole($"Ile razy się przesunie: {FileData.Length * 8 - binPol.Length}");

            foreach (var item in FileData)
            {
                AppendToConsole(Convert.ToString(item, 2));
            }

            lblProgress.Content = String.Empty;
            AppendToConsole($"Operacja zakończona sukcesem! Crc wynosi: {crc}. Binarnie: {Convert.ToString((long)crc, 2)}." +
                $" Hexadecymalnie: {Convert.ToString((long)crc, 16)}");
            Choice = Algorithm.crc;
        }

        private void Button_back_crc_Click(object sender, RoutedEventArgs e)
        {
            CrcGrid.Visibility = Visibility.Hidden;
            AlgorithmGrid.Visibility = Visibility.Visible;
        }

        // Włączenie menu do wpisania wielomianu crc
        private void Button_crc_Click(object sender, RoutedEventArgs e)
        {
            CrcGrid.Visibility = Visibility.Visible;
            AlgorithmGrid.Visibility = Visibility.Hidden;
        }

        // Otwarcie pliku
        private void Button_open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }
    }
}
