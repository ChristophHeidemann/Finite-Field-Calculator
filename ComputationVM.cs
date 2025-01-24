using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFG2
{
    public enum Op
    {
        None = 0, Plus, Minus, Times, Divides, Inverse, MultiplicativeOrder
    }

    public class ComputationVM : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Interface members
        public event PropertyChangedEventHandler PropertyChanged;
        protected internal void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion


        #region Properties
        public int BasePrime { get; }
        public Polynomial OneP { get; }
        public Polynomial ZeroP { get; }
        public Polynomial IrreducibleElement { get; }
        
        private Polynomial _term1;
        public Polynomial Term1
        {
            get { return _term1; }
            set
            {
                _term1 = value;
                NotifyPropertyChanged();
            }
        }
        private Polynomial _term2;
        public Polynomial Term2
        {
            get { return _term2; }
            set
            {
                _term2 = value;
                NotifyPropertyChanged();
            }
        }
        // term3 is used for Buttons "Inverse" and "Mult.O." so it always returns for the last selected Item
        private Polynomial _term3;
        public Polynomial Term3
        {
            get { return _term3; }
            set
            {
                _term3 = value;
                NotifyPropertyChanged();
            }
        }
        // _useResultHelper ist used for hte Button "Result" and is not bound to anything
        private Polynomial _useResultHelper;
        public Polynomial UseResultHelper
        {
            get { return _useResultHelper; }
            set
            {
                _useResultHelper = value;
                NotifyPropertyChanged();
            }
        }
        private int _exponent;
        public int Exponent
        {
            get { return _exponent; }
            set
            {
                if(_exponent != value)
                {
                    _exponent = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Op _op;
        public Op Op
        {
            get { return _op; }
            set
            {
                if(_op != value)
                {
                    _op = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private Polynomial _result;
        public Polynomial Result
        {
            get { return _result; }
            set
            {
                _result = value;
                NotifyPropertyChanged();
            }
        }
        #endregion


        public ComputationVM(int p, Polynomial irred)
        {
            this.BasePrime = p;
            this.IrreducibleElement = irred;
            this.OneP = Polynomial.GetOnePolynomial(p);
            this.ZeroP = Polynomial.GetZeroPolynomial(p);
        }



        public static IEnumerable<int> GetPrimeNumbers()
        {
            yield return 2;
            int p = 3;
            while (true)
            {
                if (IsPrime(p))
                {
                    yield return p;
                }
                p += 2;
            }
        }

        public static bool IsPrime(int n)
        {
            if (n < 2)
                return false;
            else if (n == 2)
                return true;
            else
            {
                int bound = (int)(Math.Sqrt(n));
                for (int i = 3; i <= bound; i += 2)
                {
                    if (n % i == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static (int p, int k) GetPrimePowerDecomposition(int n)
        {
            if (n < 2)
                throw new ArgumentException("n must be >1.");

            int p = 0, k = 0;
            var iterator = ComputationVM.GetPrimeNumbers().GetEnumerator();
            while (iterator.MoveNext())
            {
                p = iterator.Current;
                //if (p > Math.Sqrt(n))
                //{
                //    throw new ArgumentException("This was not a Primepower.");                    
                //}
                if (n % p == 0)
                {
                    int prod = 1;
                    do
                    {
                        k++;
                        prod *= p;
                    } while (prod < n);
                    if (prod == n)
                    {
                        return (p, k);
                    }
                    else
                    {
                        throw new ArgumentException("This was not a Primepower.");
                    }
                    
                }                
            }
            //dummy
            return (p, k);
        }
    }
}
