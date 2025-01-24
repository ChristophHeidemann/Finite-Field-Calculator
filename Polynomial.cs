using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtils.Combinatorics;

namespace FFG2
{
	public class Polynomial : IComparable, IEquatable<Polynomial>
	{
		#region IComparable Interface members
		public int CompareTo(object obj)
		{
			if (obj is null)
				return 1;
			if(obj is Polynomial pol)
            {
				int order;
				if (this == pol)
				{
					order = 0;
				}
				else
                {
                    order = this < pol ? -1 : 1;
                }
                return order;
			}
			else
				throw new ArgumentException("Object is not a Polynomial");
		}
        #endregion

        #region Comparing and Equating operators and methods
        public bool Equals(Polynomial other)
		{
			if (other is null)
				return false;
			return this == other;
		}
        public override bool Equals(object obj)
        {
			if (obj is null)
				return false;
			if(obj is Polynomial pol)
            {
				return this.Equals(pol);
            }
            else
            {
				return false;
            }
        }
        public override int GetHashCode()
        {
			return this.BasePrime.GetHashCode() + this._coeffList.Sum(c => c.GetHashCode());
        }
		public static bool operator ==(Polynomial f, Polynomial g)
		{
			if (f.BasePrime != g.BasePrime)
				return false;
			if (f.Degree != g.Degree)
			{
				return false;
			}
			else
			{
				for (int i = 0; i < f._coeffList.Count; i++)
				{
					if (f._coeffList[i] != g._coeffList[i])
					{
						return false;
					}
				}
				return true;
			}
		}
		public static bool operator !=(Polynomial f, Polynomial g)
		{
			return !(f == g);
		}
		public static bool operator <(Polynomial f, Polynomial g)
		{
			if(f.Degree == g.Degree)
            {
				for(int i = f._coeffList.Count - 1; i >= 0; i--)
                {
					if(f._coeffList[i] != g._coeffList[i])
                    {
						return f._coeffList[i] < g._coeffList[i];
                    }
                }
				return false;
            }
            else
            {
				return f.Degree < g.Degree;
            }
		}
		public static bool operator >(Polynomial f, Polynomial g)
		{
			if (f.Degree == g.Degree)
			{
				for (int i = f._coeffList.Count - 1; i >= 0; i--)
				{
					if (f._coeffList[i] != g._coeffList[i])
					{
						return f._coeffList[i] > g._coeffList[i];
					}
				}
				return false;
			}
			else
			{
				return f.Degree > g.Degree;
			}
		}
		public static bool operator <=(Polynomial f, Polynomial g)
		{
			if (f.Degree == g.Degree)
			{
				for (int i = f._coeffList.Count - 1; i >= 0; i--)
				{
					if (f._coeffList[i] != g._coeffList[i])
					{
						return f._coeffList[i] < g._coeffList[i];
					}
				}
				return true;
			}
			else
			{
				return f.Degree < g.Degree;
			}
		}
		public static bool operator >=(Polynomial f, Polynomial g)
		{
			if (f.Degree == g.Degree)
			{
				for (int i = f._coeffList.Count - 1; i >= 0; i--)
				{
					if (f._coeffList[i] != g._coeffList[i])
					{
						return f._coeffList[i] > g._coeffList[i];
					}
				}
				return true;
			}
			else
			{
				return f.Degree > g.Degree;
			}
		}
		#endregion


		#region Properties
		private List<int> _coeffList;
		public ReadOnlyCollection<int> CoeffList => _coeffList.AsReadOnly();

		public int BasePrime { get; }
		public int Degree
		{
			get
			{
				if (this._coeffList.All(c => c == 0))
					return -1;
				return this._coeffList.Count - 1;
			}
		}
        #endregion

        #region Constructors
        public Polynomial(IEnumerable<int> coeffs, int p)
		{
			List<int> coefficients;
			if (coeffs is null)
				throw new ArgumentNullException(nameof(coeffs), "The coefficient list cannot be null.");
			else 
				coefficients = coeffs.Any() ? new List<int>(coeffs) : new List<int>() { 0 };
			while(coefficients.Count > 1 && coefficients.Last() == 0)
            {
				coefficients.RemoveAt(coefficients.Count - 1);
			}

			this._coeffList = coefficients;
			this.BasePrime = p;
		}
		public Polynomial(int cons, int p)
		{
			this._coeffList = new List<int>() { cons };
			this.BasePrime = p;
		}
        #endregion

        #region Methods
        /// <summary>
		/// Gets the zero polynomial
		/// </summary>
		/// <param name="p">The base prime</param>
		/// <returns>The zero polynomial for the given base prime</returns>
		public static Polynomial GetZeroPolynomial(int p)
        {
			return new Polynomial(new int[] { 0 }, p);
        }
		/// <summary>
		/// Gets the constant one polynomial
		/// </summary>
		/// <param name="p">The base prime</param>
		/// <returns>The one-polynomial for the given base prime</returns>
		public static Polynomial GetOnePolynomial(int p)
		{
			return new Polynomial(new int[] { 1 }, p);
		}

		/// <summary>
		/// Performs Euclidean polynomial division.
		/// </summary>
		/// <param name="f">The dividend polynomial</param>
		/// <param name="g">The divisor polynomial</param>
		/// <returns>A tupel (q, r) such that f = q*g + r</returns>
		public static (Polynomial quotient, Polynomial remainder) PolynomialQuotientRemainder(Polynomial f, Polynomial g)
		{
			if (g.Degree < 0)
				throw new InvalidOperationException("Cannot divide by the zero polynomial.");
			int p = f.BasePrime;
			if (f.Degree < g.Degree)
			{
				return (Polynomial.GetZeroPolynomial(p), new Polynomial(f._coeffList, p));
			}

			Polynomial q = Polynomial.GetZeroPolynomial(p);
			Polynomial r = new Polynomial(f._coeffList, p);
			Polynomial s;

			int d = g.Degree;
			int lc = g.Lc();

			while (r.Degree >= d)
			{
				s = new Polynomial(Enumerable.Repeat(0, r.Degree - d).Append(Divide(r.Lc(), lc, p)), p);
				//s = r.Lt() / g.Lt();
				q = q + s;
				r = r - s * g;
			}
			return (q, r);
		}
		/// <summary>
		/// Gets the leading coefficient
		/// </summary>
		/// <returns>The leading coefficient of this polynomial instance</returns>
		public int Lc()
		{
			return this._coeffList[CoeffList.Count - 1];
		}
		/// <summary>
		/// Gets the leading term
		/// </summary>
		/// <returns>The leading term of this polynomial, i.e. the leading coefficient times the leading monomial</returns>
		public Polynomial Lt()
		{
			if (this.Degree < 0)
				throw new InvalidOperationException("The leading term of the zero polynomial is not defined.");
			return new Polynomial(Enumerable.Repeat(0, this.Degree).Append(this.Lc()), this.BasePrime);
		}
		/// <summary>
		/// Gets the leading monomial
		/// </summary>
		/// <returns>The leading monomial of this polynomial, i.e. the highest power product</returns>
		public Polynomial Lm()
        {
			if (this.Degree < 0)
				throw new InvalidOperationException("The leading monomial of the zero polynomial is not defined.");
			return new Polynomial(Enumerable.Repeat(0, this.Degree).Append(1), this.BasePrime);
		}
        #endregion

        #region Arithmetic Operators
        public static Polynomial operator +(Polynomial f, Polynomial g)
		{
			IEnumerable<int> coeffs;
			int p = f.BasePrime;
			int m = f._coeffList.Count;
			int n = g._coeffList.Count;
			var sumCoeffs = f._coeffList.Zip(g._coeffList, (a, b) => (a + b) % p);
			if(m > n)
            {
				coeffs = sumCoeffs.Concat(f._coeffList.Skip(n));
			}
			else if(n > m)
            {
				coeffs = sumCoeffs.Concat(g._coeffList.Skip(m));
            }
            else
            { // n == m
				coeffs = sumCoeffs;
            }
			return new Polynomial(coeffs, p);
		}
		public static Polynomial operator -(Polynomial f)
		{
			return new Polynomial(f._coeffList.Select(c => (-1 * c) + f.BasePrime), f.BasePrime);
		}
		public static Polynomial operator +(Polynomial f)
		{
			return f;
		}
		public static Polynomial operator -(Polynomial f, Polynomial g)
		{
			return f + (-g);
		}
		public static Polynomial operator *(Polynomial f, Polynomial g)
		{
			List<int> coeffs = new List<int>();
			for (int k = 0; k < f.Degree + g.Degree + 1; k++)
			{
				coeffs.Add(GetProductCoeff(f, g, k));
			}
			return new Polynomial(coeffs, f.BasePrime);

			// Local helper function
			int GetProductCoeff(Polynomial a, Polynomial b, int k)
			{
				int cauchySum = 0;
				int op1, op2;
				int p = a.BasePrime;
				for (int i = 0; i <= k; i++)
				{
					op1 = i >= a._coeffList.Count ? 0 : a._coeffList[i];
					op2 = k - i >= b._coeffList.Count ? 0 : b._coeffList[k - i];
					cauchySum += (op1 * op2) % p;
				}
				return cauchySum % p;
			}
		}
		public static Polynomial operator %(Polynomial f, Polynomial g)
		{
			if (g.Degree < 0)
				throw new InvalidOperationException("Cannot determine remainder on division by zero polynomial.");
			return Polynomial.PolynomialQuotientRemainder(f, g).remainder;
		}
		public static Polynomial operator /(Polynomial f, Polynomial g)
        {
			if (g.Degree < 0)
				throw new InvalidOperationException("Cannot determine remainder on division by zero polynomial.");
			return Polynomial.PolynomialQuotientRemainder(f, g).quotient;
		}
		

		public static Polynomial operator +(Polynomial f, int a)
		{
			while (a < 0)
				a += f.BasePrime;
			a = a % f.BasePrime;
			Polynomial ret = new Polynomial(f._coeffList.Skip(1).Prepend((f._coeffList[0] + a) % f.BasePrime), f.BasePrime);
			return ret;
		}
		public static Polynomial operator +(int a, Polynomial f)
		{
			return f + a;
		}
		public static Polynomial operator *(int a, Polynomial f)
		{
			while (a < 0)
				a += f.BasePrime;
			a = a % f.BasePrime;
			Polynomial ret = new Polynomial(f._coeffList.Select(c => (c * a) % f.BasePrime), f.BasePrime);
			return ret;
		}
		public static Polynomial operator *(Polynomial f, int a)
		{
			return a * f;
		}
        #endregion

        public static Polynomial Multiply(Polynomial f, Polynomial g, Polynomial h)
		{
			return (f * g) % h;
		}
		public static Polynomial Divide(Polynomial f, Polynomial g, Polynomial h)
        {
			return Polynomial.Multiply(f, g.GetInverseModulo(h), h);
        }

		public Polynomial GetInverseModulo(Polynomial h)
		{
			if (h.Degree == -1)
			{
				throw new ArgumentException("Module cannot be the zero polynomial.");
			}
			if(this == Polynomial.GetZeroPolynomial(this.BasePrime))
            {
				throw new InvalidOperationException("The zero polynomial has no inverse.");
            }
			Polynomial oneP = Polynomial.GetOnePolynomial(this.BasePrime);
			var allCandidates = GetPermutationsWithRept(Enumerable.Range(0, this.BasePrime), h.Degree).Select(perm => new Polynomial(perm, this.BasePrime));
			foreach (Polynomial pol in allCandidates)
			{
				if (oneP == Polynomial.Multiply(this, pol, h))
				{
					return pol;
				}
			}
			return null;
		}

		public static int Divide(int a, int b, int p)
		{
			return (a * GetInverse(b, p)) % p;
		}
		public static int GetInverse(int a, int p)
		{
			int inv = 0;
			do
			{
				inv++;
			} while ((a * inv) % p != 1);
			return inv;
		}
        /// <summary>
        /// uses the binary exponentiation method to copmute this^k 
        /// </summary>
        /// <param name="k">the Power which is to compute</param>
        /// <param name="p">the irreducible Polynomial used to compute Products</param>
        /// <returns>this Polynomial to the Power of k</returns>
        public Polynomial Power(int k, Polynomial p)
        {

            string binary = Convert.ToString(k, 2);
            Polynomial ret = new Polynomial(this.CoeffList, this.BasePrime);
            for (int i = 1; i < binary.Length; i++)
            {
                if(binary[i].ToString() == "1")
                {
                    ret = Multiply(ret, ret, p);
                    ret = Multiply(ret, this, p);
                }
                else
                {
                    ret = Multiply(ret, ret, p);
                }
            }
            return ret;
        }
		
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			if(this.Degree == -1)
			{
				return 0.ToString();
			}
			if (this.Degree == 0)
			{
				return _coeffList[0].ToString();
			}
			else if (_coeffList[0] != 0)
			{
				builder.Append($"{ _coeffList[0]} + ");
			}

			for (int k = 1; k < _coeffList.Count - 1; k++)
			{
				if (k == 1)
				{
					if (_coeffList[k] == 1)
					{
						builder.Append($"x + ");
					}
					else if (_coeffList[k] != 0)
					{
						builder.Append($"{ _coeffList[k] }x + ");
					}
				}
				else
				{
					if (_coeffList[k] == 1)
					{
						builder.Append($"x^{ k } + ");
					}
					else if (CoeffList[k] != 0)
					{
						builder.Append($"{ _coeffList[k] }x^{ k } + ");
					}
				}

			}
			if (_coeffList.Count == 2)
			{
				if (_coeffList[1] == 1)
				{
					builder.Append($"x");
				}
				else if (_coeffList[1] != 0)
				{
					builder.Append($"{ _coeffList[1] }x");
				}
				return builder.ToString();
			}
			else
			{
				if (_coeffList[_coeffList.Count - 1] == 1)
				{
					builder.Append($"x^{ _coeffList.Count - 1 }");
				}
				else if (_coeffList[_coeffList.Count - 1] != 0)
				{
					builder.Append($"{ _coeffList[_coeffList.Count - 1] }x^{ _coeffList.Count - 1 }");
				}
				return builder.ToString();
			}
		}
    }
}