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
using System.Windows.Threading;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for VastausLomake.xaml
    /// </summary>
    public partial class VastausLomake : UserControl
    {
        int voittaja = 0;
        int arvaus = -1;

        /// <summary>
        /// Konstruktori
        /// </summary>
        public VastausLomake()
        {
            InitializeComponent();
            DispatcherTimer kello = new DispatcherTimer();
            kello.Interval = new TimeSpan(0, 0, 0, 0,100);
            kello.Tick += new EventHandler(kello_Tick);
            kello.Start();
            
        }

        void kello_Tick(object sender, EventArgs e)
        {
            progressBarAika.Value += 0.1;
            if(progressBarAika.Value == progressBarAika.Maximum)
                RaiseEvent(new RoutedEventArgs(ValintaEvent, "0"));
        }

        /// <summary>
        /// Delegaatti valinta eventille
        /// </summary>
        public delegate void ValintaHandler(object sender, RoutedEventArgs e);

        /// <summary>
        /// Valinta event, joka laukaistaa kun valinta nappia on painettu
        /// </summary>
        public static readonly RoutedEvent ValintaEvent =
    EventManager.RegisterRoutedEvent("Valinta", RoutingStrategy.Bubble,
    typeof(RoutedEventHandler), typeof(VastausLomake));

        /// <summary>
        /// Valinta eventin asettaja
        /// </summary>
        public event RoutedEventHandler Valinta
        {
            add { AddHandler(ValintaEvent, value); }
            remove { RemoveHandler(ValintaEvent, value); }
        }

        /// <summary>
        /// Asettaa annetut merkkijonot kysymyksen vaihtoehdoiksi
        /// </summary>
        /// <param name="a">ensimmäinen vaihtoehto</param>
        /// <param name="b">toinen vaihtoehto</param>
        /// <param name="c">kolmas vaihtoehto</param>
        /// <param name="d">neljäs vaihtoehto</param>
        public void asetaKysymykset(string a, string b, string c, string d)
        {
            radioButtonKappale1.Content = a;
            radioButtonKappale2.Content = b;
            radioButtonKappale3.Content = c;
            radioButtonKappale4.Content = d;
        }

        /// <summary>
        /// Asettaa oikean vaihtoehdon
        /// </summary>
        /// <param name="voittaja">Mikä vaihtoehdoista on oikein</param>
        public void asetaVoittaja(int voittaja)
        {
            this.voittaja = voittaja;
        }

        /// <summary>
        /// Palauttaa arvauksen
        /// </summary>
        /// <returns>arvaus</returns>
        public int getArvaus()
        {
            return arvaus;
        }

        /// <summary>
        /// Tarkistaa menikö arvaus oikein vai ei
        /// </summary>
        private void buttonVastaa_Click(object sender, RoutedEventArgs e)
        {
            if (arvaus == voittaja)
            {
                RaiseEvent(new RoutedEventArgs(ValintaEvent, 10 - progressBarAika.Value));
            }

            else
            {
                RaiseEvent(new RoutedEventArgs(ValintaEvent, -1));
            }
        }

        /// <summary>
        /// Asettaa arvauksen
        /// </summary>
        private void radio1_Checked(object sender, RoutedEventArgs e)
        {
            arvaus = 0;
        }

        /// <summary>
        /// Asettaa arvauksen
        /// </summary>
        private void radio2_Checked(object sender, RoutedEventArgs e)
        {
            arvaus = 1;
        }

        /// <summary>
        /// Asettaa arvauksen
        /// </summary>
        private void radio3_Checked(object sender, RoutedEventArgs e)
        {
            arvaus = 2;
        }

        /// <summary>
        /// Asettaa arvauksen
        /// </summary>
        private void radio4_Checked(object sender, RoutedEventArgs e)
        {
            arvaus = 3;
        }

        
    }
}
