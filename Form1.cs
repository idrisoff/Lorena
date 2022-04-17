using Microsoft.Data.Sqlite;
using System.Data;

namespace SalonLorena
{
    public partial class Form1 : Form
    {
        string path = "data.db";

        public Form1()
        {
            InitializeComponent();
            button3.Click += (o, e) => { CreateNode(treeView1, textBox3.Text, "A"); };
            button4.Click += (o, e) => { CreateNode(treeView1, textBox3.Text, "E"); };
            button5.Click += (o, e) => { CreateNode(treeView1, textBox3.Text, "D"); };
        }
        void CreateDB()
        {
            using (var con = new SqliteConnection(@"Data Source=" + path))
            {
                con.Open();
                string sql1 = "create table if not exists shops(" +
                    "id integer not null primary key autoincrement unique, " +
                    "name varchar(20) not null unique, " +
                    "discount real default 0, " +
                    "description varchar(124) default '', " +
                    "depend integer default 0, " +
                    "parentId integer)";
                SqliteCommand command = new SqliteCommand(sql1, con);
                command.ExecuteNonQuery();
                string stm = "SELECT * FROM shops";
                var cmd = new SqliteCommand(stm, con);
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    string sql2 = @"insert into shops(name, discount, description, depend, parentId) values "
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
        void MyFormLoad(object sender, EventArgs e)
        {
            CreateDB();
            ShowData();
        }
        void CreateNode(TreeView view, string text, string type)
        {
            TreeNode node = new TreeNode(text);
            if (type == "A")
            {
                try
                {
                    view.SelectedNode.Nodes.Add(node);
                }
                catch (Exception)
                { view.Nodes.Add(node); }
            }
            else if (type == "E")
                view.SelectedNode.Text = text;
            else
                view.SelectedNode.Remove();
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
    }
}