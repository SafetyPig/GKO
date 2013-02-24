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

namespace MediaPlayer
{
    /// <summary>
    /// UserControl, jossa on slider kahdella täpällä.
    /// </summary>
    public partial class Slider2Thumbs : UserControl
    {
        /// <summary>
        /// Konstruktori, joka asettaa loaded tapahtuman
        /// </summary>
        public Slider2Thumbs()
        {
            InitializeComponent();

            this.Loaded += Slider_Loaded;
        }

        /// <summary>
        /// Kun arvoja vaihdetaan, pitää huolen siitä, että higher value ei pääse alemman alle
        /// </summary>
        void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            LowerSlider.ValueChanged += LowerSlider_ValueChanged;
            HigherSlider.ValueChanged += HigherSlider_ValueChanged;
        }

        /// <summary>
        /// Dependency property LowerValue. 
        /// Voi asettaa matalamman korkeamman arvon omaavalle täpälle
        /// </summary>
        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register(
            "LowerValue",
            typeof(double),
            typeof(Slider2Thumbs));

        /// <summary>
        /// Alemman täpän arvo
        /// </summary>
        public double LowerValue
        {
            get { return (double)GetValue(LowerValueProperty); }
            set { SetValue(LowerValueProperty, value); }
        }

        /// <summary>
        /// Varmistaa, että HigherValue on aina suurempi
        /// </summary>
        private void LowerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            HigherSlider.Value = Math.Max(HigherSlider.Value, LowerSlider.Value);
        }

        /// <summary>
        /// Dependency property HigherValue. 
        /// Voi asettaa arvon korkeamman arvon omaavalle täpälle
        /// </summary>
        public static readonly DependencyProperty HigherValueProperty =
            DependencyProperty.Register(
            "HigherValue", 
            typeof(double), 
            typeof(Slider2Thumbs));

        /// <summary>
        /// Ylemmän täpän arvo
        /// </summary>
        public double HigherValue
        {
            get { return (double)GetValue(HigherValueProperty); }
            set { SetValue(HigherValueProperty, value); }
        }

        /// <summary>
        /// Varmistaa, että lowerValue on aina pienempi
        /// </summary>
        private void HigherSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LowerSlider.Value = Math.Min(HigherSlider.Value, LowerSlider.Value);
        }

        /// <summary>
        /// Dependency property Maximum. 
        /// Voi asettaa maksimi arvon sliderille
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(Slider2Thumbs));

        /// <summary>
        /// Sliderien maksimiarvo
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Dependency property Minimum. 
        /// Voi asettaa minimi arvon sliderille
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
    DependencyProperty.Register("Minimum", typeof(double), typeof(Slider2Thumbs));

        /// <summary>
        /// Sliderien minimiarvo
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

    }
}
