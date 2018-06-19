using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GroceryList
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadProducts();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        private ProductGroup[] ProductGroups;
        private Product[] Products;
        private Product[] SelectedProdcuts = new Product[24];
        private object[,] GroceryList = new object[24,2];

        private void LoadProducts()
        {
            using(DataSet ds = new DataSet())
            {
                Stream stream = File.OpenRead(Environment.CurrentDirectory + @"\Resources\ProductGroups.xml");
                ds.ReadXml(stream, XmlReadMode.InferSchema);
                stream.Close();
                DataTable dt = ds.Tables[0];

                ProductGroups = new ProductGroup[dt.Rows.Count];

                int i = 0;
                foreach (DataRow row in dt.Rows)
                {
                    ProductGroups[i] = new ProductGroup(Int32.Parse(row.ItemArray[0].ToString()), row.ItemArray[1].ToString());
                    i++;
                }
            }

            using (DataSet ds = new DataSet())
            {
                Stream stream = File.OpenRead(Environment.CurrentDirectory + @"\Resources\Products.xml");
                ds.ReadXml(stream, XmlReadMode.InferSchema);
                stream.Close();
                DataTable dt = ds.Tables[0];

                Products = new Product[dt.Rows.Count];

                int i = 0;
                foreach (DataRow row in dt.Rows)
                {
                    Products[i] = new Product(Int32.Parse(row.ItemArray[0].ToString()), Int32.Parse(row.ItemArray[1].ToString()), row.ItemArray[2].ToString(), Double.Parse(row.ItemArray[3].ToString()));
                    i++;
                }
            }

            int x = selectablePanel.DisplayRectangle.Left + 5;
            int y = selectablePanel.DisplayRectangle.Top + 10;

            int j = 0;
            foreach (ProductGroup group in ProductGroups)
            {
                if (j > 0)
                {
                    y += 25;
                }

                Label groupLabel = new Label
                {
                    Name = group.GetName() + "Label",
                    Text = group.GetName(),
                    Location = new Point(x, y),
                    Height = 20
                };

                Product[] groupProds = Products.Where(o => o.GetGroupIndex() == group.GetIndex()).ToArray<Product>();

                y += 20;
                ComboBox groupDdwn = new ComboBox
                {
                    Name = group.GetName() + "Ddwn",
                    Location = new Point(x, y),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                foreach (Product product in groupProds)
                {
                    groupDdwn.Items.Add(product.GetName() + " " + product.GetPrice());
                }


                TextBox tbCount = new TextBox
                {
                    Name = group.GetName() + "Count",
                    Width = 25,
                    Height = 20,
                    Location = new Point(groupDdwn.DisplayRectangle.Right + 25, y),
                };

                selectablePanel.Controls.Add(groupLabel);
                selectablePanel.Controls.Add(groupDdwn);
                selectablePanel.Controls.Add(tbCount);

                tbCount.KeyPress += new KeyPressEventHandler(DisallowText);
                groupDdwn.SelectedIndexChanged += new EventHandler(OnDdwnSelect);
                tbCount.Invalidated += new InvalidateEventHandler(OnCountUpdate);
                tbCount.Leave += new EventHandler(OnCountUpdate);

                y += 5;
                j++;
            }

            Label countLabel = new Label
            {
                Text = "Count",
                Location = new Point(140, selectablePanel.DisplayRectangle.Top + 10)
            };

            selectablePanel.Controls.Add(countLabel);
        }

        private void DisallowText (object sender, KeyPressEventArgs e)
        {
            e.Handled = true;

            if (Regex.IsMatch(e.KeyChar.ToString(), "[0-9]") || char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
        }

        private void OnDdwnSelect (object sender, EventArgs e)
        {
            ComboBox obj = (ComboBox)sender;
            Panel parent = (Panel)obj.Parent;
            TextBox tb = (TextBox)parent.Controls.Find(obj.Name.Substring(0, obj.Name.Length - 4) + "Count", true)[0];

            if (obj.SelectedIndex > -1)
            {
                tb.Text = "1";
            }

            tb.Invalidate();
        }

        private void OnCountUpdate(object sender, EventArgs e)
        {
            TextBox countTb = (TextBox)sender;
            int Count = 0;
            try
            {
                Count = Int32.Parse(countTb.Text);
            }
            catch (FormatException)
            {
            }

            string objName = countTb.Name.Substring(0, countTb.Name.Length - 5);
            ComboBox cb = (ComboBox)selectablePanel.Controls.Find(objName + "Ddwn", true)[0];
            int GroupIndex = ProductGroups.FirstOrDefault(o => o.GetName() == objName).GetIndex();
            Product selectedProd = Products.FirstOrDefault(a => a.GetGroupIndex() == GroupIndex && a.GetIndex() == cb.SelectedIndex);

            if (selectablePanel.Controls.ContainsKey(objName + "PriceLabel"))
            {
                Label priceLabel = (Label)selectablePanel.Controls.Find(objName + "PriceLabel", true)[0];
                if (Count == 0 || selectedProd == null)
                {
                    priceLabel.Text = "";
                }
                else
                {
                    priceLabel.Text = (selectedProd.GetPrice() * Int32.Parse(countTb.Text)).ToString() + "$";
                }
            }
            else
            {
                Label priceLabel = new Label
                {
                    Name = objName + "PriceLabel",
                    Location = new Point(countTb.Location.X + 25, countTb.Location.Y + 5),
                    Text = (selectedProd.GetPrice() * Int32.Parse(countTb.Text)).ToString() + "$"
                };
                selectablePanel.Controls.Add(priceLabel);
            }
        }

        private void AddProduct (object sender, EventArgs e)
        {
            int x = 0;
            int count = 0;
            Product addedProd = null;
            foreach (Control control in selectablePanel.Controls)
            {
                if (control.GetType() == typeof(TextBox))
                {
                    TextBox countTb = (TextBox)control;
                    Int32.TryParse(countTb.Text, out count);
                    countTb.Text = "";
                }

                if (control.GetType() == typeof(ComboBox))
                {
                    ComboBox prodCb = (ComboBox)control;
                    ProductGroup addedProdGroup = ProductGroups.FirstOrDefault(pg => pg.GetName() == prodCb.Name.Substring(0, prodCb.Name.Length - 4));
                    addedProd = Products.FirstOrDefault(p => p.GetGroupIndex() == addedProdGroup.GetIndex() && p.GetIndex() == prodCb.SelectedIndex);
                    prodCb.SelectedIndex = -1;
                }

                if (count > 0)
                {
                    GroceryList[x, 0] = addedProd;
                    GroceryList[x, 1] = count;
                    count = 0;
                    x++;
                }
            }
            LoadList(GroceryList);
        }

        private void RemoveProduct (object sender, EventArgs e)
        {
        }

        private void LoadList(object[,] GL)
        {
            for (int i = 0; i < GL.GetLength(0); i++)
            {
                if (GL[i, 0] == null)
                {
                    break;
                }

                Product prod = (Product)GL[i, 0];
                string itemText = prod.GetName() + " x " + GL[i, 1].ToString() + " á " + prod.GetPrice() + " - " + (prod.GetPrice() * Int32.Parse(GL[i, 1].ToString())).ToString() + "$" ;

                addedListBox.Items.Add(itemText);
            }
        } 
    }
}
