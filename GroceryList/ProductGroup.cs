using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroceryList
{
    class ProductGroup
    {
        private int Index;
        private string Name;

        public ProductGroup(int _Index, string _Name)
        {
            this.Index = _Index;
            this.Name = _Name;
        }

        public string GetName()
        {
            return this.Name;
        }

        public int GetIndex()
        {
            return this.Index;
        }
    }
}
