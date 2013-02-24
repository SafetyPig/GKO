using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using System.IO;
using System.Threading;

namespace MediaPlayer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Huom, saattaa antaa varoituksia siitä, että ei löydä dynanic resourceja.
    /// Ne asetetaan vasta ohjelman käynnityessä.
    /// </summary>
    public partial class MainWindow : Window
    {
        private ResourceDictionary dict;
        private bool suomi = true;
        private Soittolista valittuSoittolista;
        private List<Soittolista> listaSoittoListoista = new List<Soittolista>();
        private bool muokattu = false;
        private int numeroLadatessa = 0;
        private Kappale valittuKappale;
        private string vanhaNimi;
        private int virheNumero = 0;
        private int kasvavaNumero = 0;
        /// <summary>
        /// Ohjelman pääikkuna
        /// </summary>
        public MainWindow()
        {
            AsetaKieli("fi-FI");
            InitializeComponent();
            haeTallennetutSoittolistat();
        }

        /// <summary>
        /// Skaalaa kappale listboxin sekä soittolista listboxin sopivan kokoisiksi.
        /// </summary>
        private void Ikkuna_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListBoxKappaleet.MaxHeight = Ikkuna.ActualHeight - 270;
            ListBoxSoittolistat.MaxHeight = Ikkuna.ActualHeight - 270;
        }

        /// <summary>
        /// Asettaa ohjelman kielen annetuksi. Vaihtoehtoina suomi ja englanti
        /// </summary>
        private void AsetaKieli(string kieli)
        {
            dict = new ResourceDictionary();
            switch (kieli)
            {
                case "en-US":
                    dict.Source = new Uri("..\\Resources\\StringResources.en-US.xaml",
                                  UriKind.Relative);
                    suomi = false;
                    break;

                default:
                    dict.Source = new Uri("..\\Resources\\StringResources.xaml",
                                      UriKind.Relative);
                    suomi = true;
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }

        /// <summary>
        /// Vaihtaa ohjelman kielen
        /// </summary>
        private void MenuItemVaihdaKieli_Click(object sender, RoutedEventArgs e)
        {
            if (suomi)
                AsetaKieli("en-US");

            else
                AsetaKieli("fi-FI");
        }

        /// <summary>
        /// Kutsuu tallenna soittolistat metodia ja asettaa muokkauksen falseksi
        /// </summary>
        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            tallennaSoittolistat();
            muokattu = false;
        }

        /// <summary>
        /// Näyttää dialogin, joka kertoo käyttäjän versionnumeron yms.
        /// </summary>
        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder about = new StringBuilder();
            about.Append((string)FindResource("ohjelma"));
            about.Append(": MediaPlayer \n");
            about.Append((string)FindResource("tekija"));
            about.Append(": Hannu Viinikainen \n");
            about.Append((string)FindResource("versio"));
            about.Append(": 1.0.0 \n");
            about.Append((string)FindResource("paivamaara"));
            about.Append(": 5.12.2012");

            MessageBox.Show(about.ToString());
        }

        /// <summary>
        /// Näyttää avustuksen. Jos löytää tiedoston readme.txt tai lueminut.txt niin avaa
        /// sen, jos ei näyttää oikein suppean dialogin.
        /// </summary>
        private void MenuItemHelp_Click(object sender, RoutedEventArgs e)
        {
            string kieli = Environment.CurrentDirectory + "\\" + (string)FindResource("kieli") + ".txt";

            try
            {
                System.Diagnostics.Process.Start(kieli);
            }

            catch
            {
                MessageBox.Show("Ohjelma on mediasoitin, jolla voi myös leikata mp3 kappaleita");
            }

        }

        /// <summary>
        /// Lopettaa musiikin soittamisen sekä asettaa tarvittaessa Muokkaa menun käyttöön
        /// </summary>
        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
                MenuItemMuokkaa.IsEnabled = true;
            else
                MenuItemMuokkaa.IsEnabled = false;

            soitin.lopetaSoitto();

        }


        /// <summary>
        /// Jos ikkuna on menossa kiinni ja muutoksia on tehty soittolistoihin, kysytään
        /// halutaanko tehdyt muutokset tallentaa
        /// </summary>
        private void Ikkuna_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (muokattu)
            {
                MessageBoxResult tulos = MessageBox.Show("Haluatko tallentaa soittolistat", "Sulkeminen", MessageBoxButton.YesNoCancel);
                switch (tulos)
                {
                    case MessageBoxResult.Yes:
                        tallennaSoittolistat();
                        break;

                    case MessageBoxResult.No:
                        break;

                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Tallentaa soittolistat tiedostoiksi.
        /// Tallennetaan sijaintiin jossa ohjelma ajetaan.
        /// Luo tiedoston soittolistat.txt sinne.
        /// </summary>
        public void tallennaSoittolistat()
        {
            using (StreamWriter tiedosto = new StreamWriter(Environment.CurrentDirectory + "\\soittolistat.txt", false))
            {
                for (int i = 0; i < listaSoittoListoista.Count; i++)
                    tiedosto.WriteLine(listaSoittoListoista[i].tallennettavaMuoto());
            }
        }

        /// <summary>
        /// Hakee tallennetut soittolistat
        /// Etsitään ajosijainnissa tiedostoa soittolistat.txt
        /// </summary>
        public void haeTallennetutSoittolistat()
        {
            try
            {
                using (StreamReader lukija = new StreamReader(Environment.CurrentDirectory + "\\soittolistat.txt"))
                {
                    string soittolistaString;
                    while ((soittolistaString = lukija.ReadLine()) != null)
                    {
                        Soittolista uusiLista = new Soittolista(soittolistaString);
                        listaSoittoListoista.Add(uusiLista);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                //Miettiä tarvitseeko käyttäjälle ilmaista, jos lataaminen epäonnistui.
                //Suora ilmoittaminen huono idea, koska ensimmäisellä kerralla ei kannata ilmoittaa.
            }

            paivitaListat();
            paivitaKappaleet();
        }

        #region Musiikki

        /// <summary>
        /// Avaa soittolistan tekstikentän nimen muokkaamista varten
        /// </summary>
        private void Soittolista_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox lahettaja = (TextBox)sender;
            lahettaja.IsReadOnly = false;
            lahettaja.Background = Brushes.LightBlue;
            muokattu = true;
        }

        /// <summary>
        /// Asettaa uudet tiedot oikeiseiin kenttiin. Tarkistaa myös tietojen oikeellisuuden
        /// Kahta samannimistä soittolistaa ei saa olla, eikä soittlistat saa sisältää merkkiä
        /// '|', koska sitä käytetään tallennuksessa apuna.
        /// </summary>
        private void Soittolista_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox lahettaja = (TextBox)sender;
            lahettaja.Background = Brushes.White;
            lahettaja.IsReadOnly = true;

            if (lahettaja.Text.Equals(vanhaNimi))
                return;

            if (lahettaja.Text.Contains("|"))
                lahettaja.Text = lahettaja.Text.Replace("|", "");


            for (int i = 0; i < listaSoittoListoista.Count; i++)
            {
                if (lahettaja.Text.Equals(listaSoittoListoista[i].Nimi))
                {
                    lahettaja.Text = lahettaja.Text + ++virheNumero;
                }
            }

            valittuSoittolista.Nimi = lahettaja.Text;
            LabelSoittolista.Content = lahettaja.Text;
           
        }

        /// <summary>
        /// Vaihtaa soittolistan valittuun
        /// </summary>
        private void Soittolista_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox lahettaja = (TextBox)sender;
            lahettaja.Background = Brushes.Aqua;
            LabelSoittolista.Content = lahettaja.Text;
            vanhaNimi = lahettaja.Text;
            asetaSoittolista(lahettaja.Text);
            paivitaKappaleet();

            soitin.vaihdaPlay();
        }

        /// <summary>
        /// Asettaa soittolistalle oikean nimen
        /// </summary>
        private void Soittolista_Loaded(object sender, RoutedEventArgs e)
        {

            if (numeroLadatessa < listaSoittoListoista.Count)
            {
                TextBox lahettaja = (TextBox)sender;
                lahettaja.Text = listaSoittoListoista[numeroLadatessa].Nimi;
                numeroLadatessa++;
            }
        }


        /// <summary>
        /// Luo listan ja asettaa sen valituksi.
        /// </summary>
        private void MenuItemLuoLista_Click(object sender, RoutedEventArgs e)
        {
            Soittolista uusiSoittolista = new Soittolista();
            uusiSoittolista.Nimi = "Uusi Soittolista" + ++kasvavaNumero;
            valittuSoittolista = uusiSoittolista;
            ListBoxSoittolistat.Items.Add(uusiSoittolista);
            listaSoittoListoista.Add(uusiSoittolista);

            MenuItemPoistaLista.IsEnabled = true;
            MenuItemLisaaKappale.IsEnabled = true;

            muokattu = true;
        }

        /// <summary>
        /// Lisää kappaleen soittolistalle jollei se jo valmiiksi ole siellä
        /// </summary>
        private void MenuItemLisaaKappale_Click(object sender, RoutedEventArgs e)
        {
            if (valittuSoittolista == null)
                return;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Music Files (*.mp3, *.m4a, *.wma)|*.mp3;*.m4a;*.wma";
            dialog.ShowDialog();

            if (dialog.FileName == "")
                return;
            for (int i = 0; i < valittuSoittolista.getKappaleidenMaara(); i++)
            {
                if (valittuSoittolista.getKappale(i).TiedostoSijainti.Equals(dialog.FileName))
                    return;
            }

            valittuSoittolista.lisaa(new Kappale(dialog.FileName));
            paivitaKappaleet();
            muokattu = true;
            MenuItemPoistaKappale.IsEnabled = true;
        }

        /// <summary>
        /// Päivittää soittolistat
        /// </summary>
        private void paivitaListat()
        {
            for (int i = 0; i < listaSoittoListoista.Count; i++)
            {
                ListBoxSoittolistat.Items.Add(listaSoittoListoista[i]);
            }

            if (listaSoittoListoista.Count > 0)
            {
                valittuSoittolista = listaSoittoListoista[0];
                MenuItemPoistaLista.IsEnabled = true;
                MenuItemLisaaKappale.IsEnabled = true;
            }
        }

        /// <summary>
        /// Päivittää kappale listauksen
        /// </summary>
        private void paivitaKappaleet()
        {
            if (valittuSoittolista == null)
            {
                LabelSoittolista.Content = "";
                ListBoxKappaleet.Items.Clear();
                MenuItemPoistaKappale.IsEnabled = false;
                return;
            }

            valittuSoittolista.jarjestaAakkos();
            ListBoxKappaleet.Items.Clear();

            for (int i = 0; i < valittuSoittolista.getKappaleidenMaara(); i++)
            {
                ListBoxKappaleet.Items.Add(valittuSoittolista.getKappale(i));
            }
            soitin.luoSoittolista(valittuSoittolista);

            if (valittuSoittolista.getKappaleidenMaara() > 0)
                MenuItemPoistaKappale.IsEnabled = true;

            else
                MenuItemPoistaKappale.IsEnabled = false;
        }

        /// <summary>
        /// Asettaa parametrina tuodun soittolistan nimen mukaisen soittolistan
        /// valituksi soittolistaksi
        /// </summary>
        /// <param name="soittolistanNimi">Soittolistan nimi, joka halutaan valituksi</param>
        private void asetaSoittolista(string soittolistanNimi)
        {
            for (int i = 0; i < listaSoittoListoista.Count; i++)
            {
                if (listaSoittoListoista[i].Nimi.Equals(soittolistanNimi))
                {
                    valittuSoittolista = listaSoittoListoista[i];
                    break;
                }
            }
        }

        /// <summary>
        /// Asettaa näpätyn kappaleen soimaan
        /// </summary>
        private void Kappale_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            for (int i = 0; i < valittuSoittolista.getKappaleidenMaara(); i++)
            {
                ListBoxItem valittu = (ListBoxItem)sender;
                if (valittu.Content.ToString().Contains(valittuSoittolista.getKappale(i).Nimi))
                {
                    soitin.asetaSoitettavaKappale(valittuSoittolista.getKappale(i), true);
                    soitin.vaihdaPause();
                    break;
                }

            }
        }

        /// <summary>
        /// Sulkemisnappulan painaminen
        /// </summary>
        private void MenuItemClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Poistaa valitun listan
        /// </summary>
        private void MenuItemPoistaLista_Click(object sender, RoutedEventArgs e)
        {
            ListBoxSoittolistat.Items.Remove(valittuSoittolista);
            listaSoittoListoista.Remove(valittuSoittolista);

            if (listaSoittoListoista.Count > 0)
            {
                valittuSoittolista = (Soittolista)ListBoxSoittolistat.Items[0];
                LabelSoittolista.Content = valittuSoittolista.Nimi;
            }

            else
            {
                valittuSoittolista = null;
                MenuItemPoistaLista.IsEnabled = false;
                MenuItemLisaaKappale.IsEnabled = false;
            }

            paivitaKappaleet();
            muokattu = true;
        }

        /// <summary>
        /// Poistaa valitun kappaleen soittolistalta
        /// </summary>
        private void MenuItemPoistaKappale_Click(object sender, RoutedEventArgs e)
        {
            valittuSoittolista.poista(valittuKappale);
            paivitaKappaleet();
            muokattu = true;

            if (valittuSoittolista.getKappaleidenMaara() == 0)
                MenuItemPoistaKappale.IsEnabled = false;
        }

        /// <summary>
        /// Kappale valinnan vaihtuessa asetetaan valittu kappale attribuutiksi
        /// </summary>
        private void ListBoxKappaleet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lahettaja = (ListBox)sender;
            valittuKappale = (Kappale)lahettaja.SelectedItem;
        }

        /// <summary>
        /// Kun painetaan enter, niin listbox menettään focuksen
        /// </summary>
        /// <param name="e">Tapahtuman argumentit</param>
        private void ListBoxSoittolistat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tabItemMusiikki.Focus();
            }
        }

        #endregion

        #region Peli

        private double pisteet;
        private int kysymysNumero = 0;
        private Soittolista pelinSoittolista;
        private Random r = new Random();

        /// <summary>
        /// Poistaa vanhat objektit ja asettaa kuuntelijan, kun ladataan
        /// </summary>
        private void stackPanelPeliAlusta_Loaded(object sender, RoutedEventArgs e)
        {
            stackPanelPeliAlusta.Children.Clear();
            naytaSoittolistat();

            stackPanelPeliAlusta.AddHandler(VastausLomake.ValintaEvent, new RoutedEventHandler(stackPanelPeliAlusta_Valinta));
        }

        /// <summary>
        /// Asettaa soittolistat näkyviin
        /// </summary>
        private void naytaSoittolistat()
        {
            Label aloitusTeksti = new Label();
            aloitusTeksti.Content = FindResource("valinta");
            stackPanelPeliAlusta.Children.Add(aloitusTeksti);
            WrapPanel wrappi = new WrapPanel();
            wrappi.MaxHeight = 200;
            stackPanelPeliAlusta.Children.Add(wrappi);
            for (int i = 0; i < listaSoittoListoista.Count; i++)
            {
                RadioButton soittolista = new RadioButton();
                soittolista.Margin = new Thickness(5, 5, 5, 5);
                soittolista.Content = listaSoittoListoista[i].Nimi;
                soittolista.Click += new RoutedEventHandler(soittolista_Click);
                wrappi.Children.Add(soittolista);
            }

            Button ButtonAloitus = new Button();
            ButtonAloitus.Content = FindResource("aloitaPeli");
            ButtonAloitus.Click += new RoutedEventHandler(ButtonAloitus_Click);
            stackPanelPeliAlusta.Children.Add(ButtonAloitus);
        }

        /// <summary>
        /// Kutsuu aliohjelmaa, joka asettaa kysymykset. Tarkistaa ensin, että
        /// soittolista on valittu ja että se on sopiva
        /// </summary>
        private void ButtonAloitus_Click(object sender, RoutedEventArgs e)
        {
            if (pelinSoittolista == null || pelinSoittolista.getKappaleidenMaara() < 4)
            {
                MessageBox.Show(FindResource("vaaraLista") as string);
                return;
            }
            stackPanelPeliAlusta.Children.Clear();
            asetaKysymys();
        }

        /// <summary>
        /// Asettaa ruutuun uuden kysymyksen
        /// </summary>
        private void asetaKysymys()
        {

            VastausLomake vastausLomake = new VastausLomake();
            vastausLomake.Name = "VastausLomake";
            vastausLomake.Margin = new Thickness(10, 10, 0, 0);
            vastausLomake.VerticalAlignment = VerticalAlignment.Stretch;
            vastausLomake.HorizontalAlignment = HorizontalAlignment.Stretch;
            Kappale[] kappaleet = pelinSoittolista.annaRandomKappale(4);
            vastausLomake.asetaKysymykset(kappaleet[0].Nimi, kappaleet[1].Nimi, kappaleet[2].Nimi, kappaleet[3].Nimi);
            int oikeaKappaleenNumero = r.Next(0, 4);
            vastausLomake.asetaVoittaja(oikeaKappaleenNumero);
            soitin.asetaSoitettavaKappale(kappaleet[oikeaKappaleenNumero], true);
            stackPanelPeliAlusta.Children.Add(vastausLomake);
        }

        /// <summary>
        /// Asettaa pelattavan soittolistan
        /// </summary>
        private void soittolista_Click(object sender, RoutedEventArgs e)
        {
            RadioButton lahettaja = (RadioButton)sender;
            pelinSoittolista = annaSoittolista(lahettaja.Content.ToString());
        }

        /// <summary>
        /// Valinta eventin kuuntelija. Antaa pisteet sen perusteella paljonko niitä
        /// ansaitsee
        /// </summary>
        /// <param name="sender">lähettäjä</param>
        /// <param name="e">Tapahtuman argumentit, joista saadaan pistemäärä selville</param>
        private void stackPanelPeliAlusta_Valinta(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            double saadutPisteet = Double.Parse(e.OriginalSource.ToString());

            pisteet += saadutPisteet;


            LabelPisteet.Content = String.Format("{0:0.00}", pisteet);
            kysymysNumero++;
            if (kysymysNumero >= 10)
            {
                soitin.lopetaSoitto();
                stackPanelPeliAlusta_Loaded(null, null);
                MessageBox.Show(FindResource("sait") as string + " " + pisteet + " " + FindResource("pistetta") as string);
                pisteet = 0;
                LabelPisteet.Content = String.Format("{0:0.00}", pisteet);
                kysymysNumero = 0;
                return;
            }

            stackPanelPeliAlusta.Children.Clear();
            asetaKysymys();
        }

        /// <summary>
        /// Näyttää ohjeet
        /// </summary>
        private void ButtonOhjeet_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult ohjeet = MessageBox.Show(FindResource("ohjeet") as string);
        }

        /// <summary>
        /// Palauttaa listan, jonka nimi täsmää parametriin
        /// </summary>
        /// <param name="nimi">halutun soittolistan nimi</param>
        /// <returns>Lista, jonka nimi täsmäsi tai null, jos ei löydy listaa</returns>
        private Soittolista annaSoittolista(string nimi)
        {
            foreach (Soittolista lista in listaSoittoListoista)
            {
                if (lista.Nimi.Equals(nimi))
                    return lista;
            }

            return null;
        }
        #endregion
    }
}
