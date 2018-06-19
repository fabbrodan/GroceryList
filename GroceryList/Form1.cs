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
        }

        // array for globally handling prod groups
        private ProductGroup[] ProductGroups;

        // array for globally handling available products
        private Product[] Products;

        // 2D array for handling product and count of how many thereof
        private object[,] GroceryList = new object[24,2];

        // Initial method to load product groups and products
        private void LoadProducts()
        {
            // Setting applications Culture info to always use comma as decimal separator
            CultureInfo CI = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            CI.NumberFormat.NumberDecimalSeparator = ",";
            Thread.CurrentThread.CurrentCulture = CI;

            using(DataSet ds = new DataSet())
            {
                // Read in Productgroups from XML file
                Stream stream = File.OpenRead(Environment.CurrentDirectory + @"\Resources\ProductGroups.xml");
                ds.ReadXml(stream, XmlReadMode.InferSchema);
                stream.Close();
                DataTable dt = ds.Tables[0];

                // assign global variable
                ProductGroups = new ProductGroup[dt.Rows.Count];

                // Loop through read data and populate array with new ProductGroup objects
                int i = 0;
                foreach (DataRow row in dt.Rows)
                {
                    ProductGroups[i] = new ProductGroup(Int32.Parse(row.ItemArray[0].ToString()), row.ItemArray[1].ToString());
                    i++;
                }
            }

            using (DataSet ds = new DataSet())
            {
                // Read in Products from XML File
                Stream stream = File.OpenRead(Environment.CurrentDirectory + @"\Resources\Products.xml");
                ds.ReadXml(stream, XmlReadMode.InferSchema);
                stream.Close();
                DataTable dt = ds.Tables[0];

                // assign global variable
                Products = new Product[dt.Rows.Count];

                // Loop through read data and populate array with Product objects
                int i = 0;
                foreach (DataRow row in dt.Rows)
                {
                    Products[i] = new Product(Int32.Parse(row.ItemArray[0].ToString()), Int32.Parse(row.ItemArray[1].ToString()), row.ItemArray[2].ToString(), Double.Parse(row.ItemArray[3].ToString()));
                    i++;
                }
            }

            int x = selectablePanel.DisplayRectangle.Left + 5;
            int y = selectablePanel.DisplayRectangle.Top + 10;

            // Create UI Elements for Selectable Products/Groups

            // Loop through product groups
            int j = 0;
            foreach (ProductGroup group in ProductGroups)
            {
                if (j > 0)
                {
                    y += 25;
                }
                // For each group create label
                Label groupLabel = new Label
                {
                    Name = group.GetName() + "Label",
                    Text = group.GetName(),
                    Location = new Point(x, y),
                    Height = 20
                };

                // Array to store Product objects pertaining to each group
                Product[] groupProds = Products.Where(o => o.GetGroupIndex() == group.GetIndex()).ToArray<Product>();

                y += 20;

                // Create Dropdown for each group
                ComboBox groupDdwn = new ComboBox
                {
                    Name = group.GetName() + "Ddwn",
                    Location = new Point(x, y),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                // Loop through each Product in each group and add to dropdown
                foreach (Product product in groupProds)
                {
                    groupDdwn.Items.Add(product.GetName() + " " + product.GetPrice());
                }

                // Create one text box for each group to keep count of number of products user wishes to add
                TextBox tbCount = new TextBox
                {
                    Name = group.GetName() + "Count",
                    Width = 25,
                    Height = 20,
                    Location = new Point(groupDdwn.DisplayRectangle.Right + 25, y),
                };

                // Add controls to panel
                selectablePanel.Controls.Add(groupLabel);
                selectablePanel.Controls.Add(groupDdwn);
                selectablePanel.Controls.Add(tbCount);

                // Add eventhandlers to controls
                tbCount.KeyPress += new KeyPressEventHandler(DisallowText);
                groupDdwn.SelectedIndexChanged += new EventHandler(OnDdwnSelect);
                tbCount.Invalidated += new InvalidateEventHandler(OnCountUpdate);
                tbCount.Leave += new EventHandler(OnCountUpdate);

                y += 5;
                j++;
            }

            // Header label for count textbox
            Label countLabel = new Label
            {
                Text = "Count",
                Location = new Point(140, selectablePanel.DisplayRectangle.Top + 10)
            };

            selectablePanel.Controls.Add(countLabel);
        }

        // Event Handler to only allow numeric input into Count tetboxes
        private void DisallowText (object sender, KeyPressEventArgs e)
        {
            e.Handled = true;

            if (Regex.IsMatch(e.KeyChar.ToString(), "[0-9]") || char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
        }

        // Event handler to reset count to 1 on change of product
        private void OnDdwnSelect (object sender, EventArgs e)
        {
            // Convert sender to combobox
            ComboBox obj = (ComboBox)sender;
            
            // Create object for parent to combobox (Panel)
            Panel parent = (Panel)obj.Parent;

            // Find the corresponding count textbox based on name of dropdown used
            TextBox tb = (TextBox)parent.Controls.Find(obj.Name.Substring(0, obj.Name.Length - 4) + "Count", true)[0];

            if (obj.SelectedIndex > -1)
            {
                tb.Text = "1";
            }

            tb.Invalidate();
        }

        // Event handler to change the text for the price of selected item * count when count is altered
        private void OnCountUpdate(object sender, EventArgs e)
        {
            // Convert the sender to TextBox
            TextBox countTb = (TextBox)sender;

            int Count = 0;
            try
            {
                Count = Int32.Parse(countTb.Text);
            }
            catch (FormatException)
            {
            }

            // Math dropdown with count textbox
            string objName = countTb.Name.Substring(0, countTb.Name.Length - 5);
            ComboBox cb = (ComboBox)selectablePanel.Controls.Find(objName + "Ddwn", true)[0];

            // Find group index of selected dropdown used
            int GroupIndex = ProductGroups.FirstOrDefault(o => o.GetName() == objName).GetIndex();

            // Instantiate Product object based on selected group and dropdown index to get price
            Product selectedProd = Products.FirstOrDefault(a => a.GetGroupIndex() == GroupIndex && a.GetIndex() == cb.SelectedIndex);

            // Check if label exists to avoid overwriting and creating multiple identical objects
            if (selectablePanel.Controls.ContainsKey(objName + "PriceLabel"))
            {
                // If label already exists set temporary object to control it
                Label priceLabel = (Label)selectablePanel.Controls.Find(objName + "PriceLabel", true)[0];
                // If the textbox text is 0 or user tries to add count before selecting product set text to nothing
                if (Count == 0 || selectedProd == null)
                {
                    priceLabel.Text = "";
                }
                // Otherwise set the text to be the calculated value of Products price times entered count
                else
                {
                    priceLabel.Text = (selectedProd.GetPrice() * Int32.Parse(countTb.Text)).ToString() + "$";
                }
            }
            // If label does not exist create new object and add it to the parent controls collection
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

        // Event Handler for adding products
        private void AddProduct (object sender, EventArgs e)
        {
            // int to control index in 2D array
            int x = 0;

            // int to represent count of products entered by user
            int count = 0;
            
            // Empty Product object to be used for adding
            Product addedProd = null;

            // Loop through the controls of left hand panel
            foreach (Control control in selectablePanel.Controls)
            {
                // if control is text box store entered number in count variable and leave 0 if nothing is entered
                // reset the text to empty
                if (control.GetType() == typeof(TextBox))
                {
                    TextBox countTb = (TextBox)control;
                    Int32.TryParse(countTb.Text, out count);
                    countTb.Text = "";
                }

                // if control is dropdown
                if (control.GetType() == typeof(ComboBox))
                {
                    ComboBox prodCb = (ComboBox)control;

                    // ProductGroup object corresponding to selected dropdown group
                    ProductGroup addedProdGroup = ProductGroups.FirstOrDefault(pg => pg.GetName() == prodCb.Name.Substring(0, prodCb.Name.Length - 4));

                    // Set empty Product object to selected object based on group and selected item in dropdown
                    addedProd = Products.FirstOrDefault(p => p.GetGroupIndex() == addedProdGroup.GetIndex() && p.GetIndex() == prodCb.SelectedIndex);

                    // reset dropdown select to empty
                    prodCb.SelectedIndex = -1;
                }

                // If the count entered is larger than 0
                if (count > 0)
                {
                    // set the corresponding positions in 2D array to correspond to selected product and count thereof
                    GroceryList[x, 0] = addedProd;
                    GroceryList[x, 1] = count;

                    // Reset Count variable
                    count = 0;

                    // Update index variable
                    x++;
                }
            }

            // Call method to present added products
            LoadList(GroceryList);
        }

        // Event Handler for removing products
        /* ----- FIX THIS FIX THIS FIX THIS FIX THIS FIX THIS FIX THIS ------ */
        private void RemoveProduct (object sender, EventArgs e)
        {
            Product RemovedProduct = null;

            while (addedListBox.SelectedItems.Count > 0)
            {
                Remove();
            }

            void Remove()
            {
                List<string> Removed = new List<string>();
                foreach (string SelectedItem in addedListBox.SelectedItems)
                {
                    string ProdString = SelectedItem.Substring(0, SelectedItem.IndexOf('x') - 1);

                    for (int i = 0; i < GroceryList.GetLength(0); i++)
                    {
                        if (GroceryList[i,0] != null)
                        {
                            RemovedProduct = (Product)GroceryList[i, 0];
                            if (RemovedProduct.GetName() == ProdString)
                            {
                                GroceryList[i, 0] = null;
                                GroceryList[i, 1] = null;
                                Removed.Add(SelectedItem);
                                break;
                            }
                        }
                    }
                }
                foreach(string rem in Removed)
                {
                    addedListBox.Items.Remove(rem);
                }
            }
        }

        // Method to display added products in a selectable list
        private void LoadList(object[,] GL)
        {
            // Loop through 1st dimension of 2D array
            for (int i = 0; i < GL.GetLength(0); i++)
            {
                // when we hit empty space break loop
                if (GL[i, 0] == null)
                {
                    break;
                }

                // Product to generate text added to ListBox view
                Product prod = (Product)GL[i, 0];

                // Display prod name + count + cost for single item + total cost of price * count
                string itemText = prod.GetName() + " x " + GL[i, 1].ToString() + " á " + prod.GetPrice() + " - " + (prod.GetPrice() * Int32.Parse(GL[i, 1].ToString())).ToString() + "$" ;

                // add string to list box item collection
                addedListBox.Items.Add(itemText);
            }
        } 
    }
}
