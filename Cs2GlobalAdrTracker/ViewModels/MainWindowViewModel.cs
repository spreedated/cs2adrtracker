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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cs2GlobalAdrTracker.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        internal TaskbarIcon taskbarIcon;

        public MainWindowViewModel()
        {
            this.taskbarIcon = new()
            {
#pragma warning disable S1075
                IconSource = new BitmapImage(new Uri(@"pack://application:,,,/Resources/logo.ico", UriKind.Absolute))
#pragma warning restore S1075
            };
        }

        public ICommand AddCommand { get; } = new RelayCommand<MainWindow>((w) =>
        {
            if (!int.TryParse(((MainWindowViewModel)w.DataContext).InputAdr, out int outadr))
            {
                Log.Warning($"Invalid adr provided \"{((MainWindowViewModel)w.DataContext).InputAdr}\"");
                return;
            }

            ((MainWindowViewModel)w.DataContext).InputAdr = null;

            Task.Factory.StartNew(() =>
            {
                if (!RuntimeStorage.Database.AddAdr(new AdrRecord() { Value = outadr, UnixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds() }))
                {
                    Log.Warning($"ADR value \"{outadr}\" NOT added to database due to being invalid");
                    return;
                }

                Log.Information($"ADR value \"{outadr}\" added to database");

                w.Dispatcher.Invoke(() =>
                {
                    ((MainWindowViewModel)w.DataContext).RefreshData();
                });
            });
        });

        public void RefreshData()
        {
            Task.Factory.StartNew(() =>
            {
                IEnumerable<AdrRecord> adrs = RuntimeStorage.Database.GetAdrs();

                this.CurrentAdr = adrs.Any() ? (float)adrs.Average(x => x.Value) : 0f;
                this.TrackedGamesCount = adrs.Any() ? adrs.Count() : 0;
                this.Last10Records = new(RuntimeStorage.Database.GetLast());
                this.Last10Average = this.Last10Records.Any() ? (float)this.Last10Records.Average(x => x.Value) : 0f;

                Log.Information("Data refreshed");
            });
        }

        private ObservableCollection<AdrRecord> _Last10Records;
        public ObservableCollection<AdrRecord> Last10Records
        {
            get
            {
                return this._Last10Records;
            }
            set
            {
                this._Last10Records = value;
                base.OnPropertyChanged(nameof(this.Last10Records));
            }
        }

        private float _Last10Average;
        public float Last10Average
        {
            get
            {
                return this._Last10Average;
            }
            set
            {
                this._Last10Average = value;
                base.OnPropertyChanged(nameof(this.Last10Average));
            }
        }

        private ContextMenu _ContextMenuTaskbar;
        public ContextMenu ContextMenuTaskbar
        {
            get
            {
                return this._ContextMenuTaskbar;
            }
            set
            {
                this._ContextMenuTaskbar = value;
                base.OnPropertyChanged(nameof(this.ContextMenuTaskbar));
                this.taskbarIcon.ContextMenu = value;
            }
        }

        private Window _Instance;
        public Window Instance
        {
            get
            {
                return this._Instance;
            }
            set
            {
                this._Instance = value;
                base.OnPropertyChanged(nameof(this.Instance));
            }
        }

        private float _CurrentAdr;
        public float CurrentAdr
        {
            get
            {
                return this._CurrentAdr;
            }
            set
            {
                this._CurrentAdr = value;
                base.OnPropertyChanged(nameof(this.CurrentAdr));
                base.OnPropertyChanged(nameof(this.IndicatorBrush));
            }
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

        private int _TrackedGamesCount;
        public int TrackedGamesCount
        {
            get
            {
                return this._TrackedGamesCount;
            }
            set
            {
                this._TrackedGamesCount = value;
                base.OnPropertyChanged(nameof(this.TrackedGamesCount));
            }
        }

        private string _InputAdr;
        public string InputAdr
        {
            get
            {
                return this._InputAdr;
            }
            set
            {
                this._InputAdr = value;
                base.OnPropertyChanged(nameof(this.InputAdr));
                base.OnPropertyChanged(nameof(this.IsInputNumeric));
                base.OnPropertyChanged(nameof(this.HasLength));
            }
        }

        public bool IsInputNumeric
        {
            get
            {
                return int.TryParse(this.InputAdr, out _);
            }
        }

        public bool HasLength
        {
            get
            {
                return this.InputAdr != null && this.InputAdr.Length > 0;
            }
        }

        public void DisposeNotifyIcon()
        {
            this.taskbarIcon?.Dispose();
        }
    }
}
