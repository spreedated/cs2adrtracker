using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using DatabaseLayer.Models;
using System.Collections.ObjectModel;
using System;
using System.Reflection;
using System.Threading.Tasks;
using AdrTracker.Logic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Avalonia.Media.Immutable;

namespace AdrTracker.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
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

        #region Ctor
        public MainWindowViewModel()
        {
            this.WindowTitle = typeof(MainWindowViewModel).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
            
            if (Design.IsDesignMode)
            {
                this.CurrentAdr = 88.45f;
                this.TrackedGamesCount = 142;
                this.Statistic = new()
                {
                    Draws = 10,
                    Losses = 20,
                    Wins = 112
                };
                return;
            }

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
                Log.Warning("Invalid adr provided \"{InputAdr}\"", this.InputAdr);
                return;
            }

            await Task.Run(() =>
            {
                if (!Globals.Database.AddAdr(new AdrRecord() { Value = outadr, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds(), Outcome = this.Outcome }))
                {
                    Log.Warning("ADR value \"{Outadr}\" NOT added to database due to being invalid", outadr);
                    return;
                }

                Log.Information("ADR value \"{Outadr}\" (Outcome: \"{Outcome}\") added to database", outadr, this.Outcome.ToString());
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

            Log.Verbose("Data refreshed");
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
