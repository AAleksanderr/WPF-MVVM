using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using Task2.Annotations;
using Task2.Models;

namespace Task2.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private const int Days = 30;
        private readonly string _dataPath = "data/";
        private readonly string _savedDataPath = "data/SavedData/";
        private string _alertMessage;
        private Visibility _alertVisibility;
        private UserInterfaceData _selectedUser;
        private bool _sortedByAverage;
        private bool _sortedByMax;
        private bool _sortedByMin;
        private bool _sortedByName;

        public MainWindowViewModel()
        {
            _alertVisibility = Visibility.Collapsed;
            LoadData();
            CreateListBoxUsersData();
        }


        private List<UserData> UsersData { get; set; } = new List<UserData>();


        public ObservableCollection<UserInterfaceData> ListBoxUsersDatas { get; set; } =
            new ObservableCollection<UserInterfaceData>();

        public UserInterfaceData SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        public Visibility AlertVisibility
        {
            get => _alertVisibility;
            set
            {
                _alertVisibility = value;
                OnPropertyChanged();
            }
        }

        public string AlertMessage
        {
            get => _alertMessage;
            set
            {
                _alertMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand NameCommand => new Command(com => SortByName());
        public ICommand AverageCommand => new Command(com => SortByAverage());
        public ICommand MaxCommand => new Command(com => SortByMax());
        public ICommand MinCommand => new Command(com => SortByMin());
        public ICommand SaveSelected => new Command(com => SaveSlectedToFile());

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadData()
        {
            for (var i = 1; i <= Days; i++)
            {
                var items = new List<Customer>();
                var path = $"{_dataPath}day{i}.json";
                if (File.Exists(path))
                {
                    using (var r = new StreamReader(path))
                    {
                        var json = r.ReadToEnd();
                        try
                        {
                            items = JsonConvert.DeserializeObject<List<Customer>>(json);
                        }
                        catch (Exception)
                        {
                            AlertMessage = "Some data files are missing or corrupted.";
                            AlertVisibility = Visibility.Visible;
                        }
                    }
                }
                else
                {
                    AlertMessage = "Some data files are missing or corrupted.";
                    AlertVisibility = Visibility.Visible;
                }

                foreach (var customer in items)
                    if (UsersData.Select(t => t.User).Contains(customer.User))
                    {
                        var currentUser = UsersData.Find(u => u.User == customer.User);
                        currentUser.Steps[i - 1] = customer.Steps;
                        currentUser.Rank[i - 1] = customer.Rank;
                        currentUser.Status[i - 1] = customer.Status;
                    }
                    else
                    {
                        UsersData.Add(new UserData
                        {
                            User = customer.User,
                            Steps = new int[Days],
                            Rank = new int[Days],
                            Status = new string[Days]
                        });
                        UsersData.Last().Steps[i - 1] = customer.Steps;
                        UsersData.Last().Rank[i - 1] = customer.Rank;
                        UsersData.Last().Status[i - 1] = customer.Status;
                    }
            }

            UsersData = UsersData.OrderBy(i => i.User).ToList();
        }

        private void CreateListBoxUsersData()
        {
            foreach (var usersData in UsersData)
            {
                SolidColorBrush solidColorBrush;
                var userAverageSteps = usersData.Steps.Sum() / Days;
                var maxSteps = usersData.Steps.Max();
                var minSteps = usersData.Steps.Min();
                var points = new PointCollection();
                for (var i = 1; i <= usersData.Steps.Length; i++)
                    points.Add(new Point(i * 15, 330 - usersData.Steps[i - 1] / 320));
                if (userAverageSteps * 1.2 > maxSteps && userAverageSteps * 0.8 < minSteps)
                    solidColorBrush = new SolidColorBrush(Colors.AliceBlue);
                else solidColorBrush = new SolidColorBrush(Colors.BlanchedAlmond);

                var user = new UserInterfaceData
                {
                    AverageSteps = userAverageSteps,
                    MaxSteps = maxSteps,
                    MinSteps = minSteps,
                    Name = usersData.User,
                    ItemColor = solidColorBrush,
                    Points = points
                };
                ListBoxUsersDatas.Add(user);
            }
        }

        private void SaveSlectedToFile()
        {
            var path = $"{_savedDataPath}{_selectedUser.Name}.json";
            var userToSaveData = UsersData.Find(i => i.User == _selectedUser.Name);
            try
            {
                Directory.CreateDirectory(_savedDataPath);
                using (var w = new StreamWriter(path))
                {
                    var json = JsonConvert.SerializeObject(userToSaveData);
                    w.Write(json);
                }

                AlertVisibility = Visibility.Collapsed;
            }
            catch (Exception)
            {
                AlertMessage = "Data not saved";
                AlertVisibility = Visibility.Visible;
            }
        }

        private void SortByName()
        {
            var obsCollection = _sortedByName
                ? new ObservableCollection<UserInterfaceData>(ListBoxUsersDatas.OrderBy(i => i.Name))
                : new ObservableCollection<UserInterfaceData>(ListBoxUsersDatas.OrderByDescending(i => i.Name));
            ListBoxUsersDatas.Clear();
            foreach (var listBoxUserData in obsCollection)
                ListBoxUsersDatas.Add(listBoxUserData);
            _sortedByName = !_sortedByName;
        }

        private void SortByAverage()
        {
            var obsCollection = _sortedByAverage
                ? new ObservableCollection<UserInterfaceData>(ListBoxUsersDatas.OrderBy(i => i.AverageSteps))
                : new ObservableCollection<UserInterfaceData>(ListBoxUsersDatas.OrderByDescending(i => i.AverageSteps));
            ListBoxUsersDatas.Clear();
            foreach (var listBoxUserData in obsCollection)
                ListBoxUsersDatas.Add(listBoxUserData);
            _sortedByAverage = !_sortedByAverage;
        }

        private void SortByMax()
        {
            var obsCollection = _sortedByMax
                ? new ObservableCollection<UserInterfaceData>(ListBoxUsersDatas.OrderBy(i => i.MaxSteps))
                : new ObservableCollection<UserInterfaceData>(ListBoxUsersDatas.OrderByDescending(i => i.MaxSteps));
            ListBoxUsersDatas.Clear();
            foreach (var listBoxUserData in obsCollection)
                ListBoxUsersDatas.Add(listBoxUserData);
            _sortedByMax = !_sortedByMax;
        }

        private void SortByMin()
        {
            var obsCollection = _sortedByMin
                ? new ObservableCollection<UserInterfaceData>(ListBoxUsersDatas.OrderBy(i => i.MinSteps))
                : new ObservableCollection<UserInterfaceData>(ListBoxUsersDatas.OrderByDescending(i => i.MinSteps));
            ListBoxUsersDatas.Clear();
            foreach (var listBoxUserData in obsCollection)
                ListBoxUsersDatas.Add(listBoxUserData);
            _sortedByMin = !_sortedByMin;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class Command : ICommand
        {
            private readonly Action<object> _action;

            public Command(Action<object> action)
            {
                _action = action;
            }

            public event EventHandler CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _action?.Invoke(parameter);
            }
        }
    }
}