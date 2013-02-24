using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPlayer
{
    /// <summary>
    /// Soittolista luokka
    /// </summary>
    public class Soittolista
    {
        private List<Kappale> soittoLista = new List<Kappale>();
        private int kappaleidenMaara = 0;
        /// <summary>
        /// Soittolistan nimi
        /// </summary>
        public string Nimi { get; set; }

        /// <summary>
        /// Palauttaa soittolistan kappaleet
        /// </summary>
        /// <returns>soittolistan kappaleet</returns>
        public List<Kappale> getSoittoLista()
        {
            return soittoLista;
        }

        /// <summary>
        /// Konstruktori
        /// </summary>
        public Soittolista()
        {
        }

        /// <summary>
        /// Ylikirjoitettu toString(), palauttaa soittolistan nimen
        /// </summary>
        /// <returns>Soittolistan nimi</returns>
        public override string ToString()
        {
            return Nimi;
        }
        /// <summary>
        /// Konstruktori, jolle tuodaan soittolista tiedostoon tallennetussa muodossa eli:
        /// nimi|kappaleen sijainti|kappaleen sijainti .... jne.
        /// </summary>
        /// <param name="soittolistaTiedostoMuodossa">soittolista tiedosto muodossa</param>
        public Soittolista(string soittolistaTiedostoMuodossa)
        {
            string[] pilkottuLista = soittolistaTiedostoMuodossa.Split('|');
            this.Nimi = pilkottuLista[0];
            for(int i = 1; i < pilkottuLista.Length; i++)
            {
                Kappale kappale = new Kappale(pilkottuLista[i]);
                lisaa(kappale);
            }
        }

        /// <summary>
        /// Lisää kappaleen soittolistaan
        /// </summary>
        /// <param name="kappale">lisättävä kappale</param>
        public void lisaa(Kappale kappale)
        {
            soittoLista.Add(kappale);
            kappaleidenMaara++;
        }

        /// <summary>
        /// Palauttaa kappaleiden määrän soittolistalla
        /// </summary>
        /// <returns>kappaleiden määrä</returns>
        public int getKappaleidenMaara()
        {
            return kappaleidenMaara;
        }

        /// <summary>
        /// Palauttaa kappaleen, jonka indeksi annetaan parametrinä.
        /// Jos pyydetään kappaletta jota ei ole, palautetaan null
        /// </summary>
        /// <param name="index">Halutun kappaleen indeksi</param>
        /// <returns>kappale, jonka indeksi osuu</returns>
        public Kappale getKappale(int index)
        {
            if (soittoLista.Count <= index)
                return null;
            return soittoLista[index];
        }

        /// <summary>
        /// Poistaa listalta annetun kappaleen
        /// </summary>
        public void poista(Kappale kappale)
        {
            for (int i = 0; i < getKappaleidenMaara(); i++)
            {
                if (soittoLista[i].Equals(kappale))
                    soittoLista.RemoveAt(i);
            }

            kappaleidenMaara--;
        }

        /// <summary>
        /// Palauttaa tallennettavan muodon
        /// </summary>
        /// <returns>Tallennettava muoto</returns>
        public string tallennettavaMuoto()
        {
            StringBuilder tallennettavaTeksti = new StringBuilder();
            tallennettavaTeksti.Append(Nimi);
            for (int i = 0; i < kappaleidenMaara; i++)
            {
                tallennettavaTeksti.Append( "|" + getKappale(i).TiedostoSijainti);
            }
            return tallennettavaTeksti.ToString();
        }
        
        /// <summary>
        /// Järjestää soittolistan aakkosjärjestykseen
        /// </summary>
        public void jarjestaAakkos()
        {
            soittoLista.Sort(new KappaleVertailija());
        }

        /// <summary>
        /// palauttaa talukollisen random kappaleita
        /// </summary>
        /// <param name="maara">Kuinka monta kappaletta palautetaan</param>
        /// <returns>Kappale taulukko, jossa haluttu määrä kappaleita. Jos pyytää enemmän kappaleita kun on, palauttaa null</returns>
        public Kappale[] annaRandomKappale(int maara)
        {
            if (maara > getKappaleidenMaara())
                return null;

            int[] numerot =  arvoRandomNumerot(4,getKappaleidenMaara());
            Kappale[] kappaleet = new Kappale[maara];

            for (int i = 0; i < maara; i++)
            {
                kappaleet[i] = soittoLista[numerot[i]];
            }

            return kappaleet;
        }

        /// <summary>
        /// Arpoo halutun määrän random numeroita väliltä 0 - maksimi ja palauttaa ne taulukkona.
        /// </summary>
        /// <param name="maara">Montako numeroa arvotaan</param>
        /// <param name="maksimi">Mikä on numeroiden maksimikoko</param>
        /// <returns>Arvotut numerot taulukossa</returns>
        public int[] arvoRandomNumerot(int maara, int maksimi)
        {
            int[] arvotutNumerot = new int[maksimi];
            int[] palautettavatNumerot = new int[maara];

            for(int i = 0; i < arvotutNumerot.Length; i++)
            {
                arvotutNumerot[i] = i;
            }

            shuffle(arvotutNumerot);

            for(int i = 0; i < maara; i++)
            {
                palautettavatNumerot[i] = arvotutNumerot[i];
            }

            return palautettavatNumerot;
        }

        /// <summary>
        /// Sekoittaa listan Fisher-Yates periaatteella
        /// </summary>
        /// <param name="taulukko">sekoitettava taulukko</param>
        public static void shuffle(int[] taulukko)
        {
            Random r = new Random();   
            int n = taulukko.Length; 
            while (n > 1)
            {
                int k = r.Next(n);
                n--;                  
                int temp = taulukko[n];
                taulukko[n] = taulukko[k];
                taulukko[k] = temp;
            }
        }

        /// <summary>
        /// Palauttaa kappaleen paikan listala
        /// </summary>
        /// <param name="kappale">Etsittävä kappale</param>
        /// <returns>Kappaleen paikka listalla</returns>
        public int annaKappaleenPaikkaListalla(Kappale kappale)
        {
            return soittoLista.IndexOf(kappale);
        }

    }
}
