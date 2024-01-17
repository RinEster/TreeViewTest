using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace treeviewTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string ConnStr = "Data Source = DESKTOP-E6HKS9J\\SQLEXPRESS;Initial Catalog = \"CodeVerseLeesons\"; Integrated Security = True"; //строка одключения бд

        public MainWindow()
        {
            InitializeComponent();

            using (SqlConnection connection = new SqlConnection(ConnStr))
            {
                connection.Open();
                // Получение данных из таблицы ParentChildTree
                string query = "SELECT P.ParentID, P.ChildID, P.HaveChild, T.ID FROM ParentChildTree P LEFT JOIN TopicDirectory T ON P.ChildID = T.ID";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                var treeNodes = new Dictionary<int, TreeViewItem>();
                while (reader.Read())
                {
                    int parentID = reader.GetInt32(0);
                    int childID = reader.GetInt32(1);
                    bool haveChild = reader.GetBoolean(2);
                    int childParentID = reader.IsDBNull(3) ? -1 : reader.GetInt32(3); // Если запись в TopicDirectory отсутствует, используем -1

                    TreeViewItem parentNode;
                    if (!treeNodes.TryGetValue(parentID, out parentNode))
                    {
                        parentNode = new TreeViewItem { Header = $"Node {parentID}" };
                        treeNodes[parentID] = parentNode;
                    }

                    TreeViewItem childNode;

                    // Проверяем, является ли ChildID также ParentID
                    if (childParentID == parentID)
                    {
                        childNode = new TreeViewItem { Header = $"Node {childID}" };
                        if (haveChild)
                        {
                            var dummyNode = new TreeViewItem { Header = $"Node {childID}" };
                            childNode.Items.Add(dummyNode); // Добавляем заглушку для дочернего узла
                        }
                        parentNode.Items.Add(childNode); // Добавляем на второй уровень
                    }
                    else
                    {
                        childNode = new TreeViewItem { Header = $"Node {childID}" };
                        if (haveChild)
                        {
                            var dummyNode = new TreeViewItem { Header = "Loading..." };
                            childNode.Items.Add(dummyNode); // Добавляем заглушку для дочернего узла
                        }
                        parentNode.Items.Add(childNode); // Добавляем на первый уровень
                    }

                }

                // Привязка структуры дерева к элементу управления TreeView
                foreach (var node in treeNodes.Values)
                {
                    if (node.Parent == null)
                    {
                        treeView.Items.Add(node);
                    }
                }
                reader.Close();
                // Получение данных из таблицы TopicDirectory для соответствия с ParentID
                string topicQuery = "SELECT ID, Name FROM TopicDirectory";
                SqlCommand topicCommand = new SqlCommand(topicQuery, connection);
                using (SqlDataReader topicReader = topicCommand.ExecuteReader())
                {
                    var topicNames = new Dictionary<int, string>();
                    while (topicReader.Read())
                    {
                        int topicID = topicReader.GetInt32(0);
                        string topicName = topicReader.GetString(1);
                        topicNames[topicID] = topicName;
                    }

                    // Использование значений из TopicDirectory для замены названий узлов
                    foreach (var node in treeNodes.Values)
                    {
                        UpdateNodeHeaders(node, topicNames); // Вызов рекурсивной функции для обновления названий узлов
                    }
                }
            }
        }
        void UpdateNodeHeaders(TreeViewItem node, Dictionary<int, string> topicNames)
        {
            if (node.Header != null)
            {
                string[] headerParts = node.Header.ToString().Split(' ');
                if (headerParts.Length > 1)
                {
                    int id;
                    if (int.TryParse(headerParts[1], out id))
                    {
                        if (topicNames.ContainsKey(id))
                        {
                            node.Header = topicNames[id];
                        }
                    }
                }
            }
            foreach (TreeViewItem childNode in node.Items)
            {
                UpdateNodeHeaders(childNode, topicNames);
            }
        }
    }

}
    
