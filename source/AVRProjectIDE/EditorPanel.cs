﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ScintillaNet;
using WeifenLuo.WinFormsUI.Docking;

namespace AVRProjectIDE
{
    public partial class EditorPanel : DockContent
    {

        #region Fields and Properties

        public string FileName
        {
            get { return file.FileName; }
            set { file.FileName = value; }
        }

        List<string> lastAutocompleteList;

        private AVRProject project;

        private ProjectFile file;
        public ProjectFile File
        {
            get { return file; }
        }

        private bool hasChanged;
        public bool HasChanged
        {
            get { return hasChanged | Scint.Modified; }
            set { hasChanged = value; Scint.Modified = value; }
        }

        public Scintilla Scint
        {
            get { return scint; }
        }
        
        public FileSystemWatcher ExternChangeWatcher
        {
            get { return fileSystemWatcher1; }
        }
        public bool WatchingForChange
        {
            get { return fileSystemWatcher1.EnableRaisingEvents; }
            set { fileSystemWatcher1.EnableRaisingEvents = value; }
        }

        private bool closeWithoutSave = false;

        #endregion

        #region Events and Delegates

        public event RenamedEventHandler OnRename;

        public delegate void CloseAllButMe(string fileName);
        public event CloseAllButMe CloseAllExceptMe;

        #endregion

        public EditorPanel(ProjectFile file, AVRProject project)
        {
            InitializeComponent();

            this.file = file;
            this.project = project;

            this.timerBackupMaker.Interval = SettingsManagement.BackupInterval * 1000;
        }

        private void EditorPanelContent_Shown(object sender, EventArgs e)
        {
            if (LoadFile())
            {
                file.IsOpen = true;

                scint = SettingsManagement.SetScintSettings(scint);
                scint = KeywordImageGen.ApplyImageList(scint);

                fileSystemWatcher1.Filter = file.FileName;
                fileSystemWatcher1.Path = file.FileDir + Path.DirectorySeparatorChar;
                fileSystemWatcher1.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                fileSystemWatcher1.EnableRaisingEvents = true;
            }
            else
            {
                HasChanged = false;
                this.Close();
            }
        }

        #region Saving and Loading

        private bool WriteToFile(string path)
        {
            // obviously the filesystem watcher will know if you rewrite the file, so disable it
            bool wasWatching = WatchingForChange;
            WatchingForChange = false;

            path = Program.CleanFilePath(path);

            bool success = true;

            if (Program.MakeSurePathExists(path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar))) == false)
                return false;

            try
            {
                KeywordScanner.FeedFileContent(file, scint.Text);
                KeywordScanner.DoMoreWork();
                System.IO.File.WriteAllText(file.FileAbsPath, scint.Text);
            }
            catch { success = false; }

            WatchingForChange = wasWatching;

            return success;
        }

        public SaveResult Save()
        {
            if (string.IsNullOrEmpty(file.FileAbsPath) == false)
                return Save(file.FileAbsPath);
            else
                return SaveAs();
        }

        public SaveResult SaveAs()
        {
            saveFileDialog1.Filter = String.Format("{0} File (*.{0})|*.{0}|Any File (*.*)|*.*", file.FileExt);

            if (string.IsNullOrEmpty(project.DirPath) == false)
                saveFileDialog1.InitialDirectory = file.FileDir;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SaveResult res = Save(saveFileDialog1.FileName);

                return res;
            }
            else
                return SaveResult.Cancelled;
        }

        public SaveResult Save(string path)
        {
            fileSystemWatcher1.EnableRaisingEvents = false;

            if (WriteToFile(path))
            {
                DeleteBackup();

                string oldName = FileName;

                FileName = Path.GetFileName(path);

                fileSystemWatcher1.Filter = FileName;
                fileSystemWatcher1.Path = file.FileDir + Path.DirectorySeparatorChar;
                this.Text = FileName;
                this.TabText = FileName;

                if (oldName != FileName)
                    OnRename(this, new RenamedEventArgs(WatcherChangeTypes.Renamed, file.FileDir, FileName, oldName));

                fileSystemWatcher1.EnableRaisingEvents = true;

                scint.Modified = false;
                hasChanged = false;

                return SaveResult.Successful;
            }
            else
            {
                fileSystemWatcher1.EnableRaisingEvents = true;
                return SaveResult.Failed;
            }
        }

        public void DeleteBackup()
        {
            file.DeleteBackup();
        }

        private bool ReadFromFile(string path)
        {
            path = Program.CleanFilePath(path);

            bool success = true;

            try
            {
                scint.Text = System.IO.File.ReadAllText(file.FileAbsPath).TrimEnd();
            }
            catch { success = false; }

            KeywordScanner.FeedFileContent(file, scint.Text);
            KeywordScanner.DoMoreWork();

            scint.Modified = false;
            hasChanged = false;
            scint.UndoRedo.EmptyUndoBuffer();

            return success;
        }

        public bool LoadFile()
        {
            if (file.BackupExists)
            {
                if (MessageBox.Show("A backup of " + file.FileName + " still exists, load that instead?", "Backup Found", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (ReadFromFile(file.BackupPath))
                    {
                        DeleteBackup();

                        fileSystemWatcher1.Filter = FileName;
                        fileSystemWatcher1.Path = file.FileDir + Path.DirectorySeparatorChar;
                        this.Text = FileName;
                        this.TabText = FileName;

                        return true;
                    }
                    else
                        MessageBox.Show("Error Loading Backup");
                }
            }

            if (file.Exists)
                return LoadFile(file.FileAbsPath);
            else
            {
                if (MessageBox.Show(file.FileName + " Can't be Found\r\nDo you want to find it?", "File Not Found", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    openFileDialog1.Filter = String.Format("{0} File (*.{0})|*.{0}", file.FileExt);

                    if (string.IsNullOrEmpty(project.DirPath) == false)
                        saveFileDialog1.InitialDirectory = file.FileDir;

                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        if (LoadFile(openFileDialog1.FileName))
                        {
                            file.FileAbsPath = openFileDialog1.FileName;

                            return true;
                        }
                        else
                            return false; // Read Failed
                    }
                    else
                        return false; // User Cancelled
                }
                else
                    return false; // User Refused
            }
        }

        public bool LoadFile(string path)
        {
            if (ReadFromFile(path))
            {
                DeleteBackup();

                string oldName = FileName;

                FileName = Path.GetFileName(path);

                fileSystemWatcher1.Filter = FileName;
                fileSystemWatcher1.Path = file.FileDir + Path.DirectorySeparatorChar;
                this.Text = FileName;
                this.TabText = FileName;

                if (oldName != FileName)
                    OnRename(this, new RenamedEventArgs(WatcherChangeTypes.Renamed, file.FileDir, FileName, oldName));

                return true;
            }
            else
            {
                MessageBox.Show("Error Loading " + Path.GetFileName(path));
                return false;
            }
        }

        #endregion

        #region Events Handlers

        private void timerChangeMonitor_Tick(object sender, EventArgs e)
        {
            if (scint.Modified)
            {
                hasChanged = scint.Modified;
                this.Text = FileName + " *";
                this.TabText = FileName + " *";
            }
        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new FileSystemEventHandler(fileSystemWatcher1_Changed), new object[] { sender, e, });
            }
            else
            {
                if (e.ChangeType == WatcherChangeTypes.Changed && e.FullPath == file.FileAbsPath)
                {
                    if (MessageBox.Show(file.FileName + " was Changed on Disk\r\nDo you want to reload it (yes = load file from disk into editor)?", "External Edit Detected", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        ReadFromFile(file.FileAbsPath);
                }
            }
        }

        private void fileSystemWatcher1_Renamed(object sender, RenamedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new RenamedEventHandler(fileSystemWatcher1_Renamed), new object[] { sender, e, });
            }
            else
            {
                if (e.ChangeType == WatcherChangeTypes.Renamed)
                {
                    if (Path.GetFileName(e.FullPath) == FileName)
                        return;

                    ProjectFile f;
                    if (project.FileList.TryGetValue(e.Name, out f))
                        return;

                    string oldName = FileName;
                    FileName = Path.GetFileName(e.FullPath);

                    fileSystemWatcher1.Filter = FileName;
                    fileSystemWatcher1.Path = file.FileDir + Path.DirectorySeparatorChar;

                    if (HasChanged)
                    {
                        this.Text = FileName + " *";
                        this.TabText = FileName + " *";
                    }
                    else
                    {
                        this.Text = FileName;
                        this.TabText = FileName;
                    }

                    OnRename(this, e);
                }
            }
        }

        private void timerBackupMaker_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(file.FileAbsPath) == false && HasChanged)
                WriteToFile(file.BackupPath);
        }

        private void EditorPanelContent_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (HasChanged && closeWithoutSave == false)
            {
                DialogResult res = MessageBox.Show("You Have Not Saved " + FileName + "\r\nWould you like to save it?", "Closing Unsaved File", MessageBoxButtons.YesNoCancel);
                if (res == DialogResult.Yes)
                {
                    if (Save() != SaveResult.Successful)
                        e.Cancel = true;
                }
                else if (res == DialogResult.Cancel)
                    e.Cancel = true;

                if (e.Cancel == false)
                    file.IsOpen = false;
            }
            if (e.Cancel == false)
            {
                WatchingForChange = false;
                File.DeleteBackup();
            }
        }

        public event EditorClosedEvent EditorClosed;
        public delegate void EditorClosedEvent(string fileName, object sender, FormClosedEventArgs e);

        private void EditorPanelContent_FormClosed(object sender, FormClosedEventArgs e)
        {
            EditorClosed(FileName, sender, e);
        }

        private void EditorPanel_Activated(object sender, EventArgs e)
        {
            scint.Focus();
        }

        #endregion

        #region Edit and Navigation Actions

        public void BlockComment()
        {
            scint.Focus();
            scint.Lexing.LineComment();
        }

        public void BlockUncomment()
        {
            scint.Focus();
            scint.Lexing.LineUncomment();
        }

        public void BlockIndent()
        {
            scint.Focus();
            if (scint.Selection.Length > 0 && scint.Lines.FromPosition(scint.Selection.Start) != scint.Lines.FromPosition(scint.Selection.End))
                SendKeys.Send("{TAB}");
            else
                SendKeys.Send("{HOME}{TAB}");
        }

        public void BlockUnindent()
        {
            scint.Focus();
            if (scint.Selection.Length > 0 && scint.Lines.FromPosition(scint.Selection.Start) != scint.Lines.FromPosition(scint.Selection.End))
                SendKeys.Send("+{TAB}");
            else
                SendKeys.Send("{HOME}+{TAB}");
        }

        public void GoTo(int line)
        {
            scint.Focus();
            int pos = scint.Lines[line].StartPosition;
            scint.Selection.Start = pos;
            scint.Selection.End = pos;
            scint.GoTo.Line(line);
        }

        public void GoTo(int start, int end)
        {
            scint.Focus();
            scint.Selection.Start = start;
            scint.Selection.End = end;
            scint.GoTo.Position(start);
        }

        public void ClearBookmarks()
        {
            scint.Markers.DeleteAll();
        }

        public void ClearHighlights()
        {
            scint.FindReplace.ClearAllHighlights();
        }

        public void SelectAll()
        {
            scint.Focus();
            scint.Selection.SelectAll();
        }

        public void Cut()
        {
            scint.Focus();
            scint.Clipboard.Cut();
        }

        public void Copy()
        {
            scint.Focus();
            scint.Clipboard.Copy();
        }

        public void Paste()
        {
            scint.Focus();
            scint.Clipboard.Paste();
        }

        public void Find()
        {
            scint.FindReplace.Find(scint.Selection.Text);
        }

        public void FindWindow()
        {
            scint.FindReplace.ShowFind();
        }

        public void ReplaceWindow()
        {
            scint.FindReplace.ShowReplace();
        }

        public void FindNext()
        {
            scint.FindReplace.FindNext(scint.FindReplace.LastFindString, true);
        }

        public List<Range> FindAll(string needle, SearchFlags flags)
        {
            return scint.FindReplace.FindAll(needle, flags);
        }

        public void Undo()
        {
            scint.UndoRedo.Undo();
        }

        public void Redo()
        {
            scint.UndoRedo.Redo();
        }

        #endregion

        #region Right Click Menu Button Actions

        private void mbtnSelectAll_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        private void mbtnCut_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void mbtnCopy_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void mbtnPaste_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void mbtnFind_Click(object sender, EventArgs e)
        {
            scint.FindReplace.ShowFind();
        }

        private void mbtnComment_Click(object sender, EventArgs e)
        {
            BlockComment();
        }

        private void mbtnUncomment_Click(object sender, EventArgs e)
        {
            BlockUncomment();
        }

        private void mbtnIndent_Click(object sender, EventArgs e)
        {
            BlockIndent();
        }

        private void mbtnUnindent_Click(object sender, EventArgs e)
        {
            BlockUnindent();
        }

        #endregion

        #region Tab Context Menu Button Actions

        private void mbtnCloseMe_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mbtnCloseExceptMe_Click(object sender, EventArgs e)
        {
            CloseAllExceptMe(FileName);
        }

        #endregion

        public void Close(bool toSave)
        {
            if (toSave == false)
            {
                closeWithoutSave = true;
            }

            this.Close();
        }

        private void scint_CharAdded(object sender, CharAddedEventArgs e)
        {
            if (SettingsManagement.AutocompleteEnable == false)
                return;

            if (scint.AutoComplete.IsActive == false && "abcdefghijklmnopqrstuvwxyz_ABCDEFGHIJKLMNOPQRSTUVWXYZ#".Contains(e.Ch) && scint.PositionIsOnComment(scint.CurrentPos) == false)
            {
                string line = scint.Lines.Current.Text;
                int lineLen = scint.Caret.Position - scint.Lines.Current.StartPosition;
                line = "\r\n" + line + "\r\n";
                bool inString = false;
                bool inChar = false;
                for (int i = 2; i < lineLen + 2; i++)
                {
                    if (line[i] == '\\' && line[i + 1] == '\\' && (inString || inChar))
                    {
                        line = line.Remove(i, 2);
                        line = line.Insert(i, "  ");
                    }
                    else if (line[i] == '"' && line[i - 1] != '\\' && inChar == false)
                    {
                        inString = !inString;
                    }
                    else if (line[i] == '\'' && line[i - 1] != '\\' && inString == false)
                    {
                        inChar = !inChar;
                    }
                }

                if (inString == false && inChar == false)
                {
                    if (e.Ch == '#')
                    {
                        lastAutocompleteList = KeywordScanner.GetPreprocKeywords();

                        string w = scint.GetWordFromPosition(scint.CurrentPos);

                        bool found = false;
                        foreach (string i in lastAutocompleteList)
                        {
                            if (i.StartsWith(w))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (found)
                            scint.AutoComplete.Show(lastAutocompleteList);
                    }
                    else
                    {
                        lastAutocompleteList = KeywordScanner.GetKeywordsUpTo(file, scint.Text.Substring(0, scint.NativeInterface.WordStartPosition(scint.CurrentPos, true)));

                        string w = scint.GetWordFromPosition(scint.CurrentPos);

                        bool found = false;
                        foreach (string i in lastAutocompleteList)
                        {
                            if (i.StartsWith(w))
                            {
                                found = true;
                                break;
                            }
                        }

                        if (found)
                            scint.AutoComplete.Show(lastAutocompleteList);
                    }
                }
            }
        }

        private void scint_AutoCompleteAccepted(object sender, AutoCompleteAcceptedEventArgs e)
        {
            return;
            e.Cancel = true;

            int start = scint.NativeInterface.WordStartPosition(scint.CurrentPos, true);
            int backspace = scint.CurrentPos - start;
            int end = scint.NativeInterface.WordEndPosition(scint.CurrentPos, true);
            int delete = end - scint.CurrentPos;

            string word = scint.GetWordFromPosition(start);

            if (e.Text.StartsWith(word) == false)
            {
                for (int i = 0; i < delete; i++)
                {
                    SendKeys.Send("{RIGHT}");
                    System.Threading.Thread.Sleep(0);
                    System.Threading.Thread.Sleep(0);
                    System.Threading.Thread.Sleep(0);
                    System.Threading.Thread.Sleep(0);
                    System.Threading.Thread.Sleep(0);
                }

                for (int i = 0; i < backspace + delete; i++)
                {
                    SendKeys.Send("{BKSP}");
                    System.Threading.Thread.Sleep(0);
                    System.Threading.Thread.Sleep(0);
                    System.Threading.Thread.Sleep(0);
                    System.Threading.Thread.Sleep(0);
                    System.Threading.Thread.Sleep(0);
                }

                scint.InsertText(start, e.Text);
            }
            else
            {
                scint.InsertText(end, e.Text.Substring(word.Length));
            }

            scint.Selection.Start = start + e.Text.Length;
            scint.Selection.End = scint.Selection.Start;
        }
    }
}