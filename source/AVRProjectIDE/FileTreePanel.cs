﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace AVRProjectIDE
{
    public partial class FileTreePanel : DockContent
    {
        //[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            try
            {
                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                ErrorReportWindow.Show(ex, "Error In File Tree Panel");

            }
        }

        #region Fields and Properties

        private AVRProject project;
        private Dictionary<string, EditorPanel> editorList;

        private TreeNode updateNoticeNode;
        private TreeNode rootNode;
        private TreeNode sourceNode;
        private TreeNode headerNode;
        private TreeNode otherNode;

        private List<TreeNode> sourceNodeList = new List<TreeNode>();
        private List<TreeNode> headerNodeList = new List<TreeNode>();
        private List<TreeNode> otherNodeList = new List<TreeNode>();
        private Dictionary<string, List<TreeNode>> sourceFolderList = new Dictionary<string, List<TreeNode>>();
        private Dictionary<string, List<TreeNode>> headerFolderList = new Dictionary<string, List<TreeNode>>();
        private Dictionary<string, List<TreeNode>> otherFolderList = new Dictionary<string, List<TreeNode>>();

        #endregion

        public FileTreePanel()
        {
            InitializeComponent();

            InitializeTree();
        }

        #region Events and Delegates

        public event OpenFileEvent OpenNode;
        public delegate void OpenFileEvent(TreeNode node);

        #endregion

        #region Methods

        public void RemoveNode(TreeNode node)
        {
            string fileName = node.Text;

            ProjectFile f = null;
            if (project.FileList.TryGetValue(fileName.ToLowerInvariant(), out f))
            {
                project.FileList.Remove(fileName.ToLowerInvariant());
                if (f.BackupExists)
                {
                    try
                    {
                        File.Delete(f.BackupPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not delete backup of " + f.FileName + " due to error: " + ex.Message);
                    }
                }
            }

            EditorPanel editor;
            if (editorList.TryGetValue(fileName.ToLowerInvariant(), out editor))
            {
                editor.Close();
                editorList.Remove(fileName.ToLowerInvariant());
            }

            node.Remove();

            if (f != null)
            {
                if (f.Exists)
                {
                    if (MessageBox.Show("'" + f.FileName + "' has been removed from the project. Do you want to delete '" + f.FileName + "' permanently?", "Delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        try
                        {
                            File.Delete(f.FileAbsPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Could not delete '" + f.FileAbsPath + "', " + ex.Message);
                        }
                    }
                }
            }

            if (project.Save() == SaveResult.Failed)
            {
                MessageBox.Show("Error saving project");
            }
        }

        public bool RenameNode(TreeNode node, string newName)
        {
            newName = newName.Trim();

            if (newName.Contains(" "))
                return false;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (newName.Contains(c))
                    return false;
            }

            if (Path.GetExtension(newName) != Path.GetExtension(node.Text))
            {
                if ((newName.ToLowerInvariant().EndsWith(".c") || newName.ToLowerInvariant().EndsWith(".cpp") || newName.ToLowerInvariant().EndsWith(".asm") || newName.ToLowerInvariant().EndsWith(".s")) && (node.Text.ToLowerInvariant().EndsWith(".h") || node.Text.ToLowerInvariant().EndsWith(".hpp")))
                    return false;

                if ((newName.ToLowerInvariant().EndsWith(".h") || newName.ToLowerInvariant().EndsWith(".hpp")) && (node.Text.ToLowerInvariant().EndsWith(".c") || node.Text.ToLowerInvariant().EndsWith(".cpp") || node.Text.ToLowerInvariant().EndsWith(".s") || node.Text.ToLowerInvariant().EndsWith(".asm")))
                    return false;
            }

            ProjectFile f;
            if (project.FileList.TryGetValue(node.Text.ToLowerInvariant(), out f))
            {
                if (f.Exists == false)
                    return false;
            }
            else
                return false;

            if (project.FileList.TryGetValue(newName.ToLowerInvariant(), out f) == false)
            {
                if (project.FileList.TryGetValue(node.Text.ToLowerInvariant(), out f))
                {
                    string newPath = f.FileDir + Path.DirectorySeparatorChar + newName;
                    if (File.Exists(newPath) == false)
                    {
                        try
                        {
                            EditorPanel editor = null;
                            if (editorList.TryGetValue(node.Text.ToLowerInvariant(), out editor))
                            {
                                editor.WatchingForChange = false;
                            }
                            File.Move(f.FileAbsPath, newPath);

                            f.FileAbsPath = newPath;

                            if (editor != null)
                            {
                                editorList.Remove(node.Text.ToLowerInvariant());
                                editorList.Add(newName.ToLowerInvariant(), editor);
                                editor.File.FileAbsPath = newPath;
                                editor.Text = newName;
                                editor.TabText = newName;
                                editor.WatchingForChange = true;
                            }
                        }
                        catch { return false; }

                        node.ToolTipText = f.FileRelPathTo(project.DirPath);

                        project.FileList.Remove(node.Text.ToLowerInvariant());
                        project.FileList.Add(newName.ToLowerInvariant(), f);

                        if (project.Save() == SaveResult.Failed)
                        {
                            MessageBox.Show("Error saving project");
                        }

                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public void InitializeTree()
        {
            rootNode = new TreeNode("Project");
            sourceNode = new TreeNode("Source Files (c, cpp, cxx, S, pde)");
            headerNode = new TreeNode("Header Files (h, hpp)");
            otherNode = new TreeNode("Other Files");

            rootNode.ContextMenuStrip = treeRClickMenu;
            sourceNode.ContextMenuStrip = treeRClickMenu;
            headerNode.ContextMenuStrip = treeRClickMenu;
            otherNode.ContextMenuStrip = treeRClickMenu;

            rootNode.Checked = true;
            sourceNode.Checked = true;
            headerNode.Checked = false;
            otherNode.Checked = false;

            rootNode.ToolTipText = "Double Click Me To Open Project Folder";
            sourceNode.ToolTipText = "Only Source Code Files";
            headerNode.ToolTipText = "Only Header Files";
            otherNode.ToolTipText = "Other Files";

            rootNode.Nodes.Add(sourceNode);
            rootNode.Nodes.Add(headerNode);
            rootNode.Nodes.Add(otherNode);

            rootNode.ImageKey = "folder2.png";
            rootNode.SelectedImageKey = "folder2.png";
            rootNode.StateImageKey = "folder2.png";

            sourceNode.ImageKey = "folder.png";
            sourceNode.SelectedImageKey = "folder.png";
            sourceNode.StateImageKey = "folder.png";

            headerNode.ImageKey = "folder.png";
            headerNode.SelectedImageKey = "folder.png";
            headerNode.StateImageKey = "folder.png";

            otherNode.ImageKey = "folder.png";
            otherNode.SelectedImageKey = "folder.png";
            otherNode.StateImageKey = "folder.png";

            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(rootNode);

            if (AboutBox.AssemblyDate.AddMonths(1) <= DateTime.Now && true == false)
            {
                updateNoticeNode = new TreeNode("IMPORTANT NOTICE");
                updateNoticeNode.Nodes.Add("The build date of this version");
                updateNoticeNode.Nodes.Add("of AVR Project IDE is");
                updateNoticeNode.Nodes.Add(AboutBox.AssemblyDate.ToString("MMMM d yyyy"));
                updateNoticeNode.Nodes.Add("It is over a month old");
                updateNoticeNode.Nodes.Add("Frank usually updates once");
                updateNoticeNode.Nodes.Add("per month even if he has no");
                updateNoticeNode.Nodes.Add("real changes to make.");
                updateNoticeNode.Nodes.Add("This could mean that the");
                updateNoticeNode.Nodes.Add("automatic update checking mechanism");
                updateNoticeNode.Nodes.Add("has malfunctioned. Whatever the");
                updateNoticeNode.Nodes.Add("reason may be, you should");
                updateNoticeNode.Nodes.Add("check for an update manually");
                updateNoticeNode.Nodes.Add("from the website.");
                treeView1.Nodes.Add(updateNoticeNode);
            }
        }

        public void PopulateList(AVRProject newProj, Dictionary<string, EditorPanel> newList)
        {
            project = newProj;
            editorList = newList;
            PopulateList();
        }

        private void PopulateList()
        {
            rootNode.Text = project.FileName;

            //treeView1.SuspendLayout();

            sourceFolderList.Clear();
            headerFolderList.Clear();
            otherFolderList.Clear();

            sourceNodeList.Clear();
            headerNodeList.Clear();
            otherNodeList.Clear();

            sourceNode.Nodes.Clear();
            headerNode.Nodes.Clear();
            otherNode.Nodes.Clear();

            foreach (ProjectFile file in project.FileList.Values)
            {
                if (SettingsManagement.AutocompleteEnable)
                    KeywordScanner.FeedFileContent(file);

                TreeNode tn = file.Node;

                tn.ToolTipText = file.FileRelProjPath;

                // attach the menu
                tn.ContextMenuStrip = nodeRClickMenu;

                if (file.Exists == false)
                {
                    tn.ImageKey = "missing.ico";
                    tn.SelectedImageKey = "missing.ico";
                    tn.StateImageKey = "missing.ico";
                }
                else
                {
                    if (file.IsOpen)
                    {
                        tn.ImageKey = "file.ico";
                        tn.SelectedImageKey = "file.ico";
                        tn.StateImageKey = "file.ico";
                    }
                    else
                    {
                        tn.ImageKey = "file2.ico";
                        tn.SelectedImageKey = "file2.ico";
                        tn.StateImageKey = "file2.ico";
                    }
                }

                string ext = file.FileExt;
                if (ext == "s" || ext == "c" || ext == "cpp" || ext == "cxx" || ext == "pde")
                {
                    // only source files can be compiled

                    if (file.ToCompile)
                    {
                        tn.Checked = true;
                    }

                    sourceNodeList.Add(tn);
                    //sourceNode.Nodes.Add(tn);
                }
                else if (ext == "h" || ext == "hpp")
                {
                    //headerNode.Nodes.Add(tn);
                    headerNodeList.Add(tn);
                }
                else
                {
                    //otherNode.Nodes.Add(tn);
                    otherNodeList.Add(tn);
                }
            }

            sourceNodeList.Sort((x, y) => string.Compare(x.Text, y.Text));
            sourceNode.Nodes.AddRange(sourceNodeList.ToArray());

            headerNodeList.Sort((x, y) => string.Compare(x.Text, y.Text));
            headerNode.Nodes.AddRange(headerNodeList.ToArray());

            otherNodeList.Sort((x, y) => string.Compare(x.Text, y.Text));
            otherNode.Nodes.AddRange(otherNodeList.ToArray());

            treeView1.ExpandAll();

            //treeView1.ResumeLayout();

            if (SettingsManagement.AutocompleteEnable)
                KeywordScanner.DoMoreWork();
        }

        public SaveResult AddFileWiz(out ProjectFile file)
        {
            file = null;

            FileAddWizard faw = new FileAddWizard(project);
            if (faw.ShowDialog() == DialogResult.OK)
            {
                if (faw.CreatedFile != null)
                {
                    file = faw.CreatedFile;
                    PopulateList();
                    return SaveResult.Successful;
                }
                else
                {
                    return SaveResult.Failed;
                }
            }
            else
                return SaveResult.Cancelled;
        }

        public SaveResult AddNewFile(out ProjectFile file)
        {
            file = null;

            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Title = "Add New File";

            sfd.InitialDirectory = project.DirPath;

            string filter = GetSaveFileFilters();
            sfd.Filter = filter;
            sfd.FilterIndex = SettingsManagement.LastFileTypeFilter;

            sfd.AddExtension = true;

            sfd.OverwritePrompt = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SettingsManagement.LastFileTypeFilter = sfd.FilterIndex;
                return AddFile(out file, sfd.FileName);
            }

            return SaveResult.Cancelled;
        }

        public SaveResult AddExistingFile(out ProjectFile file)
        {
            file = null;

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title = "Add Existing File(s)";

            ofd.InitialDirectory = project.DirPath;

            string filter = "";
            filter += "Code/Header Files (*.c;*.cpp;*.S;*.pde;*.h;*.hpp)|*.c;*.cpp;*.S;*.pde;*.h;*.hpp" + "|";
            filter += GetSaveFileFilters();
            ofd.Filter = filter;
            ofd.FilterIndex = 0;

            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SaveResult result = SaveResult.Successful;
                foreach (string fileName in ofd.FileNames)
                {
                    SaveResult addFileResult = AddFile(out file, fileName);
                    if (addFileResult != SaveResult.Successful)
                    {
                        result = addFileResult;
                    }
                }
                return result;
            }

            return SaveResult.Cancelled;
        }

        private string GetSaveFileFilters()
        {
            string filter = "";
            filter += "C Source Code (*.c)|*.c" + "|";
            filter += "CPP Source Code (*.cpp)|*.cpp" + "|";
            filter += "Assembly Source Code (*.S)|*.S" + "|";
            filter += "Arduino Source Code (*.pde)|*.pde" + "|";
            filter += "H Header File (*.h)|*.h" + "|";
            filter += "HPP Header File (*.hpp)|*.hpp" + "|";
            filter += "Any File (*.*)|*.*";
            return filter;
        }

        public SaveResult AddFile(out ProjectFile file, string filePath)
        {
            string fn = Path.GetFileName(filePath);
            string ext = Path.GetExtension(fn).ToLowerInvariant();

            if (project.FileList.TryGetValue(fn.ToLowerInvariant(), out file))
            {
                if (file.FileAbsPath != filePath && file.Exists)
                {
                    // name conflict, do not allow
                    MessageBox.Show("Error, Cannot Add File " + file.FileName + " Due To Name Conflict");
                    return SaveResult.Failed;
                }
                else
                {
                    // added file already in list, maybe it was missing, so refresh the list to update icons
                    PopulateList();
                    return SaveResult.Cancelled;
                }
            }
            else
            {
                if (ext == ".c" || ext == ".cpp" || ext == ".cxx" || ext == ".s" || ext == ".h" || ext == ".hpp")
                {
                    // check for space if it's a source or header file, we don't care about the other files
                    if (fn.Contains(" "))
                    {
                        MessageBox.Show("Error, File Name May Not Contain Spaces");
                        return SaveResult.Failed;
                    }
                }

                file = new ProjectFile(filePath, this.project);

                if (file.Exists == false)
                {
                    try
                    {
                        StreamWriter newFile = new StreamWriter(file.FileAbsPath);

                        if (file.FileExt == "h" || file.FileExt == "hpp")
                        {
                            newFile.WriteLine(FileTemplate.CreateFile(file.FileName, project.FileNameNoExt, "defaultheader.txt"));
                        }
                        else if (file.FileExt == "c" || file.FileExt == "cpp")
                        {
                            newFile.WriteLine(FileTemplate.CreateFile(file.FileName, project.FileNameNoExt, "defaultcode.txt"));
                        }
                        else
                            newFile.WriteLine(FileTemplate.CreateFile(file.FileName, project.FileNameNoExt, "default_" + file.FileExt + ".txt"));

                        newFile.Close();
                    }
                    catch (Exception ex)
                    {
                        ErrorReportWindow.Show(ex, "Error Creating New File " + file.FileName);

                    }
                }

                project.FileList.Add(fn.ToLowerInvariant(), file);

                if (project.Save() == SaveResult.Failed)
                {
                    MessageBox.Show("Error saving project");
                }

                PopulateList();
                return SaveResult.Successful;
            }
        }

        public SaveResult AddNewFile(string filePath)
        {
            ProjectFile file;
            return AddFile(out file, filePath);
        }

        #endregion

        #region Event Handlers

        private void mbtnRename_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeView1.SelectedNode;

            if (tn != null)
                if (tn != rootNode && tn != sourceNode && tn != headerNode && tn != otherNode && !IsNoticeNode(tn))
                    tn.BeginEdit();
        }

        private bool IsNoticeNode(TreeNode tn)
        {
            if (updateNoticeNode == null)
                return false;
            if (tn == updateNoticeNode)
                return true;
            if (updateNoticeNode.Nodes.Contains(tn))
                return true;

            return false;
        }

        private void mbtnDelete_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeView1.SelectedNode;

            if (tn != null)
                if (tn != rootNode && tn != sourceNode && tn != headerNode && tn != otherNode && !IsNoticeNode(tn))
                    RemoveNode(tn);
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                TreeNode tn = treeView1.SelectedNode;

                if (tn != null)
                    if (tn != rootNode && tn != sourceNode && tn != headerNode && tn != otherNode)
                        RemoveNode(tn);
            }
            else if (e.KeyCode == Keys.F2)
            {
                TreeNode tn = treeView1.SelectedNode;

                if (tn != null)
                    if (tn != rootNode && tn != sourceNode && tn != headerNode && tn != otherNode && !IsNoticeNode(tn))
                        tn.BeginEdit();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (project == null)
                    return;

                TreeNode n = this.treeView1.SelectedNode;

                if (n != sourceNode && n != headerNode && n != rootNode && n != otherNode && !IsNoticeNode(n))
                {
                    OpenNode(n);
                }
                else
                {
                    if (n == rootNode && string.IsNullOrEmpty(project.DirPath) == false)
                    {
                        System.Diagnostics.Process.Start(project.DirPath + Path.DirectorySeparatorChar);
                    }
                    else if (n != sourceNode)
                    {
                        n.Checked = false;
                    }
                }
            }
        }

        private void mbtnSetOpt_Click(object sender, EventArgs e)
        {
            TreeNode tn = treeView1.SelectedNode;

            if (tn != null)
                if (tn != rootNode && tn != sourceNode && tn != headerNode && tn != otherNode && tn.Parent == sourceNode && !IsNoticeNode(tn))
                {
                    ProjectFile file;
                    if (project.FileList.TryGetValue(tn.Text.ToLowerInvariant(), out file))
                    {
                        if (file.FileExt != "pde")
                        {
                            FileOptionsDialog optForm = new FileOptionsDialog(file);
                            optForm.ShowDialog();
                        }
                    }
                }
        }

        private void mbtnAddNewFile_Click(object sender, EventArgs e)
        {
            if (project == null)
                return;

            if (project.IsReady == false)
                return;

            ProjectFile file;
            if (AddNewFile(out file) == SaveResult.Successful)
                OpenNode(new TreeNode(file.FileName)); // this is cheating, but i don't want to write another open event
        }

        private void mbtnAddExistingFile_Click(object sender, EventArgs e)
        {
            if (project == null)
                return;

            if (project.IsReady == false)
                return;

            ProjectFile file;
            if (AddExistingFile(out file) == SaveResult.Successful)
                OpenNode(new TreeNode(file.FileName)); // this is cheating, but i don't want to write another open event
        }

        private void mbtnAddFileWiz_Click(object sender, EventArgs e)
        {
            if (project == null)
                return;

            if (project.IsReady == false)
                return;

            ProjectFile file;
            if (AddFileWiz(out file) == SaveResult.Successful)
                OpenNode(new TreeNode(file.FileName)); // this is cheating, but i don't want to write another open event
        }

        private void treeView1_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node == sourceNode || e.Node == headerNode || e.Node == rootNode || e.Node == otherNode || IsNoticeNode(e.Node))
            {
                e.CancelEdit = true;
                return;
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Node == sourceNode || e.Node == headerNode || e.Node == rootNode || e.Node == otherNode || IsNoticeNode(e.Node))
            {
                e.CancelEdit = true;
                return;
            }

            if (e.Label == null)
            {
                e.CancelEdit = true;
                return;
            }

            if (RenameNode(e.Node, e.Label) == false)
            {
                e.CancelEdit = true;
                return;
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (project == null)
                return;

            if (e.Node != sourceNode && e.Node != headerNode && e.Node != rootNode && e.Node != otherNode && !IsNoticeNode(e.Node))
            {
                OpenNode(e.Node);
            }
            else
            {
                if (e.Node == rootNode && string.IsNullOrEmpty(project.DirPath) == false)
                {
                    System.Diagnostics.Process.Start(project.DirPath + Path.DirectorySeparatorChar);
                }
                else if (e.Node != sourceNode)
                {
                    e.Node.Checked = false;
                }
            }
        }

        private void treeView1_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void treeView1_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if ((e.Node == sourceNode || e.Node == headerNode || e.Node == rootNode || e.Node == otherNode) || e.Node.Parent != sourceNode || IsNoticeNode(e.Node))
            {
                e.Cancel = true;
                return;
            }

            string ext = e.Node.Text.ToLowerInvariant().Trim();
            if (ext.Contains(".") == false)
            {
                e.Cancel = true;
                return;
            }

            ext = ext.Substring(ext.LastIndexOf('.'));
            if (ext != ".c" && ext != ".cpp" && ext != ".cxx" && ext != ".s" && ext != ".pde")
            {
                e.Cancel = true;
                return;
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node != sourceNode && e.Node != headerNode && e.Node != rootNode && e.Node != otherNode && e.Node.Parent == sourceNode && !IsNoticeNode(e.Node))
            {
                ProjectFile f;
                if (project.FileList.TryGetValue(e.Node.Text.ToLowerInvariant(), out f))
                {
                    f.ToCompile = e.Node.Checked;
                }
            }
        }

        /// <summary>
        /// Fixes a bug that makes you first left click on a node then right click, this function selects the node
        /// before the menu shows up, thus now you can right click and the right node will be sent to the context
        /// menu button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;
        }

        private void tmrHeaderUnchecker_Tick(object sender, EventArgs e)
        {
            try
            {
                if (rootNode.Checked == false)
                    rootNode.Checked = true;

                if (sourceNode.Checked == false)
                    sourceNode.Checked = true;

                if (headerNode.Checked == true)
                    headerNode.Checked = false;

                if (updateNoticeNode != null && updateNoticeNode.Checked == true)
                    updateNoticeNode.Checked = false;


                foreach (TreeNode n in headerNode.Nodes)
                {
                    if (n.Checked == true)
                        n.Checked = false;
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        private void mbtnEnableCompileAll_Click(object sender, EventArgs e)
        {
            if (this.project == null)
                return;

            if (this.project.IsReady == false)
                return;

            if (this.treeView1.SelectedNode != this.sourceNode)
            {
                MessageBox.Show("You can only compile source code files");
                return;
            }

            foreach (ProjectFile f in this.project.FileList.Values)
            {
                if (f.IsSource)
                    f.ToCompile = true;

                f.Node.Checked = true;
            }
        }

        #region Drag and Drop Event Handling

        private delegate SaveResult DragInFile(string filePath);

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            if (project.IsReady == false)
                return;

            try
            {
                Array a = (Array)e.Data.GetData(DataFormats.FileDrop);

                if (a != null)
                {
                    foreach (string filePath in a)
                    {
                        if (File.Exists(filePath))
                            this.BeginInvoke(new DragInFile(AddNewFile), new object[] { filePath, });
                    }

                    this.Activate();
                }
            }
            catch { }
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            if (project.IsReady == false)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        #endregion
    }
}
