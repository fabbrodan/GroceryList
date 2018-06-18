using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroceryList
{
    class Product
    {
        private int Index;
        private int GroupIndex;
        private string Name;
        private double Price;

        public Product(int _Index, int _GroupIndex, string _Name, double _Price)
        {
            this.Index = _Index;
            this.GroupIndex = _GroupIndex;
            this.Name = _Name;
            this.Price = _Price;
        }

        public int GetGroupIndex()
        {
            return this.GroupIndex;
        }

        public int GetIndex()
        {
            return this.Index;
        }

        public string GetName()
        {
            return this.Name;
        }

        public double GetPrice()
        {
            return this.Price;
        }
    }
}
