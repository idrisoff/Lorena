using Microsoft.Data.Sqlite;
using System.Data;

namespace SalonLorena
{
    public partial class Form1 : Form
    {
        string path = "data.db";
        string pathRes = "dataRes.db";

        public Form1()
        {
            InitializeComponent();
            button3.Click += (o, e) => { CreateNode(treeView1, textBox3.Text, "A"); };
            button5.Click += (o, e) => { CreateNode(treeView1, textBox3.Text, "D"); };
            button1.Click += (o, e) => { Calculate(treeView1); };
            button2.Click += (o, e) => { SaveResult(dataGridView1, treeView1); };
        }

        void MyFormLoad(object sender, EventArgs e)
        {
            CreateDB();
            ShowData();
            CreateOrReadDBResult(dataGridView1);
        }

        void CreateDB()
        {
            using (var con = new SqliteConnection(@"Data Source=" + path))
            {
                con.Open();
                string sql1 = "create table if not exists shops(" +
                    "id integer not null primary key autoincrement unique, " +
                    "nameSalon text not null unique, " +
                    "discount real default 0, " +
                    "description text default '', " +
                    "depend integer default 0, " +
                    "parentId integer)";
                SqliteCommand command = new SqliteCommand(sql1, con);
                command.ExecuteNonQuery();
                string stm = "SELECT * FROM shops";
                var cmd = new SqliteCommand(stm, con);
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    string sql2 = @"insert into shops(nameSalon, discount, description, depend, parentId) values "
                    + "('Миасс', 4, '', 0, 0), "
                    + "('Амелия', 5, '', 1, 1), "
                    + "('Тест1', 2, '', 1, 2), "
                    + "('Тест2', 0, '', 1, 1), "
                    + "('Курган', 11, '', 0, 0)";
                    command = new SqliteCommand(sql2, con);
                    command.ExecuteNonQuery();
                }
            }
        }

        void ShowData()
        {
            treeView1.Nodes.Clear();
            using (var con = new SqliteConnection(@"Data Source=" + path))
            {
                con.Open();
                string stm = "SELECT * FROM shops where parentId = 0";
                string stmChild = "SELECT * FROM shops where parentId > 0";
                var cmd = new SqliteCommand(stm, con);
                var cmdChild = new SqliteCommand(stmChild, con);
                var readerChild = cmdChild.ExecuteReader();
                var reader = cmd.ExecuteReader();
                treeView1.Nodes.Add("Салоны");
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        treeView1.Nodes[0].Nodes.Add(reader.GetString(1)).Tag = reader.GetString(0);
                    }
                }
                while (readerChild.Read())
                {
                    if (readerChild.HasRows)
                    {
                        var node = new TreeNode(readerChild.GetString(1));
                        node.Tag = readerChild.GetString(0);
                        FindByTag(treeView1.Nodes[0].Nodes, node, Convert.ToInt32(readerChild.GetString(5)));
                    }
                }
                treeView1.ExpandAll();
            }
        }
        private void FindByTag(TreeNodeCollection Nodes, TreeNode node, int tagValue)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                //Не забываем, что в этом примере в Tag мы записывали числовой первичный ключ!
                if (Convert.ToInt32(Nodes[i].Tag) == tagValue)
                {
                    //Раскрываем нужный пункт 
                    Nodes[i].Nodes.Add(node);
                }
                else
                {
                    //Запускаем рекурсию
                    FindByTag(Nodes[i].Nodes, node, tagValue);
                }
            }
        }
        void CreateNode(TreeView view, string text, string type)
        {
            TreeNode node = new TreeNode(text);
            if (type == "A")
            {
                try
                {
                    using (var con = new SqliteConnection(@"Data Source=" + path))
                    {
                        con.Open();
                        var x = view.SelectedNode.Text;
                        string sqlParent = $"select * from shops where nameSalon='{x}'";
                        var cmdParent = new SqliteCommand(sqlParent, con);
                        var readerParent = cmdParent.ExecuteReader();
                        string stm = "";
                        if (readerParent.HasRows)
                        {
                            readerParent.Read();
                            stm = $"insert into shops(nameSalon, discount, description, depend, parentId) values ('{textBox3.Text}', {Convert.ToDouble(textBox4.Text)}, '', 1, " +
                                $"{Convert.ToInt32(readerParent.GetString(0))})";
                        }
                        else
                            stm = $"insert into shops(nameSalon, discount, description, depend, parentId) values ('{textBox3.Text}', {Convert.ToDouble(textBox4.Text)}, '', 0, 0)";

                        var cmd = new SqliteCommand(stm, con);
                        cmd.ExecuteNonQuery();
                    }
                    view.SelectedNode.Nodes.Add(node);
                }
                catch (Exception)
                { MessageBox.Show("Error"); }
            }
            else
            {
                try
                {
                    using (var con = new SqliteConnection(@"Data Source=" + path))
                    {
                        con.Open();
                        var x = view.SelectedNode.Text;
                        string sql = $"select * from shops where nameSalon='{x}'";
                        var cmd = new SqliteCommand(sql, con);
                        var reader = cmd.ExecuteReader();
                        string stm = "";
                        if (reader.HasRows)
                        {
                            reader.Read();
                            stm = $"delete from shops where id = {Convert.ToInt32(reader.GetString(0))}";
                        }
                        var cmdElse = new SqliteCommand(stm, con);
                        cmdElse.ExecuteNonQuery();
                    }
                    view.SelectedNode.Remove();
                }
                catch (Exception)
                { MessageBox.Show("Error"); }
            }
        }
        void Calculate(TreeView view)
        {
            try
            {
                using (var con = new SqliteConnection(@"Data Source=" + path))
                {
                    con.Open();
                    var price = Double.Parse(textBox1.Text);
                    var x = view.SelectedNode.Text;
                    string sql = $"select * from shops where nameSalon='{x}'";
                    var cmd = new SqliteCommand(sql, con);
                    var reader = cmd.ExecuteReader();
                    double discount = 0;
                    int parentId = 0;
                    if (reader.HasRows)
                    {
                        reader.Read();
                        parentId = reader.GetInt32(5);
                        if (parentId == 0)
                            discount = reader.GetInt32(2);
                        else
                            discount = FindDiscountParent(parentId, reader.GetInt32(2));
                    }
                    textBox2.Text = (price - (price * (discount / 100))).ToString();
                }
            }
            catch (Exception)
            { MessageBox.Show("Error"); }
        }
        double FindDiscountParent(int id, double s)
        {
            using (var con = new SqliteConnection(@"Data Source=" + path))
            {
                con.Open();
                string sql = $"select * from shops where id={id}";
                var cmd = new SqliteCommand(sql, con);
                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    s += reader.GetDouble(2);
                    if (reader.GetInt32(4) == 0)
                        return s;
                    return FindDiscountParent(reader.GetInt32(5), s);
                }
                else return 0;
            }
        }
        void SaveResult(DataGridView dgv, TreeView view)
        {
            try
            {
                using (var con = new SqliteConnection(@"Data Source=" + pathRes))
                {
                    con.Open();
                    var price = Double.Parse(textBox1.Text);
                    var res = Double.Parse(textBox2.Text);
                    var x = view.SelectedNode.Text;
                    double discount = GetDiscount(x);
                    dgv.Rows.Add(x, price, discount, res);
                    string stm = $"insert into res(nameSalon, price, discount, res) " +
                        $"values ('{x}', " +
                        $"'{price}', '{discount}', '{res}')";
                    var cmd = new SqliteCommand(stm, con);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            { MessageBox.Show("Error"); }
        }
        void CreateOrReadDBResult(DataGridView dgv)
        {
            using (var con = new SqliteConnection(@"Data Source=" + pathRes))
            {
                con.Open();
                string sql1 = "create table if not exists res(" +
                    "id integer not null primary key autoincrement unique, " +
                    "nameSalon text not null, " +
                    "price real default 0, " +
                    "discount real default 0, " +
                    "res real)";
                SqliteCommand command = new SqliteCommand(sql1, con);
                command.ExecuteNonQuery();
                string stm = "SELECT * FROM res";
                var cmd = new SqliteCommand(stm, con);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        dgv.Rows.Add(reader.GetString(1), reader.GetString(2), GetDiscount(reader.GetString(1)), reader.GetString(4));
                    }
                }
            }
        }
        double GetDiscount(string nameSalon)
        {
            double discount = 0;
            using (var con = new SqliteConnection(@"Data Source=" + path))
            {
                con.Open();
                string sql = $"select * from shops where nameSalon='{nameSalon}'";
                var cmd = new SqliteCommand(sql, con);
                var reader = cmd.ExecuteReader();
                int parentId = 0;
                if (reader.HasRows)
                {
                    reader.Read();
                    parentId = reader.GetInt32(5);
                    if (parentId == 0)
                        discount = reader.GetInt32(2);
                    else
                        discount = FindDiscountParent(parentId, reader.GetInt32(2));
                }
            }
            return discount;
        }
    }
}