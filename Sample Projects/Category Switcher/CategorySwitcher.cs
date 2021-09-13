using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using Microsoft.Win32;

namespace Category_Switcher
{
    public partial class CategorySwitcher : Form
    {
        public CategorySwitcher()
        {
            InitializeComponent();
            LoadInitialConfiguration();
        }

        private void LoadInitialConfiguration()
        {
            treeView1.Nodes.Clear();

            string xmlPath = GetOrchestratorXmlFilePath();
            string searchPattern = "*.xml*";
            string[] files = Directory.GetFiles(xmlPath, searchPattern);

            //get a list of all the XML files in the Extensions directory
            foreach (string filename in files)
            {
                //read all of the XML files and fit them into a master XML schema by getting the appropriate nodes
                XmlDocument ipXml = new XmlDocument();

                ipXml.Load(filename);
                XmlNodeList categoriesNodes = ipXml.SelectNodes("//Categories");

                foreach (XmlNode categoriesNode in categoriesNodes)
                {

                    XmlNodeList categoryNodes = categoriesNode.SelectNodes(".//Category/Name");

                    foreach (XmlNode categoryNameNode in categoryNodes)
                    {

                        string categoryName = categoryNameNode.InnerText;
                        if (categoryName.StartsWith("str_"))
                        {
                            string dllStringFile = GetDLLStringResourceFileName(filename);
                            string xmlStringFile = GetXMLStringResourceFileName(filename);

                            if (File.Exists(xmlStringFile))
                            {
                                categoryName = GetXmlResourceString(xmlStringFile, categoryName);
                            }
                            if (File.Exists(dllStringFile))
                            {
                                categoryName = GetDLLResourceString(dllStringFile, categoryName);
                            }
                        }

                        TreeNode categoryTreeNode = new TreeNode(categoryName);
                        categoryTreeNode.Tag = filename;
                        if (filename.EndsWith(".bak"))
                        {
                            categoryTreeNode.Checked = false;
                        }
                        else
                        {
                            categoryTreeNode.Checked = true;
                        }

                        treeView1.Nodes.Add(categoryTreeNode);
                    }
                }

            }
        }

   
        private string GetOrchestratorXmlFilePath()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\SystemCenter2012\\Orchestrator\\Standard Activities");
            var path = rk.GetValue("InstallLocation", string.Empty);
            return path.ToString() + "Extensions";
        }

        private string GetOrchestratorStringsFilePath()
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\SystemCenter2012\\Orchestrator\\Standard Activities");
            var path = rk.GetValue("InstallLocation", string.Empty);
            return path.ToString() + "Strings";
        }

        private string GetDLLStringResourceFileName(string objectFileName)
        {
            string dllStringFile = string.Empty;

            dllStringFile = objectFileName.Replace("\\Extensions", "\\Strings");
            dllStringFile = dllStringFile.Replace(".xml.bak", ".xml");
            dllStringFile = dllStringFile.Replace(".xml", ".Strings.dll");

            return dllStringFile;
        }

        private string GetXMLStringResourceFileName(string objectFileName)
        {
            string xmlStringFile = string.Empty;

            xmlStringFile = objectFileName.Replace("\\Extensions", "\\Strings");
            xmlStringFile = xmlStringFile.Replace(".xml.bak", ".xml");
            xmlStringFile = xmlStringFile.Replace(".xml", ".Strings.xml");

            return xmlStringFile;
        }

        private string GetDLLResourceString(string filename, string resourceID)
        {
            
            string resourceString = string.Empty;

            if (resourceID.EndsWith("Category"))
            {
                resourceID = resourceID.Remove(resourceID.Length - 8);
            }
            if (resourceID.StartsWith("str_"))
            {
                resourceID = resourceID.Replace("str_", "");
            }
            resourceString = resourceID;

            return resourceString;

        }

        private string GetXmlResourceString(string filename, string resourceID)
        {
            string resourceString = string.Empty;

            XmlDocument xml = new XmlDocument();
            xml.Load(filename);
            XmlNode node = xml.SelectSingleNode("//" + resourceID);
            if (node != null)
            {
                resourceString = node.InnerText;
            }
            else
            {
                if (resourceID.EndsWith("Category"))
                {
                    resourceID = resourceID.Remove(resourceID.Length - 8);
                }
                if (resourceID.StartsWith("str_"))
                {
                    resourceID = resourceID.Replace("str_", "");
                }
                resourceString = resourceID;
            }
            return resourceString;
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
           
        }

        private void EnableCategoryFile(TreeNode node)
        {
            string fileName = node.Tag.ToString(); 
            string newName = fileName.Replace(".bak","");
            if (File.Exists(fileName))
            {
                if (fileName.EndsWith(".xml.bak"))
                {
                    File.Move(fileName, newName);
                    node.Tag = newName;
                }
            }
        }

        private void DisableCategoryFile(TreeNode node)
        {
            string fileName = node.Tag.ToString();
            string newName = fileName.Replace(".xml", ".xml.bak");
            if (File.Exists(fileName))
            {
                if (fileName.EndsWith(".xml"))
                {
                    File.Move(fileName, newName);
                    node.Tag = newName;

                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Checked)
                {
                    EnableCategoryFile(node);
                }
                else
                {
                    DisableCategoryFile(node);
                }
            }
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonEnableAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Checked == false)
                {
                    node.Checked = true;
                }
            }
        }

        private void buttonDisableAll_Click(object sender, EventArgs e)
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Checked)
                {
                    node.Checked = false;
                }
            }
        }
    }
}
