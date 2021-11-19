using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace Assignment2
{


    public class RSS
    {
        public string title { get; set; }
        public string url { get; set; }
        public string date { get; set; }

    }
    public class Article
    {
        public string Title { get; set; }
        public DateTime Datum { get; set; }
        public string HeadTitleForHomePage { get; set; }

    }


    public partial class MainWindow : Window
    {
        private Thickness spacing = new Thickness(5);
        private HttpClient http = new HttpClient();
        // We will need these as instance variables to access in event handlers.
        private TextBox addFeedTextBox;
        private Button addFeedButton;
        private ComboBox selectFeedComboBox;
        private Button loadArticlesButton;
        private StackPanel articlePanel;
        public List<RSS> RSSs = new List<RSS>();



        public MainWindow()
        {
            InitializeComponent();
            Start();

        }

        private void Start()
        {
            // Window options
            Title = "Feed Reader";
            Width = 800;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Scrolling
            var root = new ScrollViewer();
            root.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Content = root;

            // Main grid
            var grid = new Grid();
            root.Content = grid;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var addFeedLabel = new Label
            {
                Content = "Feed URL:",
                Margin = spacing
            };
            grid.Children.Add(addFeedLabel);

            addFeedTextBox = new TextBox
            {
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(addFeedTextBox);
            Grid.SetColumn(addFeedTextBox, 1);

            addFeedButton = new Button
            {
                Content = "Add Feed",
                Margin = spacing,
                Padding = spacing

            };
            grid.Children.Add(addFeedButton);
            Grid.SetColumn(addFeedButton, 2);

            addFeedButton.Click += new RoutedEventHandler(buttonClick);

            var selectFeedLabel = new Label
            {
                Content = "Select Feed:",
                Margin = spacing
            };
            grid.Children.Add(selectFeedLabel);
            Grid.SetRow(selectFeedLabel, 1);

            selectFeedComboBox = new ComboBox
            {
                Margin = spacing,
                Padding = spacing,
                IsEditable = false
            };
            grid.Children.Add(selectFeedComboBox);

            Grid.SetRow(selectFeedComboBox, 1);
            Grid.SetColumn(selectFeedComboBox, 1);

            loadArticlesButton = new Button
            {
                Content = "Load Articles",
                Margin = spacing,
                Padding = spacing
            };
            grid.Children.Add(loadArticlesButton);
            Grid.SetRow(loadArticlesButton, 1);
            Grid.SetColumn(loadArticlesButton, 2);

            loadArticlesButton.Click += new RoutedEventHandler(buttonClick2);

            articlePanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = spacing
            };
            grid.Children.Add(articlePanel);
            Grid.SetRow(articlePanel, 2);
            Grid.SetColumnSpan(articlePanel, 3);

            string allFeeds = "All Feeds";
            selectFeedComboBox.Items.Add(allFeeds);

        }

        private async Task<RSS> LoadDocumentAsync(RSS rss)
        {

            //await Task.Delay(1000);
            var response = await http.GetAsync(rss.url);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var feed = XDocument.Load(rss.url);
            return rss;

        }
        public async void buttonClick(object sender, EventArgs e)
        {

            string path = addFeedTextBox.Text;
            var document = XDocument.Load(path);
            await Task.Delay(1000);
            string title = document.Descendants("title").First().Value;
            string url = addFeedTextBox.Text;

            selectFeedComboBox.Items.Add(title);

            RSSs.Add(new RSS() { title = title, url = path });

            addFeedButton.IsEnabled = false;
            await Task.Run(() => XDocument.Load(path));
            addFeedButton.IsEnabled = true;

        }



        public async void buttonClick2(object sender, EventArgs e)
        {

            var nrFromDropDown = selectFeedComboBox.SelectedIndex;
            if (nrFromDropDown == 0)
            {
                var tasks = RSSs.Select(LoadDocumentAsync).ToList();
                var remainingTasks = tasks.ToList();
                articlePanel.Children.Clear();
                while (remainingTasks.Count() > 0)
                {

                    var task = await Task.WhenAny(remainingTasks);

                    remainingTasks.Remove(task);

                    int i = tasks.IndexOf(task);

                    var awaitFeeds = await task;

                    var movie = RSSs[i];

                }



                var rssList = new List<Article>();

                foreach (var item in RSSs)
                {
                    var documentX = XDocument.Load(item.url);

                    var TitleHomePage2 = documentX.Descendants("title").First().Value;

                    var titleX = documentX.Descendants("title").Select(t => t.Value).ToArray();

                    var dateX = documentX.Descendants("pubDate").Select(t => t.Value).ToArray();

                    loadArticlesButton.IsEnabled = false;
                    await Task.Run(() => XDocument.Load(item.url));
                    loadArticlesButton.IsEnabled = true;

                    for (int i = 1; i < 6; i++)
                    {
                        rssList.Add(new Article { Title = titleX[i + 1], Datum = DateTime.ParseExact(dateX[i - 1].Substring(0, 25), "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture), HeadTitleForHomePage = TitleHomePage2 });

                    }
                }
                var sortedArticles = rssList.OrderByDescending(t => t.Datum);

                foreach (var a in sortedArticles)
                {


                    var articleTitle = new TextBlock
                    {
                        Text = a.Datum + " - " + a.Title,
                        FontWeight = FontWeights.Bold,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    articlePanel.Children.Add(articleTitle);

                    var articleWebsite = new TextBlock
                    {
                        Text = a.HeadTitleForHomePage
                    };
                    articlePanel.Children.Add(articleWebsite);


                }
            }
        
            else
            {

                var movie = RSSs[nrFromDropDown - 1];

                articlePanel.Children.Clear();

                string path = movie.url;

                var document = XDocument.Load(path);

                var title = document.Descendants("title").First().Value;

                var allTitles = document.Descendants("title").Select(t => t.Value).ToArray();

                var allpubDates = document.Descendants("pubDate").Select(t => t.Value).ToArray();

                var articleList = new List<Article>();

                for (int i = 1; i < 6; i++)
                {
                    articleList.Add(new Article { Title = allTitles[i + 1], Datum = DateTime.ParseExact(allpubDates[i - 1].Substring(0, 25), "ddd, dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture), HeadTitleForHomePage = title });

                }
                var sortedArticles = articleList.OrderByDescending(t => t.Datum);

                foreach (var a in sortedArticles)
                {
                    var articlePlaceholder = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Margin = spacing
                    };
                    articlePanel.Children.Add(articlePlaceholder);

                    var articleTitle = new TextBlock
                    {
                        Text = a.Datum + " - " + a.Title,
                        FontWeight = FontWeights.Bold,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    articlePlaceholder.Children.Add(articleTitle);

                    var articleWebsite = new TextBlock
                    {
                        Text = a.HeadTitleForHomePage
                    };
                    articlePlaceholder.Children.Add(articleWebsite);

                    loadArticlesButton.IsEnabled = false;
                    await Task.Run(() => XDocument.Load(path));
                    loadArticlesButton.IsEnabled = true;

                }
            }
        }
    }
}