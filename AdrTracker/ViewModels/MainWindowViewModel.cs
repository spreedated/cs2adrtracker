using AdrTracker.Logic;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DatabaseLayer.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AdrTracker.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly Microsoft.Extensions.Logging.ILogger logger;

        #region BindableProperties
        [ObservableProperty]
        private string windowTitle;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(this.AddCommand))]
        private AdrRecord.Outcomes outcome;

        [ObservableProperty]
        private AdrRecord.Outcomes[] outcomeList = Enum.GetValues<AdrRecord.Outcomes>();

        [ObservableProperty]
        private ObservableCollection<AdrRecord> last10Records;

        [ObservableProperty]
        private float last10Average;

        [ObservableProperty]
        private ContextMenu contextMenuTaskbar;

        [ObservableProperty]
        private Window instance;

        [ObservableProperty]
        private float currentAdr;

        [ObservableProperty]
        private int trackedGamesCount;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(this.AddCommand))]
        private string inputAdr;

        [ObservableProperty]
        private bool isAddButtonVisible;

        [ObservableProperty]
        private Statistic statistic;
        #endregion

        [ObservableProperty]
        private ISeries[] series;

        [ObservableProperty]
        private Axis[] xAxes = [new() { IsVisible = false }];

        [ObservableProperty]
        private Axis[] yAxes = [new() { IsVisible = false }];

        #region Ctor
        public MainWindowViewModel()
        {
            this.WindowTitle = typeof(MainWindowViewModel).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
            
            if (Design.IsDesignMode)
            {
                this.Last10Records =
                [
                    new AdrRecord() { Value = 85, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = AdrRecord.Outcomes.Win },
                    new AdrRecord() { Value = 90, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = AdrRecord.Outcomes.Win },
                    new AdrRecord() { Value = 95, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = AdrRecord.Outcomes.Win },
                    new AdrRecord() { Value = 80, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = AdrRecord.Outcomes.Loss },
                    new AdrRecord() { Value = 75, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = AdrRecord.Outcomes.Loss },
                    new AdrRecord() { Value = 100, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = AdrRecord.Outcomes.Win },
                    new AdrRecord() { Value = 110, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = AdrRecord.Outcomes.Win },
                    new AdrRecord() { Value = 120, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = AdrRecord.Outcomes.Draw },
                    new AdrRecord() { Value = 130, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = AdrRecord.Outcomes.Draw },
                    new AdrRecord() { Value = 140, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = AdrRecord.Outcomes.Win }
                ];
                this.CurrentAdr = 88.42f;
                this.TrackedGamesCount = 142;
                this.Statistic = new()
                {
                    Draws = 10,
                    Losses = 20,
                    Wins = 112
                };
                return;
            }

            this.logger = Log.Logger != null ? new SerilogLoggerProvider(Log.Logger).CreateLogger("MainWindowViewModel"): null;

            Task.Run(async () =>
            {
                await this.RefreshData();
            });
        }
        #endregion

        #region Commands
        [RelayCommand(CanExecute = nameof(CanAdd))]
        private async Task Add()
        {
            if (!int.TryParse(this.InputAdr, out int outadr))
            {
                this.logger?.LogWarning("Invalid ADR provided \"{InputAdr}\"", this.InputAdr);
                return;
            }

            await Task.Run(() =>
            {
                if (!Globals.Database.AddAdr(new AdrRecord() { Value = outadr, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = this.Outcome }))
                {
                    this.logger?.LogWarning("ADR value \"{Outadr}\" NOT added to database due to being invalid", outadr);
                    return;
                }

                this.logger?.LogInformation("ADR value \"{Outadr}\" (Outcome: \"{Outcome}\") added to database", outadr, this.Outcome.ToString());
            });

            await this.RefreshData();
            this.InputAdr = default;
            this.Outcome = default;
        }

        private bool CanAdd()
        {
            bool res = (this.InputAdr != null && this.InputAdr.Length > 0) && this.Outcome != default;

            this.IsAddButtonVisible = res;
            return res;
        }

        [RelayCommand]
        private async Task Delete(int adrId)
        {
            Globals.Database.DeleteAdr(adrId);
            await this.RefreshData();
        }

        public async Task RefreshData()
        {
            await Task.Run(() =>
            {
                IEnumerable<AdrRecord> adrs = Globals.Database.GetAdrs();

                this.CurrentAdr = adrs.Any() ? (float)adrs.Average(x => x.Value) : 0f;
                this.TrackedGamesCount = adrs.Any() ? adrs.Count() : 0;
                this.Last10Records = new(Globals.Database.GetLast());
                this.Last10Average = this.Last10Records.Any() ? (float)this.Last10Records.Average(x => x.Value) : 0f;
                this.Statistic = Globals.Database.GetStatistic();
            });

            base.OnPropertyChanged(nameof(this.IndicatorBrush));
            base.OnPropertyChanged(nameof(this.IndicatorShadowColor));

            this.Series =
                [
                    new RowSeries<int>(this.Statistic.Wins)
                    {
                        Name = "Wins",
                        Fill = new SolidColorPaint(SKColors.LimeGreen),
                        DataLabelsSize = 14,
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                        DataLabelsPaint = new SolidColorPaint(SKColors.WhiteSmoke)
                    },
                    new RowSeries<int>(this.Statistic.Draws)
                    {
                        Name = "Draws",
                        Fill = new SolidColorPaint(SKColors.Gold),
                        DataLabelsSize = 14,
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                        DataLabelsPaint = new SolidColorPaint(SKColors.WhiteSmoke)
                    },
                    new RowSeries<int>(this.Statistic.Losses)
                    {
                        Name = "Loss",
                        Fill = new SolidColorPaint(SKColors.Firebrick),
                        DataLabelsSize = 14,
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                        DataLabelsPaint = new SolidColorPaint(SKColors.WhiteSmoke)
                    }
                ];

            this.logger?.LogTrace("Data refreshed");
        }

        public IImmutableSolidColorBrush IndicatorBrush
        {
            get
            {
                if (this.CurrentAdr < 60)
                    return new ImmutableSolidColorBrush(Color.Parse("#B22222")); // Red
                if (this.CurrentAdr < 80)
                    return new ImmutableSolidColorBrush(Color.Parse("#FF8C00")); // Orange
                if (this.CurrentAdr < 100)
                    return new ImmutableSolidColorBrush(Color.Parse("#FFD700")); // Gold
                if (this.CurrentAdr < 120)
                    return new ImmutableSolidColorBrush(Color.Parse("#32CD32")); // LimeGreen
                return new ImmutableSolidColorBrush(Color.Parse("#228B22")); // ForestGreen
            }
        }

        public Color IndicatorShadowColor
        {
            get
            {
                return this.IndicatorBrush.Color;
            }
        }
        #endregion
    }
}
