using System.Windows;

namespace test2_Even_or_Odd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void Start_Judgement_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(Input_Num.Text, out int In_Num))
            {
                MessageBox.Show("数字を入力してください");
                return;
            }
            Judgement_Even_or_Odd(In_Num);
        }

        public void Judgement_Even_or_Odd(int In_Num)
        {
            if(In_Num % 2 ==0) 
            {
                Display_Result.Text = "偶数です";
            }
            else
            {
                Display_Result.Text = "奇数です";
            }
        }
    }
}