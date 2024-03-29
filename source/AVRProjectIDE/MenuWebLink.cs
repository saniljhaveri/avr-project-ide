﻿using System;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AVRProjectIDE
{
    public class MenuWebLink
    {
        private List<MenuWebLink> children = new List<MenuWebLink>();

        private ToolStripMenuItem menuItem;
        public ToolStripMenuItem MenuItem
        {
            get { return menuItem; }
        }

        private string url;

        public MenuWebLink(string text, string url)
        {
            menuItem = new ToolStripMenuItem(text.Trim());
            url = url.Trim();

            if (string.IsNullOrEmpty(url) == false)
            {
                menuItem.ToolTipText = url;
                this.url = url;
                if (url.ToLowerInvariant().StartsWith("http://") || url.ToLowerInvariant().StartsWith("https://"))
                {
                    menuItem.Image = GraphicsResx.web;
                }
                menuItem.Click += new EventHandler(menuItem_Click);
            }
        }

        public void Add(XmlElement xContainer)
        {
            foreach (XmlElement xEle in xContainer.ChildNodes)
            {
                MenuWebLink child = new MenuWebLink(xEle.GetAttribute("Text"), xEle.GetAttribute("URL"));

                children.Add(child);
                menuItem.DropDownItems.Add(child.MenuItem);

                child.Add(xEle);
            }
        }

        public void Add(string text, string url, bool clickable)
        {
            Add(new MenuWebLink(text, url));
        }

        public void Add(MenuWebLink mwl)
        {
            children.Add(mwl);
            menuItem.DropDownItems.Add(mwl.MenuItem);
        }

        private void OpenURL(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
            }
        }

        void menuItem_Click(object sender, EventArgs e)
        {
            OpenURL(url);
        }

        public static ToolStripMenuItem GetMenuLinkRoot(string text)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.Image = GraphicsResx.web;

            XmlDocument xDoc = new XmlDocument();

            if (File.Exists(SettingsManagement.AppDataPath + "helplinks.xml") == false)
            {
                try
                {
                    File.WriteAllText(SettingsManagement.AppDataPath + "helplinks.xml", Properties.Resources.helplinks);
                    xDoc.Load(SettingsManagement.AppDataPath + "helplinks.xml");
                }
                catch
                {
                    xDoc.LoadXml(Properties.Resources.helplinks);
                }
            }
            else
            {
                try
                {
                    xDoc.Load(SettingsManagement.AppDataPath + "helplinks.xml");
                }
                catch (XmlException ex)
                {
                    MessageBox.Show("Error while reading helplinks.xml: " + ex.Message);
                    return item;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while reading helplinks.xml: " + ex.Message);
                    return item;
                }
            }

            try
            {
                XmlElement xDocEle = xDoc.DocumentElement;

                foreach (XmlElement xEle in xDocEle.ChildNodes)
                {
                    MenuWebLink link = new MenuWebLink(xEle.GetAttribute("Text"), xEle.GetAttribute("URL"));
                    link.Add(xEle);

                    item.DropDownItems.Add(link.MenuItem);
                }
            }
            catch (Exception ex)
            {
                ErrorReportWindow.Show(ex, "Error while creating help links");
            }

            return item;
        }
    }
}
