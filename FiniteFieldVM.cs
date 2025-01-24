using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MathUtils.Combinatorics;

namespace FFG2
{
    public class FiniteFieldVM
    {
        #region Properties
        public int Characteristic { get; }
        public int DegreeOfExtension { get; }
        public int Order => (int)Math.Pow(Characteristic, DegreeOfExtension);
        private List<Polynomial> _fieldElements;
        public ReadOnlyCollection<Polynomial> FieldElements => _fieldElements.AsReadOnly();
        public Polynomial IrreducibleElement { get; }
        public Polynomial PrimitiveElement { get; }
        public DataTable AdditionTable { get; private set; }
        public DataTable MultiplicationTable { get; private set; }
        public List<string> PE_Darstellung { get; private set; }
        #endregion

        #region Constructors and Initialization
        private FiniteFieldVM(int p, int k)
        {
            this.Characteristic = p;
            this.DegreeOfExtension = k;
            SetFieldElements();
            IrreducibleElement = GetIrreduciblePolynomialOfExtensionDegree();
            PrimitiveElement = GetPrimitiveElement();
        }

        
        private async Task<FiniteFieldVM> InitializeAsync()
        {
            List<Task<DataTable>> tasks = new List<Task<DataTable>>();
            var task1 = ComputeAdditionTableAsync();
            var task2 = ComputeMultiplicationTableAsync();
            var task3 = CreatePEViewAsync();
            tasks.Add(task1);
            tasks.Add(task2);
            while(tasks.Any())
            {
                var finishedTask = await Task.WhenAny(tasks);
                if(finishedTask == task1)
                {
                    this.AdditionTable = await finishedTask;
                }
                if(finishedTask == task2)
                {
                    this.MultiplicationTable = await finishedTask;
                }
                tasks.Remove(finishedTask);                
            }
            //this.AdditionTable = await ComputeAdditionTableAsync();
            //this.MultiplicationTable = await ComputeMultiplicationTableAsync();
            this.PE_Darstellung = await CreatePEViewAsync();
            // ToDo: Perform both tasks in parallel?
            return this;
        }

        
        public static Task<FiniteFieldVM> CreateAsync(int p, int k)
        {
            FiniteFieldVM fieldVM = new FiniteFieldVM(p, k);
            return fieldVM.InitializeAsync();
        }
        #endregion

        /// <summary>
        /// Compute the addition table for this field's elements
        /// </summary>
        /// <returns>A DataTable where the (i,j)-cell represents the sum of the i-th and j-th field element.</returns>
        private async Task<DataTable> ComputeAdditionTableAsync()
        {
            DataTable table = new DataTable("AdditionTable");
            table.Columns.Add("+", typeof(Polynomial));

            // This is probably never worth the overhead of running it asynchronously?
            // There's no computation, just creating the columns.
            await Task.Run(() =>
            {
                for (int i = 0; i < FieldElements.Count; i++)
                {
                    DataColumn col = new DataColumn(i.ToString(), typeof(Polynomial));
                    col.Caption = FieldElements[i].ToString();
                    table.Columns.Add(col);
                }
            });

            // Compute the cell entries of the addition table asynchronously
            await Task.Run(() =>
            {
                for (int i = 0; i < FieldElements.Count; i++)
                {
                    DataRow row = table.NewRow();
                    row["+"] = FieldElements[i];
                    for (int k = 0; k < FieldElements.Count; k++)
                    {
                        row[k.ToString()] = FieldElements[i] + FieldElements[k];
                    }
                    table.Rows.Add(row);
                }
            });
            return table;
        }

        /// <summary>
        /// Compute the multiplication table for this field's elements
        /// </summary>
        /// <returns>A DataTable where the (i,j)-cell represents the product of the i-th and j-th field element (modulo the irreducible element).</returns>
        private async Task<DataTable> ComputeMultiplicationTableAsync()
        {
            DataTable table = new DataTable("MultiplicationTable");
            table.Columns.Add("*", typeof(Polynomial));

            // This is probably never worth the overhead of running it asynchronously?
            // There's no computation, just creating the columns.
            await Task.Run(() =>
            {
                for (int i = 0; i < FieldElements.Count; i++)
                {
                    DataColumn col = new DataColumn(i.ToString(), typeof(Polynomial));
                    col.Caption = FieldElements[i].ToString();
                    table.Columns.Add(col);
                }
            });

            // Compute the cell entries of the multiplication table asynchronously
            await Task.Run(() =>
            {
                for (int i = 0; i < FieldElements.Count; i++)
                {
                    DataRow row = table.NewRow();
                    row["*"] = FieldElements[i];
                    for (int k = 0; k < FieldElements.Count; k++)
                    {
                        row[k.ToString()] = Polynomial.Multiply(FieldElements[i], FieldElements[k], this.IrreducibleElement);
                    }
                    table.Rows.Add(row);
                }
            });
            return table;
        }
        
        private async Task<List<string>> CreatePEViewAsync()
        {
            List<string> output = new List<string>();
            await Task.Run(() =>
            {
                output.Add(0.ToString());
                output.Add("a       =  " + this.PrimitiveElement.ToString());
                for (int i = 1; i < Math.Pow(this.Characteristic, this.DegreeOfExtension) - 1; i++)
                {
                    Polynomial it = new Polynomial(this.PrimitiveElement.CoeffList, this.Characteristic);
                    for (int n = 0; n < i; n++)
                    {
                        it = Polynomial.Multiply(it, this.PrimitiveElement, this.IrreducibleElement);
                    }
                    output.Add($"a^{i + 1}  =  " + it.ToString());
                }
            });
            return output;
        }

        private void SetFieldElements()
        {
            _fieldElements = new List<Polynomial>();
            var perm = GetPermutationsWithRept(Enumerable.Range(0, Characteristic), DegreeOfExtension);
            foreach(var item in perm)
            {
                _fieldElements.Add(new Polynomial(item, Characteristic));
            }
            _fieldElements.Sort();
        }

        /// <summary>
        /// Find an irreducible polynomial
        /// </summary>
        /// <returns>An irreducible polynomial of the field extension's degree.</returns>
        private Polynomial GetIrreduciblePolynomialOfExtensionDegree()
        {
            // Get a reference to the zero polynomial
            Polynomial zeroP = Polynomial.GetZeroPolynomial(Characteristic);
            // Get an enumeration of all monic polynomials of degree DegreeOfExtension
            var candidatesOfExtensionDegree = GetPermutationsWithRept(Enumerable.Range(0, Characteristic), DegreeOfExtension)
                .Select(item => new Polynomial(item.Append(1), Characteristic));
            // Possible divisors have degree >= 1 and <= DegreeOfExtension / 2
            var polsToTest = FieldElements.Where(pol => (pol.Degree >= 1 && pol.Degree <= DegreeOfExtension / 2)).ToList();
            
            // Test each candidate polynomial for irreducibility, i.e. test for divisors
            foreach (Polynomial pol in candidatesOfExtensionDegree)
            {
                if(polsToTest.All(q => pol % q != zeroP))
                { // No polynomial divides pol ==> pol is irreducible
                    return pol;
                }
            }
            return null;
        }

        /// <summary>
        /// Find a primitive element
        /// </summary>
        /// <returns>A primitive field element, i.e. a generator of the field's multiplicative group.</returns>
        private Polynomial GetPrimitiveElement()
        {
            Polynomial oneP = Polynomial.GetOnePolynomial(Characteristic);
            return FieldElements.First(p => p.Degree >= 0 && p != oneP && GetMultiplicativeOrder(p) == Math.Pow(Characteristic, DegreeOfExtension) - 1);            
        }
        
        /// <summary>
        /// Compute the multiplicative order of a field element but only checks facotors of Order - 1 
        /// </summary>
        /// <param name="f">A field element</param>
        /// <returns>The multiplicative order of f, i.e. the smallest m > 0 such that f^m = 1 (mod IrreducibleElement).</returns>
        public int GetMultiplicativeOrder(Polynomial f)
        {
            var list = Enumerable.Range(2, Order - 1).ToList();
            List<int> factors = list.Where(x => (Order -1) % x == 0).ToList();

            Polynomial oneP = Polynomial.GetOnePolynomial(Characteristic);

            if (f == oneP)
            {
                return 1;
            }
            for (int i = 0; i < factors.Count - 1; i++)
            {
                if (f.Power(factors[i], IrreducibleElement) == oneP)
                {
                    return factors[i];
                }
            }
            return Order - 1;

            //Polynomial g = oneP;
            //int ord = 0;
            //do
            //{
            //    g = Polynomial.Multiply(f, g, IrreducibleElement);
            //    ord++;
            //} while (g != oneP);
            //return ord;
        }

        //public Polynomial Multiply(Polynomial f, Polynomial g)
        //{
        //    return Polynomial.Multiply(f, g, this.IrreducibleElement);
        //}
        //public Polynomial Divide(Polynomial f, Polynomial g)
        //{
        //    return Polynomial.Divide(f, g, this.IrreducibleElement);
        //}
    }
}
