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
using System.Windows.Shapes;
using NAudio;
using NAudio.Wave;
using System.Windows.Threading;
using WMPLib;
using System.Windows.Controls.Primitives;
using System.Globalization;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for MusicPlayer.xaml
    /// </summary>
    public partial class MusicPlayer : UserControl
    {

        #region Propertyt
        private double kappaleenPituus;
        private WindowsMediaPlayer soitin;
        private DispatcherTimer kello;
        private int tekstinVaihtaja = 0;
        private string kappaleenNimi;
        private string kappaleenEsittaja;
        private string kappaleenAlbumi;
        private string kappaleenSijainti;
        private bool valiSoitto;
        private double kappaleenKohta;
        private double siirto;
        private Soittolista soittoLista;
        private int kappaleNro;
        private bool repeat;
        private bool shuffle;

        double KappaleenPituus
        {
            get { return kappaleenPituus; }
            set { kappaleenPituus = value; }
        }

        /// <summary>
        /// Property, joka kertoo kuinka paljon kappaletta on kulunut
        /// </summary>
        public static readonly DependencyProperty KappalettaKulunutProperty =
         DependencyProperty.Register(
           "KappalettaKulunut",
           typeof(double), // propertyn tietotyyppi
           typeof(MusicPlayer), // luokka jossa property sijaitsee
           new FrameworkPropertyMetadata(0.0,  // propertyn oletusarvo
              FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
              new PropertyChangedCallback(KappalettaKulunutVaihtui),  // kutsutaan propertyn arvon muuttumisen jälkeen
              new CoerceValueCallback(MuutaKappalettaKulunut))); // kutsutaan ennen propertyn arvon muutosta

        /// <summary>
        /// voidaan asettaa tieto siitä, kuinka paljon kappaletta on kulunut
        /// </summary>
        public double KappalettaKulunut
        {
            get { return (double)GetValue(KappalettaKulunutProperty); }
            set { SetValue(KappalettaKulunutProperty, value); }
        }


        private static object MuutaKappalettaKulunut(DependencyObject element, object value)
        {
            double arvo = (double)value;
            return arvo;
        }

        private static void KappalettaKulunutVaihtui(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // ei tarvita?
        }

        /// <summary>
        /// Proprty, jonka avulla saadaan bindattua haluttu tieto labeliin
        /// </summary>
        public static readonly DependencyProperty KappaleenTiedotProperty =
            DependencyProperty.Register("KappaleenTiedot", typeof(string), typeof(MusicPlayer));

        /// <summary>
        /// Voi asettaa haluamansa asian kappaleen tiedoksi, käytetään kun vaihdellan
        /// sitä, mitä tietoa kappaleestä näytetään( artisti, albumi, nimi)
        /// </summary>
        public string KappaleenTiedot
        {
            get { return (string)GetValue(KappaleenTiedotProperty); }
            set { SetValue(KappaleenTiedotProperty, value); }
        }

        #endregion

        /// <summary>
        /// Musiikki soittimen konstruktori, luo soittimen ja kellon sitä varten
        /// </summary>
        public MusicPlayer()
        {
            InitializeComponent();
            soitin = new WindowsMediaPlayer();

            kello = new DispatcherTimer();
            kello.Tick += new EventHandler(kello_Tick);
            kello.Interval = new TimeSpan(0, 0, 0, 0, 10);
            sliderVolume.Value = 50;
            sliderTahti.Value = 1;
            sliderTahti.Minimum = 0.05;

        }

        /// <summary>
        /// Asettaa annetun string listan soittimen soittolistaksi.
        /// </summary>
        /// <param name="soittoLista">soittolista, jota aloitetaan soittamaan</param>
        public void luoSoittolista(Soittolista soittoLista)
        {
            this.soittoLista = soittoLista;
            asetaSoitettavaKappale(this.soittoLista.getKappale(0), false);
        }

        #region Soitin
        /// <summary>
        /// Siirtää täppiä sliderilla ja vaihtaa kappaleesta kertovaa tietoa tietyin väliajoin
        /// </summary>
        private void kello_Tick(object sender, EventArgs e)
        {
            KappalettaKulunut = soitin.controls.currentPosition;

            if (valiSoitto)
            {
                

                if (soitin.controls.currentPosition >= sliderKappale.HigherValue)
                {
                    
                    buttonValiPysayta_Click(null, new RoutedEventArgs(null));
                }
            }

            else
            {
                if (KappalettaKulunut >= KappaleenPituus - 1)
                {
                    if (repeat)
                    {
                        soitin.controls.currentPosition = 0;

                    }
                    else
                        buttonNext_Click(null, null);
                }

                tekstinVaihtaja++;

                if (tekstinVaihtaja > 750)
                    tekstinVaihtaja = 0;

                if (tekstinVaihtaja > 500)
                {
                    KappaleenTiedot = kappaleenEsittaja;
                    return;
                }

                if (tekstinVaihtaja > 250)
                {
                    KappaleenTiedot = kappaleenAlbumi;
                }

                else
                    KappaleenTiedot = kappaleenNimi;
            }
        }

        /// <summary>
        /// Asettaa annetun kappaleen soitettavaksi ja aloittaa soittamisen 
        /// jos parametrina tuoraan true
        /// </summary>
        /// <param name="kappale">Soitettava kappala</param>
        /// <param name="soitetaanko">Soitetaanko kappale</param>
        public void asetaSoitettavaKappale(Kappale kappale, bool soitetaanko)
        {
            if (kappale == null)
                return;

            buttonLeikkausMod.IsEnabled = true;

            kappaleNro = soittoLista.annaKappaleenPaikkaListalla(kappale);
            kappaleenSijainti = kappale.TiedostoSijainti;
            kappaleenNimi = kappale.Nimi;
            kappaleenEsittaja = kappale.Esittaja;
            kappaleenAlbumi = kappale.Albumi;
            KappaleenPituus = kappale.Pituus;
            sliderKappale.Maximum = KappaleenPituus;
            sliderKappale.HigherValue = KappaleenPituus;
            soitin.URL = kappaleenSijainti;
            soitin.settings.rate = sliderTahti.Value;
            soitin.settings.volume = (int)sliderVolume.Value;

            if (soitetaanko)
            {
                kello.IsEnabled = true;
                soitin.controls.play();
            }

            else
                soitin.controls.stop();

            
        }

        /// <summary>
        /// Asettaa soittimen stop tilaan
        /// </summary>
        public void lopetaSoitto()
        {
            soitin.controls.stop();
        }

        /// <summary>
        /// Leikkaa kappaleen slidereilla määrätyltä väliltä sekä tallentaa se käyttäjältä kysyttyyn kohtaan.
        /// </summary>
        public void leikkaaJaTallennaKappale()
        {

            SaveFileDialog tallennusDialogi = new SaveFileDialog();
            tallennusDialogi.Filter = "mp3 |*.mp3";
            tallennusDialogi.Title = "Save MP3 file";
            tallennusDialogi.ShowDialog();
            if (tallennusDialogi.FileName != "")
            {
                using (Mp3FileReader lukija = new Mp3FileReader(kappaleenSijainti))
                {
                    System.IO.FileStream kirjoitettavaStream;
                    double framienMaara = sliderKappale.HigherValue / 0.026122449 - sliderKappale.LowerValue / 0.026122449; //0.026s on yhden framin pituus mp3:ssa
                    lukija.Skip(((int)sliderKappale.LowerValue * 1000) / 1000);
                    Mp3Frame frame = lukija.ReadNextFrame();

                    try
                    {
                        kirjoitettavaStream = new System.IO.FileStream(tallennusDialogi.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    }

                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                        return;
                    }

                    while (framienMaara >= 0)
                    {
                        kirjoitettavaStream.Write(frame.RawData, 0, frame.RawData.Length);
                        framienMaara--;
                        frame = lukija.ReadNextFrame();
                    }

                    kirjoitettavaStream.Close();

                }
            }
        }

        /// <summary>
        /// Asettaa play nappulan esille ja pause nappulan piiloon
        /// </summary>
        public void vaihdaPlay()
        {
            buttonPlay.Visibility = Visibility.Visible;
            buttonPause.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Asettaa pause nappulan esille ja play nappulan piiloon
        /// </summary>
        public void vaihdaPause()
        {
            buttonPlay.Visibility = Visibility.Hidden;
            buttonPause.Visibility = Visibility.Visible;
        }

        #endregion

        #region Nappulat

        /// <summary>
        /// Aloittaa kappaleen soittamisen tai jatkaa sitä.
        /// </summary>
        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (soittoLista == null)
                return;

            kello.IsEnabled = true;
            soitin.controls.play();
            buttonPlay.Visibility = Visibility.Hidden;
            buttonPause.Visibility = Visibility.Visible;
            valiSoitto = false;
            buttonLeikkausMod.IsEnabled = true;
        }

        /// <summary>
        /// Pysayttaa kappaleen soimisen
        /// </summary>
        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            soitin.controls.pause();
            buttonPause.Visibility = Visibility.Hidden;
            buttonPlay.Visibility = Visibility.Visible;
            buttonLeikkaa.IsEnabled = true;
            buttonvali.IsEnabled = true;
        }

        /// <summary>
        /// Vaihtaa seuraavaan kappaleeseen, jos ei ole kappaleita jälellä soittolistalla
        /// lopettaa soittamisen ja siirtää soittolistan ensimmäisen kappaleen soittovuoroon
        /// </summary>
        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if (soittoLista == null)
                return;

            if (shuffle)
                siirryRandomKappaleeseen();

            else
            {
                kappaleNro++;
                if (kappaleNro >= soittoLista.getKappaleidenMaara())
                {
                    kappaleNro = 0;
                    soitin.controls.stop();
                    buttonPause.Visibility = Visibility.Hidden;
                    buttonPlay.Visibility = Visibility.Visible;
                    asetaSoitettavaKappale(soittoLista.getKappale(kappaleNro), false);
                }

                else
                {
                    asetaSoitettavaKappale(soittoLista.getKappale(kappaleNro), false);
                    if (tarkistaSoitetaanko())
                    {
                        soitin.controls.play();
                    }
                    else
                    {
                        soitin.controls.stop();
                    }
                }


            }
        }

        /// <summary>
        /// Siirtyy edelliseen kappaleeseen, jos shuffle niin randomiin
        /// </summary>
        private void buttonPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (soittoLista == null)
                return;

            if (shuffle)
                siirryRandomKappaleeseen();

            else
            {
                kappaleNro--;
                if (kappaleNro < 0)
                {
                    kappaleNro = 0;
                }

                asetaSoitettavaKappale(soittoLista.getKappale(kappaleNro),false);

                if (tarkistaSoitetaanko())
                    soitin.controls.play();
                else
                    soitin.controls.stop();
            }
        }

        /// <summary>
        /// Tarkistaa soitetaanko parhaillaan vai ei.
        /// </summary>
        /// <returns>tieto, onko musiikki päällä vai ei</returns>
        private bool tarkistaSoitetaanko()
        {
            if (buttonPlay.Visibility == Visibility.Visible)
                return false;

            else
                return true;
        }
        //siirtyy random kappaleeseen
        private void siirryRandomKappaleeseen()
        {

            Random r = new Random();
            asetaSoitettavaKappale(soittoLista.getKappale(r.Next(soittoLista.getKappaleidenMaara())),false);
            soitin.controls.play();

        }
        /// <summary>
        /// Toggle button repeat. Voit asettaa repeat toiminnon päälle tai pois päältä
        /// </summary>
        private void buttonRepeat_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton lahettaja = (ToggleButton)sender;
            if (lahettaja.IsChecked == true)
            {
                repeat = true;
            }
            else
                repeat = false;
        }

        /// <summary>
        /// Voi asettaa shuffle toiminnon päälle tai pois päältä
        /// </summary>
        private void buttonShuffle_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton lahettaja = (ToggleButton)sender;
            if (lahettaja.IsChecked == true)
            {
                shuffle = true;
            }
            else
                shuffle = false;
        }

        /// <summary>
        /// Tarkistaa, onko kappale mp3 ja, jos on kutsuu aliohjelmaa, joka suorittaa kappaleen leikkamisen
        /// </summary>
        private void buttonLeikkaa_Click(object sender, RoutedEventArgs e)
        {
            if (!(kappaleenSijainti.EndsWith(".mp3")))
            {
                MessageBoxResult moka = MessageBox.Show("Pystyn leikkaamaan vain mp3 tiedostoja");
            }

            else
            {
                string tallennusTeksti = "Leikataanko väli " + aikaMuunnin(sliderKappale.LowerValue) + " - " + aikaMuunnin(sliderKappale.HigherValue);
                string otsikko = "Mp3 kappaleiden leikkaus";
                MessageBoxButton nappulat = MessageBoxButton.YesNo;
                MessageBoxResult tulos = MessageBox.Show(tallennusTeksti, otsikko, nappulat);

                switch (tulos)
                {
                    case MessageBoxResult.Yes:
                        soitin.controls.pause();
                        leikkaaJaTallennaKappale();
                        break;

                    case MessageBoxResult.No:
                        break;
                }
            }
        }

        /// <summary>
        /// Aloittaa kappaleen soittamisen täppien välillä.
        /// Kun on päässyt täpältä täpällä palaa ensimmäiselle täpälle
        /// </summary>
        private void buttonvali_Click(object sender, RoutedEventArgs e)
        {
            kappaleenKohta = KappalettaKulunut;
            valiSoitto = true;
            soitin.controls.play();
            kello.Start();
            buttonvali.Visibility = Visibility.Hidden;
            buttonValiPysayta.Visibility = Visibility.Visible;
            buttonValiPysayta.IsEnabled = true;
        }

        /// <summary>
        /// Pysayttaa valin soittamisen
        /// </summary>
        private void buttonValiPysayta_Click(object sender, RoutedEventArgs e)
        {
            soitin.controls.currentPosition = kappaleenKohta;
            soitin.controls.pause();
            buttonvali.Visibility = Visibility.Visible;
            buttonValiPysayta.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Vaihtaa komponentin näppäimet soittamista varten soveltuviksi
        /// </summary>
        private void buttonSoittoMod_Click(object sender, RoutedEventArgs e)
        {
            buttonPlay.Visibility = Visibility.Visible;
            buttonNext.Visibility = Visibility.Visible;
            buttonPrevious.Visibility = Visibility.Visible;
            buttonRepeat.Visibility = Visibility.Visible;
            buttonShuffle.Visibility = Visibility.Visible;
            buttonLeikkausMod.Visibility = Visibility.Visible;

            buttonSoittoMod.Visibility = Visibility.Hidden;
            buttonValiPysayta.Visibility = Visibility.Hidden;
            buttonvali.Visibility = Visibility.Hidden;
            buttonSiirraAlku.Visibility = Visibility.Hidden;
            buttonSoittoMod.Visibility = Visibility.Hidden;
            textBoxAlunSiirto.Visibility = Visibility.Hidden;
            buttonLeikkaa.Visibility = Visibility.Hidden;
            buttonSiirraAloitusHetki.Visibility = Visibility.Hidden;
            buttonSiirraLopetusHetki.Visibility = Visibility.Hidden;
            buttonSiirraLoppua.Visibility = Visibility.Hidden;
            textBoxLopetusKohta.Visibility = Visibility.Hidden;
            textBoxLopunSiirto.Visibility = Visibility.Hidden;
            textBoxAloitusKohta.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Vaihtaa komponentin näppäimet kappaleiden leikkaamiseen sopivaksi
        /// </summary>
        private void buttonLeikkausMod_Click(object sender, RoutedEventArgs e)
        {
            soitin.controls.pause();
            buttonPause.Visibility = Visibility.Hidden;
            buttonPlay.Visibility = Visibility.Hidden;
            buttonNext.Visibility = Visibility.Hidden;
            buttonPrevious.Visibility = Visibility.Hidden;
            buttonRepeat.Visibility = Visibility.Hidden;
            buttonShuffle.Visibility = Visibility.Hidden;
            buttonLeikkausMod.Visibility = Visibility.Hidden;

            buttonSoittoMod.Visibility = Visibility.Visible;
            buttonvali.Visibility = Visibility.Visible;
            buttonSiirraAlku.Visibility = Visibility.Visible;
            buttonSoittoMod.Visibility = Visibility.Visible;
            textBoxAlunSiirto.Visibility = Visibility.Visible;
            buttonLeikkaa.Visibility = Visibility.Visible;
            buttonSiirraAloitusHetki.Visibility = Visibility.Visible;
            textBoxAloitusKohta.Text = aikaMuunnin(sliderKappale.LowerValue);
            textBoxLopetusKohta.Text = aikaMuunnin(sliderKappale.HigherValue);
            textBoxAloitusKohta.Visibility = Visibility.Visible;
            buttonSiirraLopetusHetki.Visibility = Visibility.Visible;
            buttonSiirraLoppua.Visibility = Visibility.Visible;
            textBoxLopetusKohta.Visibility = Visibility.Visible;
            textBoxLopunSiirto.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Siirtää aloituskohtaa textboksissa olevan määrän
        /// </summary>
        private void buttonSiirraAlku_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxAlunSiirto.Background == Brushes.Red)
                return;

            if (KappalettaKulunut + siirto <= sliderKappale.HigherValue)
            {
                KappalettaKulunut += siirto;
                soitin.controls.currentPosition += siirto;

                if (KappalettaKulunut < 0)
                    KappalettaKulunut = 0;
            }
        }

        /// <summary>
        /// Siirtaa aloitushetken eli pienemmän arvon omaavan täpän textboxAloitusKohdassa olevaan aikaan
        /// </summary>
        private void buttonSiirraAloitusHetki_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxAloitusKohta.Background == Brushes.White)
            {
                KappalettaKulunut = merkkijonoSekunteiksi(textBoxAloitusKohta.Text);
                soitin.controls.currentPosition = KappalettaKulunut;
            }
        }

        /// <summary>
        /// Siirtää lopetushetken, eli suuremman täpän textBoxLopetusKohta olevaan aikaan
        /// </summary>
        private void buttonSiirraLopetusHetki_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxLopetusKohta.Background == Brushes.White)
            {
                sliderKappale.HigherValue = merkkijonoSekunteiksi(textBoxLopetusKohta.Text);
            }
        }

        /// <summary>
        /// Siirtää suurempaa täppää textBoxLopunSiirrossa olevan ajan verran
        /// </summary>
        private void buttonSiirraLoppua_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxLopunSiirto.Background == Brushes.Red)
                return;

            if (sliderKappale.HigherValue + siirto <= KappaleenPituus)
            {
                sliderKappale.HigherValue += siirto;
            }
        }
        #endregion

        #region Sliders

        /// <summary>
        /// Kun sliderKappaleen drag aloitetaan pysäytetään kello, jotta
        /// kappale ei täppä ei pyri siirtymään
        /// </summary>
        private void sliderKappale_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (kello != null)
                kello.Stop();
        }

        /// <summary>
        /// Kun siirto on suoritettu laitetaan kappale soimaan täpän kohdalta ja 
        /// käynnistetään kello uudestaan
        /// </summary>
        private void sliderKappale_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (kello != null)
            {
                soitin.controls.currentPosition = sliderKappale.LowerValue;
                kello.Start();
            }
        }

        /// <summary>
        /// Jos vaihtaa slider Volumen arvoa, niin äänenvoimakkuus vaihtuu
        /// </summary>
        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            soitin.settings.volume = (int)sliderVolume.Value;
        }

        /// <summary>
        /// Kun vaihtaa slidertahdin arvoa niin soiton tahti muuttuu
        /// </summary>
        private void sliderTahti_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            soitin.settings.rate = sliderTahti.Value;
        }
        #endregion

        #region Muuntimet
        /// <summary>
        /// Muuntaa annetun ajan muotoon 00:00
        /// </summary>
        /// <param name="aika">aika double arvona</param>
        /// <returns>aika muutettuna muotoon 00:00</returns>
        public static string aikaMuunnin(double aika)
        {
            int aikaSekunteina = (int)aika;
            int sekunnit = aikaSekunteina % 60;
            int minutit = aikaSekunteina / 60;

            return string.Format("{0:00}:{1:00}", minutit, sekunnit);
        }

        /// <summary>
        /// Muuttaa annetun stringin, joka on muotoa 00:00 sekunteiksi
        /// </summary>
        /// <param name="aika">string muotoa 00:00</param>
        /// <returns>annettu aika sekunteina, palauttaa -1, jos aika on väärää muotoa</returns>
        public static int merkkijonoSekunteiksi(string aika)
        {
            if (tarkistaAjanTyyli(aika))
            {
                int sekunnit;
                string[] ajat = aika.Split(':');
                sekunnit = int.Parse(ajat[0]) * 60 + int.Parse(ajat[1]);
                return sekunnit;
            }

            else
                return -1;
        }

        /// <summary>
        /// Tarkistaa, että annettu aika on tyyliä 00:00 ja ettei sekuntit ole yli 59
        /// </summary>
        /// <param name="aika">tarkastettava aika</param>
        /// <returns>tieto, onko tyyli oikein</returns>
        public static bool tarkistaAjanTyyli(string aika)
        {
            string tyyli = @"^([0-9][0-9]:[0-5][0-9])$";
            Regex tarkistin = new Regex(tyyli);
            if ((tarkistin.IsMatch(aika)))
                return true;

            return false;
        }

        #endregion

        #region Tarkistimet

        private void textBoxAlunSiirto_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox lahettaja = (TextBox)sender;

            try
            {
                siirto = double.Parse(lahettaja.Text);
                lahettaja.Background = Brushes.White;

            }

            catch (FormatException)
            {
                lahettaja.Background = Brushes.Red;
            }
        }

        private void textBoxAloitusKohta_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tarkistaAjanTyyli(textBoxAloitusKohta.Text))
            {
                textBoxAloitusKohta.Background = Brushes.White;
            }

            else
            {
                textBoxAloitusKohta.Background = Brushes.Red;
            }
        }

        private void textBoxLopetusKohta_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tarkistaAjanTyyli(textBoxLopetusKohta.Text))
            {
                textBoxLopetusKohta.Background = Brushes.White;
            }

            else
            {
                textBoxLopetusKohta.Background = Brushes.Red;
            }
        }

        #endregion
    }

    /// <summary>
    /// Luokka, jolla voi muuttaa sekuntit muotoon 00:00
    /// </summary>
    public class SekuntitAjaksi : IValueConverter
    {
        /// <summary>
        /// Muuttaa sekuntit muotoo 00:00
        /// </summary>
        /// <param name="value">sekunteina tuotu aika, joka muunnetaan</param>
        /// <returns>aika muodossa 00:00</returns>
        /// <param name="targetType">kohteen tyyppi</param>
        /// <param name="parameter">parametrit</param>
        /// <param name="culture">tieto kulttuurista</param>
        /// <returns>palauttaa muokatun ajan</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int aikaSekunteina = (int)(double)value;
            int sekunnit = aikaSekunteina % 60;
            int minutit = aikaSekunteina / 60;

            return string.Format("{0:00}:{1:00}", minutit, sekunnit);
        }

        /// <summary>
        /// Ei toteutettu
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}
