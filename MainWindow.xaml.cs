using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FFG2
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        const string PRIMES_FILE = "PrimeNumbers.lst";

        private List<int> primes = new List<int>();

        private FiniteFieldVM fieldVM;
        private ComputationVM compVM;


		public MainWindow()
		{
			InitializeComponent();
            Polynomial r1 = new Polynomial(new List<int> { 0, 1}, 3);
            Polynomial ird = new Polynomial(new List<int> { 1, 0, 2, 1 }, 3);
            Polynomial t = r1.Power(5, ird);
			//ReadPrimeList(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PRIMES_FILE));
		}

		/*
		private void ReadPrimeList(string filePath)
		{
			if(!File.Exists(filePath))
            {
                try
                {
					File.WriteAllText(filePath, "2");
				}
				catch(Exception ex)
                {
					MessageBox.Show($"Could not process file {filePath}.{Environment.NewLine}{ex.Message}", "File access error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}
			string primeNumberString = File.ReadAllText(filePath);
			string[] primeNumberSplitString = primeNumberString.Split(',');
			primes = primeNumberSplitString.Select(n => int.Parse(n)).ToList();

		}

		private void WriteUpdatedPrimeNumbersFile()
		{
			string erg = string.Join(",", primes.Select(n => n.ToString()).ToArray());
			File.WriteAllText("PrimeNumbers.lst", erg);
		}
		*/

		

		#region Button Click Events
		private async void Btn_Go_Click(object sender, RoutedEventArgs e)
		{
            try
            {
                if (int.TryParse(Txb_Input.Text, out int primePower))
                {
                    (int p, int k) = ComputationVM.GetPrimePowerDecomposition(primePower);

                    fieldVM = await FiniteFieldVM.CreateAsync(p, k);
                    this.DataContext = fieldVM;

                    compVM = new ComputationVM(fieldVM.Characteristic, fieldVM.IrreducibleElement);
                    Grd_OperationsButtons.DataContext = compVM;
                    Stk_CalculatorDisplay.DataContext = compVM;
                }
                else
                {
                    MessageBox.Show("Input format could not be read as an integer number.", "Invalid input", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Inavalid input.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
			
		}

		private void Btn_Solve_Click(object sender, RoutedEventArgs e)
		{
            if(!(compVM.Term2 is null))
            {
                switch (compVM.Op)
                {
                    case Op.Plus:
                        compVM.Result = compVM.Term1 + compVM.Term2;
                        compVM.UseResultHelper = compVM.Result;
                        break;
                    case Op.Minus:
                        compVM.Result = compVM.Term1 - compVM.Term2;
                        compVM.UseResultHelper = compVM.Result;
                        break;
                    case Op.Times:
                        compVM.Result = Polynomial.Multiply(compVM.Term1, compVM.Term2, compVM.IrreducibleElement);
                        compVM.UseResultHelper = compVM.Result;
                        break;
                    case Op.Divides:
                        compVM.Result = Polynomial.Multiply(compVM.Term1, compVM.Term2.GetInverseModulo(compVM.IrreducibleElement), compVM.IrreducibleElement);
                        compVM.UseResultHelper = compVM.Result;
                        break;
                    case Op.Inverse:
                        if (!(compVM.Term1 is null))
                        {
                            compVM.Result = compVM.Term1.GetInverseModulo(compVM.IrreducibleElement);
                            compVM.UseResultHelper = compVM.Result;
                        }
                        break;
                    case Op.MultiplicativeOrder:
                        if (!(compVM.Term1 is null))
                        {
                            if (compVM.Term1 == compVM.ZeroP)
                            {
                                compVM.Result = compVM.ZeroP;
                                compVM.UseResultHelper = compVM.Result;
                            }
                            else
                            {
                                compVM.Result = new Polynomial(fieldVM.GetMultiplicativeOrder(compVM.Term1), fieldVM.Characteristic);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }			
        }

		private void Btn_Operation_Click(object sender, RoutedEventArgs e)
		{
			Button btn = (Button)e.Source;
			compVM.Op = (Op)btn.Tag;
		}

        private void Btn_Inverse_Click(object sender, RoutedEventArgs e)
        {
            if (!(compVM.Term3 is null))
            {
                compVM.Result = compVM.Term3.GetInverseModulo(compVM.IrreducibleElement);
            }
        }

        private void Btn_MultOrder_Click(object sender, RoutedEventArgs e)
        {

            if (!(compVM.Term3 is null))
            {
                if (compVM.Term3 == compVM.ZeroP)
                {
                    compVM.Result = compVM.ZeroP;
                }
                else
                {
                    compVM.Result = new Polynomial(fieldVM.GetMultiplicativeOrder(compVM.Term3), fieldVM.Characteristic);
                }
            }
        }

        private void Btn_Clear_Click(object sender, RoutedEventArgs e)
		{
			compVM.Term1 = null;
			compVM.Term2 = null;
			compVM.Result = null;
			compVM.Op = Op.None;
		}

        private void Btn_UseRes_Click(object sender, RoutedEventArgs e)
        {
            if (compVM.Result is null)
            {
                if (compVM.UseResultHelper is null)
                {
                    return;
                }
                if (compVM.Op == Op.None)
                {
                    compVM.Term1 = compVM.UseResultHelper;
                }
                else
                {
                    compVM.Term2 = compVM.UseResultHelper;
                }
            }
            else
            {
                compVM.Term1 = compVM.UseResultHelper;
                compVM.Term2 = null;
                compVM.Op = Op.None;
                compVM.Result = null;
            }
        }

        private void Btn_Power_Click(object sender, RoutedEventArgs e)
        {
            if (compVM.Exponent == 0)
            {
                compVM.Result = compVM.OneP;
            }
            else if (compVM.Exponent > 0)
            {
                compVM.Result = compVM.Term1.Power(compVM.Exponent, compVM.IrreducibleElement);
            }
            else if (compVM.Exponent < 0)
            {
                compVM.Result = compVM.Term1.GetInverseModulo(compVM.IrreducibleElement).Power(-compVM.Exponent, compVM.IrreducibleElement);
            }
        }
        #endregion





        // the first inner if is not properly testet
        // it should have the effect of clearing the output when one wishes to calculate further 
        // without pressing clear everytime
        private void Ltb_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{            
            if (e.AddedItems.Count > 0)
            {
                if (!(compVM.Result is null) && !(compVM.Term2 is null))
                {
                    compVM.Result = null;
                    compVM.Term2 = null;
                    compVM.Op = Op.None;
                }
                if (compVM.Op == Op.None)
                {

                    compVM.Term1 = (Polynomial)e.AddedItems[0];
                    compVM.Term3 = (Polynomial)e.AddedItems[0];
                }
                else
                {
                    compVM.Term2 = (Polynomial)e.AddedItems[0];
                    compVM.Term3 = (Polynomial)e.AddedItems[0];
                }
            }
			
		}

		private void Dg_AddT_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
		{
			e.Column.Header = fieldVM.AdditionTable.Columns[e.PropertyName].Caption;
		}
		private void Dg_MultT_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
		{
			e.Column.Header = fieldVM.MultiplicationTable.Columns[e.PropertyName].Caption;
		}
        
		
    }
}