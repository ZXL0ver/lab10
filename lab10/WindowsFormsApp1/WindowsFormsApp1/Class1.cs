using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    // Subject interface
    interface IEBook
    {
        string GetPage(int pageNumber);
        int GetTotalPages();
    }

    // Real subject class
    class EBook : IEBook
    {
        private List<string> pages;

        public EBook(List<string> pages)
        {
            this.pages = pages;
        }

        public string GetPage(int pageNumber)
        {
            if (pageNumber < 1 || pageNumber > pages.Count)
            {
                throw new ArgumentException("Invalid page number.");
            }

            return pages[pageNumber - 1];
        }

        public int GetTotalPages()
        {
            return pages.Count;
        }
    }

    // Proxy class
    class EBookProxy : IEBook
    {
        private EBook ebook;
        private Dictionary<string, List<int>> index;

        public EBookProxy(List<string> pages)
        {
            ebook = new EBook(pages);
            index = new Dictionary<string, List<int>>();

            // Build index
            for (int i = 0; i < pages.Count; i++)
            {
                string[] words = pages[i].Split(' ');
                foreach (string word in words)
                {
                    if (!index.ContainsKey(word))
                    {
                        index[word] = new List<int>();
                    }

                    index[word].Add(i + 1);
                }
            }
        }

        public string GetPage(int pageNumber)
        {
            return ebook.GetPage(pageNumber);
        }

        public int GetTotalPages()
        {
            return ebook.GetTotalPages();
        }

        public List<int> Search(string query)
        {
            if (index.ContainsKey(query))
            {
                return index[query];
            }
            else
            {
                return new List<int>();
            }
        }
    }
    class EBookForm : Form
    {
        private IEBook ebook;
        private TextBox pageTextBox;
        private NumericUpDown pageNumberNumericUpDown;
        private Button previousPageButton;
        private Button nextPageButton;
        private Button searchButton;
        private TextBox searchTextBox;
        private ListBox searchResultsListBox;

        public EBookForm(IEBook ebook)
        {
            this.ebook = ebook;

            // Initialize form elements
            this.Text = "E-Book";
            this.ClientSize = new Size(400, 400);

            pageTextBox = new TextBox();
            pageTextBox.Multiline = true;
            pageTextBox.ReadOnly = true;
            pageTextBox.ScrollBars = ScrollBars.Vertical;
            pageTextBox.Dock = DockStyle.Fill;
            this.Controls.Add(pageTextBox);

            Label pageNumberLabel = new Label();
            pageNumberLabel.Text = "Page:";
            pageNumberLabel.Dock = DockStyle.Left;
            pageNumberLabel.AutoSize = true;
            this.Controls.Add(pageNumberLabel);

            pageNumberNumericUpDown = new NumericUpDown();
            pageNumberNumericUpDown.Minimum = 1;
            pageNumberNumericUpDown.Maximum = ebook.GetTotalPages();
            pageNumberNumericUpDown.Dock = DockStyle.Left;
            pageNumberNumericUpDown.Width = 50;
            pageNumberNumericUpDown.ValueChanged += new EventHandler(PageNumberNumericUpDown_ValueChanged);
            this.Controls.Add(pageNumberNumericUpDown);

            previousPageButton = new Button();
            previousPageButton.Text = "Previous";
            previousPageButton.Dock = DockStyle.Left;
            previousPageButton.Click += new EventHandler(PreviousPageButton_Click);
            this.Controls.Add(previousPageButton);

            nextPageButton = new Button();
            nextPageButton.Text = "Next";
            nextPageButton.Dock = DockStyle.Left;
            nextPageButton.Click += new EventHandler(NextPageButton_Click);
            this.Controls.Add(nextPageButton);

            searchButton = new Button();
            searchButton.Text = "Search";
            searchButton.Dock = DockStyle.Right;
            this.Controls.Add(searchButton);

            searchTextBox = new TextBox();
            searchTextBox.Dock = DockStyle.Right;
            searchTextBox.Width = 100;
            this.Controls.Add(searchTextBox);

            searchResultsListBox = new ListBox();
            searchResultsListBox.Dock = DockStyle.Right;
            searchResultsListBox.Width = 100;
            searchResultsListBox.Height = 100;
            searchResultsListBox.Visible = false;
            searchResultsListBox.SelectedIndexChanged += new EventHandler(SearchResultsListBox_SelectedIndexChanged);
            this.Controls.Add(searchResultsListBox);
        }

        private void PageNumberNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            int pageNumber = (int)pageNumberNumericUpDown.Value;
            pageTextBox.Text = ebook.GetPage(pageNumber);
        }

        private void PreviousPageButton_Click(object sender, EventArgs e)
        {
            if (pageNumberNumericUpDown.Value > 1)
            {
                pageNumberNumericUpDown.Value--;
            }
        }

        private void NextPageButton_Click(object sender, EventArgs e)
        {
            if (pageNumberNumericUpDown.Value < ebook.GetTotalPages())
            {
                pageNumberNumericUpDown.Value++;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            string query = searchTextBox.Text.Trim().ToLower();
            if (query.Length > 0)
            {
                List<int> results = ((EBookProxy)ebook).Search(query);
                if (results.Count > 0)
                {
                    searchResultsListBox.Items.Clear();
                    searchResultsListBox.Items.AddRange(results.Select(r => $"Page {r}").ToArray());
                    searchResultsListBox.Visible = true;
                }
                else
                {
                    MessageBox.Show($"No results found for '{query}'.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void SearchResultsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int pageNumber = Int32.Parse(searchResultsListBox.SelectedItem.ToString().Substring(5));
            pageNumberNumericUpDown.Value = pageNumber;
            searchResultsListBox.Visible = false;
        }
    }
}
