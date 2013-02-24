using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPlayer
{
    /// <summary>
    /// Kappale luokka, joka osaa selvittää esittäjän, nimensä, sekä muita tietoja
    /// joita tagit antaa kappaleesta
    /// </summary>
    public class Kappale
    {
        private string tiedostonSijainti;
        private string nimi;
        private string esittaja;
        private string albumi;

        /// <summary>
        /// Kappaleen tiedostosjainti
        /// </summary>
        public string TiedostoSijainti
        {
            get
            {
                return tiedostonSijainti;
            }

            set
            {
                if (value == null)
                    tiedostonSijainti = "";

                else
                    tiedostonSijainti = value;
            }
        }

        /// <summary>
        /// Kappaleen nimi
        /// </summary>
        public string Nimi
        {
            get
            {
                return nimi;
            }

            set
            {
                if (value == null)
                    nimi = tiedostonSijainti;

                else
                    nimi = value;
            }
        }

        /// <summary>
        /// Kappaleen esittäjä
        /// </summary>
        public string Esittaja
        {
            get
            {
                return esittaja;
            }

            set
            {
                if (value == null)
                    esittaja = "Tuntematon";

                else
                    esittaja = value;
            }
        }
        /// <summary>
        /// Kappaleen albumi
        /// </summary>
        public string Albumi
        {
            get
            {
                return albumi;
            }

            set
            {
                if (value == null)
                    albumi = "";

                else
                    albumi = value;
            }
        }

        /// <summary>
        /// Kappaleen pituus sekunteina
        /// </summary>
        public double Pituus { get; set; }

        /// <summary>
        /// Kappaleen numero albumilla
        /// </summary>
        public int KappaleNro { get; set; }

        /// <summary>
        /// Konstruktori, jolle tuodaan kappaleen tiedostosijainti
        /// </summary>
        /// <param name="tiedostoSijainti">Kappaleen tiedostosijainti</param>
        public Kappale(string tiedostoSijainti)
        {
            if (tiedostoSijainti.Equals(""))
                return;

            TiedostoSijainti = tiedostoSijainti;
            TagLib.File kappaleenTiedot = TagLib.File.Create(tiedostoSijainti);
            Nimi = kappaleenTiedot.Tag.Title;
            Esittaja = kappaleenTiedot.Tag.FirstPerformer;
            Albumi = kappaleenTiedot.Tag.Album;
            Pituus = kappaleenTiedot.Properties.Duration.TotalSeconds;
            KappaleNro = (int)kappaleenTiedot.Tag.Track;
        }

        /// <summary>
        /// Ylikirjoitetaan toString, jotta voidaan näyttää siististi ikkunassa
        /// </summary>
        /// <returns>palauttaa kappaleen Nimen</returns>
        public override string ToString()
        {
            return Esittaja + " - " + Nimi;
        }
    }
}
