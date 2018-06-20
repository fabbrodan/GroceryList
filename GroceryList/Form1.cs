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

            // Call initial method to load data
            LoadProducts();
        }

        // array for globally handling prod groups
        /* ----- CLASS OBJECT ----- */
        private ProductGroup[] ProductGroups;

        // array for globally handling available products
        /* ----- CLASS OBJECT ----- */
        private Product[] Products;

        // 2D array for handling product and count of how many thereof
        /* ----- CLASS OBJECT ----- */
        private object[,] GroceryList = new object[100,2];

        // Initial method to load product groups and products
        /* ----- METHOD ----- */
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

        // Event Handler method to only allow numeric input into Count tetboxes
        /* ----- METHOD ----- */
        private void DisallowText (object sender, KeyPressEventArgs e)
        {
            e.Handled = true;

            if (Regex.IsMatch(e.KeyChar.ToString(), "[0-9]") || char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
        }

        // Event handler method to reset count to 1 on change of product
        /* ----- METHOD ----- */
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

        // Event handler method to change the text for the price of selected item * count when count is altered
        /* ----- METHOD ----- */
        private void OnCountUpdate(object sender, EventArgs e)
        {
            // Convert the sender to TextBox
            TextBox countTb = (TextBox)sender;

            int Count = 0;
            Int32.TryParse(countTb.Text, out Count);

            // Math dropdown with count textbox
            string objName = countTb.Name.Substring(0, countTb.Name.Length - 5);
            ComboBox cb = (ComboBox)selectablePanel.Controls.Find(objName + "Ddwn", true)[0];

            if (countTb.TextLength > 3)
            {
                MessageBox.Show("You can only add up to 999 items of the same type!");
                countTb.Text = "";
                cb.SelectedIndex = -1;
                if (selectablePanel.Controls.ContainsKey(objName + "PriceLabel"))
                {
                    Label priceLabel = (Label)selectablePanel.Controls.Find(objName + "PriceLabel", true)[0];
                    priceLabel.Text = "";
                }
                return;
            }

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
                if (selectedProd == null)
                {
                    return;
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
        }

        // Event Handler method for adding products
        /* ----- METHOD ----- */
        private void AddProduct (object sender, EventArgs e)
        {
            // int to control index in 2D array
            int x = 0;

            // Loop through 2D array to find the next empty index
            for (int i = 0; i < GroceryList.GetLength(0); i++)
            {
                if (GroceryList[i,0] != null)
                {
                    x++;
                }
                else { break; }
            }

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

        // Event Handler method for removing products
        /* ----- METHOD ----- */
        private void RemoveProduct (object sender, EventArgs e)
        {
            // List to contain selected objects to be removed from listbox
            List<string> removeList = new List<string>();

            // Loop through the selected items in listbox
            foreach (string selected in addedListBox.SelectedItems)
            {
                // Get the actual productname
                string ProductName = selected.Substring(0, selected.IndexOf('x') - 1);
                
                // Loop through 2D array
                for (int i = 0; i < GroceryList.GetLength(0); i++)
                {
                    // int to check if the next element in array is empty
                    int j = i+1;

                    // Create product object to validate against selected item
                    Product product = (Product)GroceryList[i,0];

                    // if product exists
                    if (product != null)
                    {
                        // if names match
                        if (product.GetName() == ProductName)
                        {
                            // local int to manage the 2D array
                            int x = i;

                            // while the next element in array is not empty move the next element into current position
                            while (GroceryList[j - 1, 0] != null)
                            {
                                GroceryList[x, 0] = GroceryList[j, 0];
                                GroceryList[x, 1] = GroceryList[j, 1];
                                x++;
                                j++;
                            }
                        }                        
                    }
                    // if product object is empty break the loop because we've reached end of items
                    else
                    {
                        break;
                    }
                }

                // finally add the selected product to list of items to be removed
                removeList.Add(selected);
            }

            // loop through list and remove the item from listbox
            foreach (string remove in removeList)
            {
                addedListBox.Items.Remove(remove);
            }

            // Call to method to update total cost
            UpdateCost();
        }

        // Method to display added products in a selectable list
        /* ----- METHOD ----- */
        private void LoadList(object[,] GL)
        {
            // int variable to keep track of populated elements in passed array
            int x = 0;

            // loop through array
            for (int i = 0; i < GL.GetLength(0); i++)
            {
                // if x position is not empty update x counter
                if (GL[i,0] != null)
                {
                    x++;
                }
                else
                {
                    break;
                }
            }

            // if x counter is not equal to items in list box
            if (x != addedListBox.Items.Count)
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
                    string itemText = prod.GetName() + " x " + GL[i, 1].ToString() + " á " + prod.GetPrice() + " - " + (prod.GetPrice() * Int32.Parse(GroceryList[i, 1].ToString())) + "$";

                    // add string to list box item collection if it does not exist
                    if (!addedListBox.Items.Contains(itemText))
                    {
                        addedListBox.Items.Add(itemText);
                    }
                }

                // Call method to calculate new total cost
                UpdateCost();
            }
        }

        // Method for updating total cost
        /* ----- METHOD ----- */
        private void UpdateCost()
        {
            // variable for the total cost
            double totalCostForAll = 0.00;

            // variable for the cost of one item times quantity
            double totalCostForItem = 0.00;

            // array to store each individual items total cost
            double[] totalCostArr = new double[addedListBox.Items.Count];

            // loop through 2D array
            for (int i = 0; i < GroceryList.GetLength(0); i++)
            {
                // if the 2D array hits empty object then break out of loop
                if (GroceryList[i,0] != null)
                {
                    // set product object
                    Product product = (Product)GroceryList[i, 0];

                    // calculate products total cost
                    totalCostForItem = (product.GetPrice() * Int32.Parse(GroceryList[i, 1].ToString()));

                    // add total cost of product to array
                    totalCostArr[i] = totalCostForItem;
                }
                else
                {
                    break;
                }
            }

            // Loop through array and sum price of each products total cost
            for (int y = 0; y < totalCostArr.Length; y++)
            {
                totalCostForAll += totalCostArr[y];
            }

            // set the label according to calculated total cost
            totalCostLabel.Text = "Total: " + Math.Round(totalCostForAll, 2) + "$";
        }
    }
}
