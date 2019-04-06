using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Task2.Models;

namespace Task2.ViewModels
{
    public class MainWindowViewModel
    {
        private const int Days = 30;
        private bool _sortedByName;
        private bool _sortedByAverage;
        private bool _sortedByMax;
        private bool _sortedByMin;

        public MainWindowViewModel()
        {
            LoadData();
            CreateListBoxUsersData();
        }


        public ObservableCollection<ListBoxUserData> ListBoxUsersDatas { get; set; } =
            new ObservableCollection<ListBoxUserData>();

        public List<DiagramUserData> DiagramUsersDatas { get; set; } = new List<DiagramUserData>();
        public ObservableCollection<Shape> CanvasElements { get; set; } = new ObservableCollection<Shape>();
        public bool[] IsSelected { get; set; } = new bool[Days];

        public ICommand DrawSelectedCommand => new Command(DrawCommand);
        public ICommand NameCommand => new Command(com => SortByName());
        public ICommand AverageCommand => new Command(com => SortByAverage());
        public ICommand MaxCommand => new Command(com => SortByMax());
        public ICommand MinCommand => new Command(com => SortByMin());


        private void LoadData()
        {
            for (var i = 1; i <= Days; i++)
            {
                var items = new List<Customer>();
                var path = $"data/day{i}.json";
                if (File.Exists(path))
                    using (var r = new StreamReader(path))
                    {
                        var json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<Customer>>(json);
                    }

                foreach (var customer in items)
                    if (DiagramUsersDatas.Select(t => t.User).Contains(customer.User))
                    {
                        var currentUser = DiagramUsersDatas.Find(u => u.User == customer.User);
                        currentUser.Steps[i - 1] = customer.Steps;
                        currentUser.Rank[i - 1] = customer.Rank;
                        currentUser.Status[i - 1] = customer.Status;
                    }
                    else
                    {
                        DiagramUsersDatas.Add(new DiagramUserData
                        {
                            User = customer.User,
                            Steps = new int[Days],
                            Rank = new int[Days],
                            Status = new string[Days]
                        });
                        DiagramUsersDatas.Last().Steps[i - 1] = customer.Steps;
                        DiagramUsersDatas.Last().Rank[i - 1] = customer.Rank;
                        DiagramUsersDatas.Last().Status[i - 1] = customer.Status;
                    }
            }

            DiagramUsersDatas = DiagramUsersDatas.OrderBy(i => i.User).ToList();
        }

        private void CreateListBoxUsersData()
        {
            foreach (var usersData in DiagramUsersDatas)
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

                var user = new ListBoxUserData
                {
                    AverageSteps = userAverageSteps,
                    MaxSteps = maxSteps,
                    MinSteps = minSteps,
                    Name = usersData.User,
                    LBItemColor = solidColorBrush,
                    Points = points
                };
                ListBoxUsersDatas.Add(user);
            }
        }

        private void DrawCommand(object obj)
        {
            CanvasElements.Clear();
            foreach (var item in (IEnumerable) obj)
            {
                var selected = (ListBoxUserData) item;
                var polyLine = new Polyline
                {
                    Margin = new Thickness {Left = 57, Top = 68},
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Points = selected.Points
                };
                CanvasElements.Add(polyLine);
            }
        }

        private void SortByName()
        {
            var obsCollection = _sortedByName
                ? new ObservableCollection<ListBoxUserData>(ListBoxUsersDatas.OrderBy(i => i.Name))
                : new ObservableCollection<ListBoxUserData>(ListBoxUsersDatas.OrderByDescending(i => i.Name));
            ListBoxUsersDatas.Clear();
            foreach (var listBoxUserData in obsCollection)
                ListBoxUsersDatas.Add(listBoxUserData);
            _sortedByName = !_sortedByName;
        }
        private void SortByAverage()
        {
            var obsCollection = _sortedByAverage
                ? new ObservableCollection<ListBoxUserData>(ListBoxUsersDatas.OrderBy(i => i.AverageSteps))
                : new ObservableCollection<ListBoxUserData>(ListBoxUsersDatas.OrderByDescending(i => i.AverageSteps));
            ListBoxUsersDatas.Clear();
            foreach (var listBoxUserData in obsCollection)
                ListBoxUsersDatas.Add(listBoxUserData);
            _sortedByAverage = !_sortedByAverage;
        }
        private void SortByMax()
        {
            var obsCollection = _sortedByMax
                ? new ObservableCollection<ListBoxUserData>(ListBoxUsersDatas.OrderBy(i => i.MaxSteps))
                : new ObservableCollection<ListBoxUserData>(ListBoxUsersDatas.OrderByDescending(i => i.MaxSteps));
            ListBoxUsersDatas.Clear();
            foreach (var listBoxUserData in obsCollection)
                ListBoxUsersDatas.Add(listBoxUserData);
            _sortedByMax = !_sortedByMax;
        }
        private void SortByMin()
        {
            var obsCollection = _sortedByMin
                ? new ObservableCollection<ListBoxUserData>(ListBoxUsersDatas.OrderBy(i => i.MinSteps))
                : new ObservableCollection<ListBoxUserData>(ListBoxUsersDatas.OrderByDescending(i => i.MinSteps));
            ListBoxUsersDatas.Clear();
            foreach (var listBoxUserData in obsCollection)
                ListBoxUsersDatas.Add(listBoxUserData);
            _sortedByMin = !_sortedByMin;
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