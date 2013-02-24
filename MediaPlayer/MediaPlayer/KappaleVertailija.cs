using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MediaPlayer
{
    /// <summary>
    /// Luokka, jolla voi vertailla kappaleiden nimiä
    /// </summary>
    public class KappaleVertailija : IComparer<Kappale>
    {
        /// <summary>
        /// Vertailee kahden kappaleen artisteja ja albumeja. Palauttaa 1, jos ensimmäinen on ennen
        /// 0 jos sama nimi, -1 jos toinen on ennen. Jos saman artistin palauttaa
        /// kappale numeron mukaisen järjestyksen
        /// </summary>
        /// <param name="a">Ensimmäinen kappale</param>
        /// <param name="b">Toinen kappale</param>
        /// <returns>1 jos ensimmäinen ennen, 0 jos samat, -1 jos toinen ennen</returns>
        public int Compare(Kappale a, Kappale b)
        {
            if (a == null || b == null)
                return 0;

            string ANimi = a.Esittaja;
            string BNimi = b.Esittaja;

            switch(ANimi.CompareTo(BNimi))
            {
                case 1:
                    return 1;
                    
                case -1:
                    return -1;

                default:
                    return vertaaAlbumi(a,b);
            }
        }

        /// <summary>
        /// Vertaa albumeja ja palauttaa sen, kumpi on ensin
        /// </summary>
        /// <param name="a">Kappale a</param>
        /// <param name="b">Kappale b</param>
        /// <returns> 1 jos a > b, 0 jos a = b, -1 jos b > a </returns>
        private int vertaaAlbumi(Kappale a, Kappale b)
        {
            switch(a.Albumi.CompareTo(b.Albumi))
            {
                case 1:
                    return 1;

                case -1:
                    return -1;

                default:
                    return vertaaKappaleNro(a,b);
            }
        }

        /// <summary>
        /// Vertaa kappaleiden numeroita ja palauttaa tiedon, kumpi on suurempi
        /// </summary>
        /// <param name="a">Kappale a</param>
        /// <param name="b">Kappale b</param>
        /// <returns>1 jos a > b, 0 jos a = b, -1 jos b > a </returns>
        private int vertaaKappaleNro(Kappale a, Kappale b)
        {
            return a.KappaleNro.CompareTo(b.KappaleNro);
        }
    }
}
