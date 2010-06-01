﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace OfficeOpenXml.Style
{
    public class ExcelRichTextCollection : XmlHelper, IEnumerable<ExcelRichText>
    {
        List<ExcelRichText> _list = new List<ExcelRichText>();
        ExcelRangeBase _cells=null;
        internal ExcelRichTextCollection(XmlNamespaceManager ns, XmlNode topNode) :
            base(ns, topNode)
        {
            var nl = topNode.SelectNodes("r", NameSpaceManager);
            if (nl != null)
            {
                foreach (XmlNode n in nl)
                {
                    _list.Add(new ExcelRichText(ns, n));
                }
            }
        }
        internal ExcelRichTextCollection(XmlNamespaceManager ns, XmlNode topNode, ExcelRangeBase cells) :
            this(ns, topNode)
        {
            _cells = cells;
        }        
        public ExcelRichText this[int Index]
        {
            get
            {
                return _list[Index];
            }
        }
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }
        /// <summary>
        /// Add a rich text string
        /// </summary>
        /// <param name="Text">The text to add</param>
        /// <returns></returns>
        public ExcelRichText Add(string Text)
        {
            XmlDocument doc;
            if (TopNode is XmlDocument)
            {
                doc = TopNode as XmlDocument;
            }
            else
            {
                doc = TopNode.OwnerDocument;
            }
            var node = doc.CreateElement("d", "r", ExcelPackage.schemaMain);
            TopNode.AppendChild(node);
            var rt = new ExcelRichText(NameSpaceManager, node);
            if (_list.Count > 0)
            {
                ExcelRichText prevItem = _list[_list.Count - 1];
                rt.FontName = prevItem.FontName;
                rt.Size = prevItem.Size;
                rt.Color = prevItem.Color;
                rt.PreserveSpace = rt.PreserveSpace;
                rt.Bold = prevItem.Bold;
                rt.Italic = prevItem.Italic;                
                rt.UnderLine = prevItem.UnderLine;
            }
            else if (_cells == null)
            {
                rt.FontName = "Calibri";
                rt.Size = 11;
            }
            else
            {
                var style = _cells.Offset(0, 0).Style;
                rt.FontName = style.Font.Name;
                rt.Size = style.Font.Size;
                rt.Bold = style.Font.Bold;
                rt.Italic = style.Font.Italic;
                _cells.IsRichText = true;
            }
            rt.Text = Text;
            rt.PreserveSpace = true;
            if(_cells!=null) rt.SetCallback(UpdateCells);
            _list.Add(rt);
            return rt;
        }
        internal void UpdateCells()
        {
            _cells.SetValueRichText(TopNode.InnerXml);
        }
        public void Clear()
        {
            _list.Clear();
            TopNode.RemoveAll();
            if (_cells != null) _cells.IsRichText = false;
        }
        public void RemoveAt(int Index)
        {
            TopNode.RemoveChild(_list[Index].TopNode);
            _list.RemoveAt(Index);
            if (_cells != null && _list.Count==0) _cells.IsRichText = false;
        }
        public void Remove(ExcelRichText Item)
        {
            TopNode.RemoveChild(Item.TopNode);
            _list.Remove(Item);
            if (_cells != null && _list.Count == 0) _cells.IsRichText = false;
        }
        //public void Insert(int index, string Text)
        //{
        //    _list.Insert(index, item);
        //}
        public string Text
        {
            get
            {
                StringBuilder sb=new StringBuilder();
                foreach (var item in _list)
                {
                    sb.Append(item.Text);
                }
                return sb.ToString();
            }
            set
            {
                if (Count == 0)
                {
                    Add(value);
                }
                else
                {
                    this[0].Text = value;
                    for (int ix = 1; ix < Count; ix++)
                    {
                        RemoveAt(ix);
                    }
                }
            }
        }
        #region IEnumerable<ExcelRichText> Members

        IEnumerator<ExcelRichText> IEnumerable<ExcelRichText>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion
    }
}
