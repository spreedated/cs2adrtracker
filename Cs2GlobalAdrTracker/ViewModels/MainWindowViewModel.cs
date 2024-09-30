using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cs2GlobalAdrTracker.Logic;
using Cs2GlobalAdrTracker.Views;
using DatabaseLayer.Models;
using Hardcodet.Wpf.TaskbarNotification;
using neXn.Lib.Wpf.ViewLogic;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cs2GlobalAdrTracker.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        internal TaskbarIcon taskbarIcon;

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
        private MainWindow instance;

        [ObservableProperty]
        private float currentAdr;

        [ObservableProperty]
        private int trackedGamesCount;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(this.AddCommand))]
        private string inputAdr;

        [ObservableProperty]
        private bool isAddButtonVisible; 

        #region Constructor
        public MainWindowViewModel()
        {
            this.WindowTitle = typeof(MainWindowViewModel).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;

            this.taskbarIcon = new()
            {
#pragma warning disable S1075
                IconSource = new BitmapImage(new Uri(@"pack://application:,,,/Resources/logo.ico", UriKind.Absolute))
#pragma warning restore S1075
            };

            Task.Run(async () =>
            { 
                await this.RefreshData();
            });
        }
        #endregion

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

        public async Task RefreshData()
        {
            await Task.Run(() =>
            {
                IEnumerable<AdrRecord> adrs = Globals.Database.GetAdrs();

                this.CurrentAdr = adrs.Any() ? (float)adrs.Average(x => x.Value) : 0f;
                this.TrackedGamesCount = adrs.Any() ? adrs.Count() : 0;
                this.Last10Records = new(Globals.Database.GetLast());
                this.Last10Average = this.Last10Records.Any() ? (float)this.Last10Records.Average(x => x.Value) : 0f;
            });

            base.OnPropertyChanged(nameof(this.IndicatorBrush));

            Log.Verbose("Data refreshed");
        }

        public Brush IndicatorBrush
        {
            get
            {
                if (this.CurrentAdr < 60)
                {
                    return Brushes.DarkRed;
                }
                if (this.CurrentAdr > 60 && this.CurrentAdr < 105)
                {
                    return Brushes.YellowGreen;
                }
                if (this.CurrentAdr > 105)
                {
                    return Brushes.DarkGreen;
                }

                return Brushes.Black;
            }
        }

        public void DisposeNotifyIcon()
        {
            this.taskbarIcon?.Dispose();
        }
    }
}
