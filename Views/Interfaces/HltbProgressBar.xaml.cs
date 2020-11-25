﻿using HowLongToBeat.Models;
using HowLongToBeat.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using PluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HowLongToBeat.Views.Interfaces
{
    /// <summary>
    /// Logique d'interaction pour HltbProgressBar.xaml
    /// </summary>
    public partial class HltbProgressBar : UserControl
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private HowLongToBeatDatabase PluginDatabase = HowLongToBeat.PluginDatabase;

        private GameHowLongToBeat _gameHowLongToBeat = null;
        private long _Playtime;

        public bool ShowToolTip { get; set; }
        public bool ShowTime { get; set; }

        private bool IsFirstLoad = true;


        public HltbProgressBar()
        {
            InitializeComponent();

            PluginDatabase.PropertyChanged += OnPropertyChanged;
        }


        protected void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
#if DEBUG
                logger.Debug($"HltbProgressBar.OnPropertyChanged({e.PropertyName}): {JsonConvert.SerializeObject(PluginDatabase.GameSelectedData)}");
#endif
                if (e.PropertyName == "GameSelectedData" || e.PropertyName == "PluginSettings")
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new ThreadStart(delegate
                    {
                        if (!PluginDatabase.GameSelectedData.HasData)
                        {
                            this.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            this.Visibility = Visibility.Visible;

                            SetHltbData(PluginDatabase.GameSelectedData);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "HowLongToBeat");
            }
        }


        private void SetDataInView(int ElIndicator, long ElValue, string ElFormat)
        {
            Decorator indicator = null;
            Rectangle track = null;

            try
            {
                switch (ElIndicator)
                {
                    case 1:
                        if (ElValue != 0)
                        {
                            ProgressHltb_El1.Visibility = Visibility.Visible;
                            spHltb_El1.Visibility = Visibility.Visible;
                            spHltbTime_El1.Visibility = Visibility.Visible;
                        }

                        ProgressHltb_El1.Value = ElValue;
                        spHltb_El1.Visibility = Visibility.Visible;

                        indicator = (Decorator)ProgressHltb_El1.Template.FindName("PART_Indicator", ProgressHltb_El1);
                        track = (Rectangle)ProgressHltb_El1.Template.FindName("PART_Track", ProgressHltb_El1);

                        // ToolTip
                        if (indicator != null && track != null)
                        {
                            spHltb_El1.Width = indicator.Width;
                            tpHltb_El1.Content = ElFormat;

                            if (track.Width == indicator.Width)
                            {
                                spHltb_El1.Width = track.ActualWidth;
                            }
                        }

                        // Time
                        spHltbTime_El1.Content = ElFormat;
                        break;

                    case 2:
                        if (ElValue != 0)
                        {
                            ProgressHltb_El2.Visibility = Visibility.Visible;
                            spHltb_El2.Visibility = Visibility.Visible;
                            spHltbTime_El2.Visibility = Visibility.Visible;
                        }

                        ProgressHltb_El2.Value = ElValue;


                        indicator = (Decorator)ProgressHltb_El2.Template.FindName("PART_Indicator", ProgressHltb_El2);

                        // ToolTip
                        if (indicator != null)
                        {
                            spHltb_El2.Width = indicator.Width - spHltb_El1.Width;
                            tpHltb_El2.Content = ElFormat;
                        }

                        // Time
                        spHltbTime_El2.Content = ElFormat;
                        break;

                    case 3:
                        if (ElValue != 0)
                        {
                            ProgressHltb_El3.Visibility = Visibility.Visible;
                            spHltb_El3.Visibility = Visibility.Visible;
                            spHltbTime_El3.Visibility = Visibility.Visible;
                        }

                        ProgressHltb_El3.Value = ElValue;

                        indicator = (Decorator)ProgressHltb_El3.Template.FindName("PART_Indicator", ProgressHltb_El3);

                        // ToolTip
                        if (indicator != null)
                        {
                            spHltb_El3.Width = indicator.Width - spHltb_El2.Width - spHltb_El1.Width;
                            tpHltb_El3.Content = ElFormat;
                        }

                        // Time
                        spHltbTime_El3.Content = ElFormat;
                        break;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "HowLongToBeat", $"Error on SetDataInView({ElIndicator}, {ElValue}, {ElFormat})");
            }
        }

        public void SetHltbData(GameHowLongToBeat gameHowLongToBeat)
        {
#if DEBUG
            logger.Debug($"HowLongToBeat - PluginSettings: {JsonConvert.SerializeObject(PluginDatabase.PluginSettings)}");
#endif

            _gameHowLongToBeat = gameHowLongToBeat;
            _Playtime = gameHowLongToBeat.Playtime;


            ShowToolTip = PluginDatabase.PluginSettings.ProgressBarShowToolTip;
            ShowTime = PluginDatabase.PluginSettings.ProgressBarShowTime;

            ProgressHltb_El1.Foreground = new SolidColorBrush(PluginDatabase.PluginSettings.ColorFirst);
            ProgressHltb_El2.Foreground = new SolidColorBrush(PluginDatabase.PluginSettings.ColorSecond);
            ProgressHltb_El3.Foreground = new SolidColorBrush(PluginDatabase.PluginSettings.ColorThird);

            if (ShowToolTip)
            {
                PART_ShowToolTip.Visibility = Visibility.Visible;
            }
            else
            {
                PART_ShowToolTip.Visibility = Visibility.Collapsed;
            }

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                if (IsFirstLoad)
                {
                    if (PluginDatabase.PluginSettings.ProgressBarShowTime && !PluginDatabase.PluginSettings.ProgressBarShowTimeInterior)
                    {
                        PART_HltbProgressBar_Contener.Height = PART_HltbProgressBar_Contener.Height - spShowTime.Height;
                    }
                    else
                    {
                        spShowTime.Height = 0;
                    }

                    if (PluginDatabase.PluginSettings.ProgressBarShowTime && PluginDatabase.PluginSettings.ProgressBarShowTimeAbove)
                    {
                        Grid.SetRow(spShowTime, 0);
                        Grid.SetRow(PART_HltbProgressBar_Contener, 1);
                    }
                    if (PluginDatabase.PluginSettings.ProgressBarShowTime && PluginDatabase.PluginSettings.ProgressBarShowTimeInterior)
                    {
                        Grid.SetRow(spShowTime, 0);
                        spShowTime.Height = PART_HltbProgressBar_Contener.Height;
                    }

                    IsFirstLoad = false;
                }

                // Definied data value in different component.
                int ElIndicator = 0;
                long MaxValue = 0;
                long MaxHltb = 0;
                List<ListProgressBar> listProgressBars = new List<ListProgressBar>();
                if (_gameHowLongToBeat.HasData)
                {
                    var HltbData = _gameHowLongToBeat.GetData();

                    if (HltbData.GameHltbData.MainStory != 0)
                    {
                        ElIndicator += 1;
                        listProgressBars.Add(new ListProgressBar { Indicator = ElIndicator, Value = HltbData.GameHltbData.MainStory, Format = HltbData.GameHltbData.MainStoryFormat });
                        if (MaxValue < HltbData.GameHltbData.MainStory)
                        {
                            MaxValue = HltbData.GameHltbData.MainStory;
                        }
                    }

                    if (HltbData.GameHltbData.MainExtra != 0)
                    {
                        ElIndicator += 1;
                        listProgressBars.Add(new ListProgressBar { Indicator = ElIndicator, Value = HltbData.GameHltbData.MainExtra, Format = HltbData.GameHltbData.MainExtraFormat });
                        if (MaxValue < HltbData.GameHltbData.MainExtra)
                        {
                            MaxValue = HltbData.GameHltbData.MainExtra;
                        }
                    }

                    if (HltbData.GameHltbData.Completionist != 0)
                    {
                        ElIndicator += 1;
                        listProgressBars.Add(new ListProgressBar { Indicator = ElIndicator, Value = HltbData.GameHltbData.Completionist, Format = HltbData.GameHltbData.CompletionistFormat });
                        if (MaxValue < HltbData.GameHltbData.Completionist)
                        {
                            MaxValue = HltbData.GameHltbData.Completionist;
                        }
                    }

                    if (HltbData.GameHltbData.Solo != 0)
                    {
                        ElIndicator += 1;
                        listProgressBars.Add(new ListProgressBar { Indicator = ElIndicator, Value = HltbData.GameHltbData.Solo, Format = HltbData.GameHltbData.SoloFormat });
                        if (MaxValue < HltbData.GameHltbData.Solo)
                        {
                            MaxValue = HltbData.GameHltbData.Solo;
                        }
                    }

                    if (HltbData.GameHltbData.CoOp != 0)
                    {
                        ElIndicator += 1;
                        listProgressBars.Add(new ListProgressBar { Indicator = ElIndicator, Value = HltbData.GameHltbData.CoOp, Format = HltbData.GameHltbData.CoOpFormat });
                        if (MaxValue < HltbData.GameHltbData.CoOp)
                        {
                            MaxValue = HltbData.GameHltbData.CoOp;
                        }
                    }

                    if (HltbData.GameHltbData.Vs != 0)
                    {
                        ElIndicator += 1;
                        listProgressBars.Add(new ListProgressBar { Indicator = ElIndicator, Value = HltbData.GameHltbData.Vs, Format = HltbData.GameHltbData.VsFormat });
                        if (MaxValue < HltbData.GameHltbData.Vs)
                        {
                            MaxValue = HltbData.GameHltbData.Vs;
                        }
                    }
                }

                // Define the maxvalue for progressbar & slider
                MaxHltb = MaxValue;
                if (_Playtime > MaxValue)
                {
                    MaxValue = _Playtime;
                }

                // Adjust position tracker
                if (_Playtime > 69)
                {
                    SliderPlaytime.Margin = new Thickness(-8, 0, -3, 0);
                }

                // Limit MaxValue when playtime is more than MaxHltb
                long MaxPercent = (long)Math.Ceiling((double)(10 * MaxHltb / 100));
                if (MaxValue > MaxHltb + MaxPercent)
                {
                    MaxValue = MaxHltb + MaxPercent;
                }


                // Add data
                ProgressHltb_El1.Maximum = MaxValue;
                ProgressHltb_El2.Maximum = MaxValue;
                ProgressHltb_El3.Maximum = MaxValue;

                SliderPlaytime.Value = _Playtime;
                SliderPlaytime.Maximum = MaxValue;

#if DEBUG
                logger.Debug($"HowLongToBeat - listProgressBars: {JsonConvert.SerializeObject(listProgressBars)}");
#endif

                ProgressHltb_El1.Visibility = Visibility.Hidden;
                spHltb_El1.Visibility = Visibility.Hidden;
                spHltbTime_El1.Visibility = Visibility.Hidden;

                ProgressHltb_El2.Visibility = Visibility.Hidden;
                spHltb_El2.Visibility = Visibility.Hidden;
                spHltbTime_El2.Visibility = Visibility.Hidden;

                ProgressHltb_El3.Visibility = Visibility.Hidden;
                spHltb_El3.Visibility = Visibility.Hidden;
                spHltbTime_El3.Visibility = Visibility.Hidden;

                foreach (var listProgressBar in listProgressBars)
                {
                    SetDataInView(listProgressBar.Indicator, listProgressBar.Value, listProgressBar.Format);
                }


                SliderPlaytime.UpdateLayout();
            }
            catch (Exception ex)
            {
                Common.LogError(ex, "HowLongToBeat", "Error on LoadData()");
            }
        }


        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            IntegrationUI.SetControlSize(PART_HltbProgressBar_Contener);
        }
    }

    internal class ListProgressBar
    {
        public int Indicator { get; set; }
        public long Value { get; set; }
        public string Format { get; set; }
    }
}