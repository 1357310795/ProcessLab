using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using ProcessLab.Helpers;
using ProcessLab.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace ProcessLab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [INotifyPropertyChanged]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NetService.Init();
            this.DataContext = this;
        }

        [ObservableProperty]
        double process;

        [ObservableProperty]
        List<int> steps = Enumerable.Range(0, 27).ToList();

        [ObservableProperty]
        int selectedStep;

        [ObservableProperty]
        string processInputData;

        [ObservableProperty]
        string processOutputData;

        [ObservableProperty]
        BitmapSource dopingImage;

        [ObservableProperty]
        string message;

        string[] processOutputDatas = new string[27];
        BitmapSource[] dopingImages = new BitmapSource[27];

        [ObservableProperty]
        bool getImage;

        bool busy;

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (busy)
            {
                MessageBox.Show("请耐心等待完成。");
                return;
            }
            Task.Run(() => { 
                Go(); 
            });
        }

        private void Go()
        {
            busy = true;
            try
            {
                for (int i = 0; i <= 26; i++)
                {
                    Message = $"正在制造中（{i}/26）";
                    Process = ((double)i) / 26;

                    var path = Path.Combine(PathHelper.AppPath, "data", $"{i}.json");
                    var data = File.ReadAllText(path);

                    var res = LabService.Measure(data);

                    if (!res.Success)
                    {
                        Debug.WriteLine($"step {i}: {res.Message}");
                        return;
                    }

                    if (res.Result.Code != "1")
                    {
                        Debug.WriteLine($"step {i}: {res.Result.Msg}");
                        return;
                    }

                    Debug.WriteLine($"step {i}: ok");
                    processOutputDatas[i] = res.Result.Data.Output;

                    if (GetImage)
                    {
                        var res3 = LabService.NetActiveDraw(data);

                        if (!res3.Success)
                        {
                            Debug.WriteLine($"step {i}: {res3.Message}");
                            return;
                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            dopingImages[i] = BitmapFrame.Create(res3.Result);
                        });
                    }
                
                }
                var res2 = LabService.GetAuth();

                if (!res2.Success)
                {
                    Debug.WriteLine($"final: {res2.Message}");
                    return;
                }

                if (res2.Result.Code != "1")
                {
                    Debug.WriteLine($"final: {res2.Result.Msg}");
                    return;
                }

                Debug.WriteLine($"final: {res2.Result.Data}");
                MessageBox.Show("制造完成，点击确定打开网页");
                Message = "制造完成";

                LaunchHelper.OpenURL($"http://123.57.66.238/3biuypozhnitff2p/3biu/?{res2.Result.Data}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            finally
            {
                busy = false;
            }
        }

        partial void OnSelectedStepChanged(int value)
        {
            var path = Path.Combine(PathHelper.AppPath, "data", $"{value}.json");
            var data = File.ReadAllText(path);
            ProcessInputData = data;
            ProcessOutputData = processOutputDatas[value];
            DopingImage = dopingImages[value];
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(PathHelper.AppPath, "data", $"{SelectedStep}.json");
            File.WriteAllText(path, ProcessInputData);
            MessageBox.Show("数据已保存，请再次确认格式是否正确");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add((BitmapFrame)DopingImage);

                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "PNG files| *.png";
                dialog.FileName = $"step_{SelectedStep}.png";

                if (dialog.ShowDialog() != false)
                {
                    using (var stream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }
                }
            });
        }
    }
}