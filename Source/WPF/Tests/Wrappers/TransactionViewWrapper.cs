﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Input;
using Walkabout.Tests.Interop;

namespace Walkabout.Tests.Wrappers
{
    public class TransactionViewWrapper
    {
        private MainWindowWrapper window;
        private AutomationElement control;
        private static Dictionary<string, TransactionViewColumns> mapping;

        static TransactionViewWrapper()
        {
            mapping = new Dictionary<string, TransactionViewColumns>();
            mapping["TheGrid_BankTransactionDetails"] = new TransactionViewColumns(
                    new TransactionViewColumn("A", "Attachment", "Button"),
                    new TransactionViewColumn("Num", "Number", "TextBox"),
                    new TransactionViewColumn("Date", "Date", "DatePicker"),
                    new TransactionViewColumn("C", "Color", "Custom"),
                    new CompoundTransactionViewColumn("Payee/Category/Memo",
                        new TransactionViewColumn("", "Payee", "ComboBox"), // ExpandCollapse, Selection, Value
                        new TransactionViewColumn("", "Category", "ComboBox"), // ExpandCollapse, Selection, Value
                        new TransactionViewColumn("", "Memo", "TextBox") // ExpandCollapse, Selection, Value
                    ),
                    new TransactionViewColumn("S", "Status", "Button"),
                    new TransactionViewColumn("Sales Tax", "SalesTax", "TextBox"),
                    new CompoundTransactionViewColumn("Payment",
                        new TransactionViewColumn("", "Splits", "Button"),
                        new TransactionViewColumn("Payment", "Payment", "TextBox")
                    ),
                    new CompoundTransactionViewColumn("Deposit",
                        new TransactionViewColumn("", "Splits", "Button"),
                        new TransactionViewColumn("Deposit", "Deposit", "TextBox")
                    )
                );


            mapping["TheGrid_InvestmentActivity"] = new TransactionViewColumns(
                    new TransactionViewColumn("A", "Attachment", "Button"),
                    new TransactionViewColumn("Num", "Number", "TextBox"),
                    new TransactionViewColumn("Date", "Date", "DatePicker"),
                    new TransactionViewColumn("C", "Color", "Custom"),
                    new CompoundTransactionViewColumn("Payee/Category/Memo",
                        new TransactionViewColumn("", "Payee", "ComboBox"), // ExpandCollapse, Selection, Value
                        new TransactionViewColumn("", "Category", "ComboBox"), // ExpandCollapse, Selection, Value
                        new TransactionViewColumn("", "Memo", "TextBox") // ExpandCollapse, Selection, Value
                    ),

                    new TransactionViewColumn("Activity", "Activity", "ComboBox"),
                    new TransactionViewColumn("Security", "Security", "ComboBox"),
                    new TransactionViewColumn("Units", "Units", "TextBox"),
                    new TransactionViewColumn("UnitPrice", "UnitPrice", "TextBox"),

                    new TransactionViewColumn("S", "Status", "Button"),
                    new TransactionViewColumn("Sales Tax", "SalesTax", "TextBox"),
                    new CompoundTransactionViewColumn("Payment",
                        new TransactionViewColumn("", "Splits", "Button"),
                        new TransactionViewColumn("", "Payment", "TextBox")
                    ),
                    new CompoundTransactionViewColumn("Deposit",
                        new TransactionViewColumn("", "Splits", "Button"),
                        new TransactionViewColumn("", "Deposit", "TextBox")
                    )
                );

            mapping["TheGrid_TransactionFromDetails"] = new TransactionViewColumns(
                    new TransactionViewColumn("A", "Attachment", "Button"),
                    new TransactionViewColumn("Account", "Account", "TextBlock"),
                    new TransactionViewColumn("Num", "Number", "TextBox"),
                    new TransactionViewColumn("Date", "Date", "DatePicker"),
                    new TransactionViewColumn("C", "Color", "Custom"),
                    new CompoundTransactionViewColumn("Payee/Category/Memo",
                        new TransactionViewColumn("", "Payee", "ComboBox"), // ExpandCollapse, Selection, Value
                        new TransactionViewColumn("", "Category", "ComboBox"), // ExpandCollapse, Selection, Value
                        new TransactionViewColumn("", "Memo", "TextBox") // ExpandCollapse, Selection, Value
                    ),
                    new TransactionViewColumn("S", "Status", "Button"),
                    new CompoundTransactionViewColumn("Payment",
                        new TransactionViewColumn("", "Splits", "Button"),
                        new TransactionViewColumn("", "Payment", "TextBox")
                    ),
                    new CompoundTransactionViewColumn("Deposit",
                        new TransactionViewColumn("", "Splits", "Button"),
                        new TransactionViewColumn("", "Deposit", "TextBox")
                    )
            );

            mapping["TheGrid_BySecurity"] = new TransactionViewColumns(
                    new TransactionViewColumn("Account", "Account", "TextBlock"),
                    new TransactionViewColumn("Date", "Date", "DatePicker"),
                    new CompoundTransactionViewColumn("Payee/Category/Memo",
                        new TransactionViewColumn("", "Payee", "ComboBox"), // ExpandCollapse, Selection, Value
                        new TransactionViewColumn("", "Category", "ComboBox"), // ExpandCollapse, Selection, Value
                        new TransactionViewColumn("", "Memo", "TextBox") // ExpandCollapse, Selection, Value
                    ),
                    new TransactionViewColumn("Security", "Security", "ComboBox"),
                    new TransactionViewColumn("Activity", "Activity", "ComboBox"),
                    new TransactionViewColumn("FIFO", "FIFO", "Custom"),
                    new TransactionViewColumn("Units", "Units", "TextBox"),
                    new TransactionViewColumn("Units A.S.", "UnitsAS", "TextBlock"),
                    new TransactionViewColumn("Holding", "Holding", "TextBlock"),
                    new TransactionViewColumn("Unit Price", "UnitPrice", "TextBlock"),
                    new TransactionViewColumn("Price A.S.", "UnitPriceAS", "TextBlock"),
                    new TransactionViewColumn("S", "Status", "Button"),
                    new CompoundTransactionViewColumn("Payment",
                        new TransactionViewColumn("", "Splits", "Button"),
                        new TransactionViewColumn("", "Payment", "TextBox")
                    ),
                    new CompoundTransactionViewColumn("Deposit",
                        new TransactionViewColumn("", "Splits", "Button"),
                        new TransactionViewColumn("", "Deposit", "TextBox")
                    )
                );

        }

        public TransactionViewWrapper(MainWindowWrapper window, AutomationElement control)
        {
            this.window = window;
            this.control = control;
        }

        public MainWindowWrapper Window { get { return this.window; } }

        public bool IsEditable
        {
            get
            {
                return this.IsBankAccount || this.IsInvestmentAccount;
            }
        }

        public bool IsBankAccount
        {
            get
            {
                string name = this.control.Current.AutomationId;
                return name == "TheGrid_BankTransactionDetails";
            }
        }

        public bool IsInvestmentAccount
        {
            get
            {
                string name = this.control.Current.AutomationId;
                return name == "TheGrid_InvestmentActivity";
            }
        }

        public bool HasTransactions
        {
            get
            {
                foreach (AutomationElement e in this.control.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem)))
                {
                    if (e.Current.Name == "Walkabout.Data.Transaction")
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int Count
        {
            get
            {
                return this.GetItems().Count;
            }
        }

        public List<TransactionViewItem> GetItems()
        {
            List<TransactionViewItem> list = new List<TransactionViewItem>();
            foreach (AutomationElement e in this.control.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem)))
            {
                if (e.Current.Name == "Walkabout.Data.Transaction")
                {
                    list.Add(new TransactionViewItem(this, e));
                }
            }
            return list;
        }

        public TransactionViewItem Select(int index)
        {
            List<TransactionViewItem> list = this.GetItems();
            if (index >= list.Count)
            {
                throw new ArgumentOutOfRangeException("Index " + index + " is out of range, list only has " + list.Count + " items");
            }

            TransactionViewItem item = list[index];
            item.Select();
            return item;
        }

        public bool HasSelection
        {
            get
            {
                return this.Selection != null;
            }
        }

        public TransactionViewItem Selection
        {
            get
            {
                SelectionPattern selection = (SelectionPattern)this.control.GetCurrentPattern(SelectionPattern.Pattern);
                AutomationElement[] selected = selection.Current.GetSelection();
                return (selected == null || selected.Length == 0) ? null : new TransactionViewItem(this, selected[0]);
            }
        }

        internal void Delete(int index)
        {
            TransactionViewItem item = this.Select(index);
            item.Delete();
        }

        internal TransactionViewColumn GetColumn(string name)
        {
            return this.Columns.GetColumn(name);
        }

        internal TransactionViewColumns Columns
        {
            get
            {
                string gridName = this.control.Current.AutomationId;
                TransactionViewColumns cols;
                if (!mapping.TryGetValue(gridName, out cols))
                {
                    throw new Exception("DataGrid named '" + gridName + "' has no mapping");
                }
                return cols;
            }
        }

        internal TransactionViewItem AddNew()
        {
            this.ScrollToEnd();
            Thread.Sleep(100);

            AutomationElement placeholder = TreeWalker.RawViewWalker.GetLastChild(this.control);
            if (placeholder.Current.Name != "{NewItemPlaceholder}")
            {
                throw new Exception("Expecting {NewItemPlaceholder} at the bottom of the DataGrid");
            }

            // the invoke pattern causes new row to be added
            InvokePattern p = (InvokePattern)placeholder.GetCurrentPattern(InvokePattern.Pattern);
            if (p == null)
            {
                throw new Exception("Expecting {NewItemPlaceholder} at the bottom of the DataGrid");
            }
            p.Invoke();

            // now get the new list of items, the new row is the last one
            List<TransactionViewItem> items = this.GetItems();
            if (items.Count == 0)
            {
                throw new Exception("New row did not get added for some unknown reason");
            }
            return items[items.Count - 1];
        }

        internal void ScrollVertical(double verticalPercent)
        {
            ScrollPattern sp = (ScrollPattern)this.control.GetCurrentPattern(ScrollPattern.Pattern);
            if (sp.Current.VerticallyScrollable && sp.Current.VerticalScrollPercent != verticalPercent)
            {
                sp.SetScrollPercent(System.Windows.Automation.ScrollPattern.NoScroll, verticalPercent);
            }
        }

        internal void SortBy(TransactionViewColumn column)
        {
            AutomationElement header = this.control.FindFirstWithRetries(TreeScope.Descendants, new AndCondition(
                new PropertyCondition(AutomationElement.ClassNameProperty, "DataGridColumnHeader"),
                new PropertyCondition(AutomationElement.NameProperty, column.Header)));
            if (header != null)
            {
                InvokePattern p = (InvokePattern)header.GetCurrentPattern(InvokePattern.Pattern);
                p.Invoke();
            }
            else
            {
                Debug.WriteLine("Could not find header for column: " + column.Name);
            }
        }

        internal void ScrollToEnd()
        {
            this.ScrollVertical(100);
        }

        internal void CommitEdit()
        {
            var selection = this.Selection;
            if (selection != null)
            {
                selection.Focus();
                Thread.Sleep(30);
                Input.TapKey(System.Windows.Input.Key.Enter);
                // key sending is completely async, so we have to give it time to arrive and be processed.
                Thread.Sleep(30);
            }
        }

        internal void NavigateTransfer()
        {
            var selection = this.Selection;

            string tofrom;
            string sourceAccount = selection.ParseTransferPayee(out tofrom);
            decimal amount = selection.GetAmount();

            if (sourceAccount == null)
            {
                Assert.IsTrue(sourceAccount != null);
            }

            for (int delay = 100; delay < 500; delay += 100)
            {
                try
                {
                    selection.Focus();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    selection = this.Selection;
                }

                Thread.Sleep(delay);
                Input.TapKey(System.Windows.Input.Key.F12);
                // key sending is completely async, so we have to give it time to arrive and be processed.
                Thread.Sleep(delay);

                for (int retries = 5; retries > 0; retries--)
                {
                    // update the new "transactions" grid in place.
                    this.control = this.window.FindTransactionGrid().control;

                    // now verify we got a new selection
                    var target = this.Selection;
                    if (target == null)
                    {
                        throw new Exception("Navigation lost the selection");
                    }

                    string fromto;
                    string targetAccount = target.ParseTransferPayee(out fromto);
                    decimal targetAmount = target.GetAmount();

                    if (sourceAccount == targetAccount && fromto == tofrom)
                    {
                        // hasn't got the F12 yet.
                    }
                    else if (Math.Round(targetAmount, 2) != -Math.Round(amount, 2))
                    {
                        // we jumped to the wrong place then!
                        throw new Exception("F12 jumped to the wrong transaction");
                    }

                    if (sourceAccount != null && targetAccount != null && ((tofrom == "to" && fromto == "from") || (tofrom == "from" && fromto == "to")))
                    {
                        return;
                    }

                    // not there yet...
                    Thread.Sleep(100);
                }
            }

            throw new Exception("Timeout waiting for F12 to work");
        }

        internal void Export(string name)
        {
            this.control = this.window.FindTransactionGrid().control;
            this.control.SetFocus();
            ContextMenu menu = new ContextMenu(this.control, true);
            menu.InvokeMenuItem("menuItemExport");
        }
    }

    public class TransactionViewItem
    {
        private TransactionViewWrapper view;
        private AutomationElement item;

        public TransactionViewItem(TransactionViewWrapper view, AutomationElement item)
        {
            this.view = view;
            this.item = item;
        }

        public AutomationElement Element { get { return this.item; } }

        public void Select()
        {
            SelectionItemPattern select = (SelectionItemPattern)this.item.GetCurrentPattern(SelectionItemPattern.Pattern);
            select.Select();
            this.ScrollIntoView();
        }

        public void ScrollIntoView()
        {
            ScrollItemPattern scroll = (ScrollItemPattern)this.item.GetCurrentPattern(ScrollItemPattern.Pattern);
            scroll.ScrollIntoView();
        }

        public void Delete()
        {
            this.Select();

            AutomationElement cell = this.item.FindFirstWithRetries(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "DataGridCell"));
            if (cell == null)
            {
                throw new Exception("DataGridCell not found");
            }
            cell.SetFocus();

            Thread.Sleep(30);
            Input.TapKey(Key.Delete);
            Thread.Sleep(30);
        }

        internal AttachmentDialogWrapper ClickAttachmentsButton()
        {
            this.ScrollIntoView();

            // AutomationId:	"CommandScanAttachment"
            try
            {
                TransactionViewColumn col = this.view.GetColumn("Attachment");
                col.Invoke(this.item);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot find the Attachment column: " + ex.Message);
            }

            AutomationElement dialog = this.view.Window.Element.FindChildWindow("Attachments", 5);
            if (dialog == null)
            {
                throw new Exception("Cannot find the AttachmentDialog");
            }
            return new AttachmentDialogWrapper(dialog);
        }

        internal string GetCheckNumber()
        {
            TransactionViewColumn col = this.view.GetColumn("Number");
            return col.GetValue(this.item);
        }
        internal void SetCheckNumber(string num)
        {
            TransactionViewColumn col = this.view.GetColumn("Number");
            col.SetValue(this.item, num);
        }

        internal string GetDate()
        {
            TransactionViewColumn col = this.view.GetColumn("Date");
            return col.GetValue(this.item);
        }
        internal void SetDate(string dateTime)
        {
            TransactionViewColumn col = this.view.GetColumn("Date");
            col.SetValue(this.item, dateTime);
        }

        internal string GetPayee()
        {
            TransactionViewColumn col = this.view.GetColumn("Payee");
            return col.GetValue(this.item);
        }
        internal void SetPayee(string payee)
        {
            TransactionViewColumn col = this.view.GetColumn("Payee");
            col.SetValue(this.item, payee);
        }

        internal string GetCategory()
        {
            TransactionViewColumn col = this.view.GetColumn("Category");
            return col.GetValue(this.item);
        }
        internal void SetCategory(string category)
        {
            TransactionViewColumn col = this.view.GetColumn("Category");
            col.SetValue(this.item, category);
        }
        internal string GetMemo()
        {
            TransactionViewColumn col = this.view.GetColumn("Memo");
            return col.GetValue(this.item);
        }
        internal void SetMemo(string memo)
        {
            TransactionViewColumn col = this.view.GetColumn("Memo");
            col.SetValue(this.item, memo);
        }
        internal decimal GetSalesTax()
        {
            return this.GetDecimalColumn("SalesTax");
        }

        private decimal GetDecimalColumn(string name)
        {
            TransactionViewColumn col = this.view.GetColumn(name);
            string s = col.GetValue(this.item);
            decimal d;
            decimal.TryParse(s, out d);
            return d;
        }
        internal void SetSalesTax(decimal tax)
        {
            TransactionViewColumn col = this.view.GetColumn("SalesTax");
            col.SetValue(this.item, tax == 0 ? "" : tax.ToString());
        }
        internal decimal GetPayment()
        {
            return this.GetDecimalColumn("Payment");
        }
        internal decimal GetDeposit()
        {
            return this.GetDecimalColumn("Deposit");
        }
        internal decimal GetAmount()
        {
            int sign = 1;
            TransactionViewColumn col = this.view.GetColumn("Deposit");
            string s = col.GetValue(this.item);
            if (string.IsNullOrEmpty(s))
            {
                sign = -1;
                col = this.view.GetColumn("Payment");
                s = col.GetValue(this.item);
            }
            decimal p = 0;
            decimal.TryParse(s, out p);
            return p * sign;
        }
        internal void SetAmount(decimal amount)
        {
            if (amount < 0)
            {
                TransactionViewColumn col = this.view.GetColumn("Payment");
                amount *= -1;
                col.SetValue(this.item, amount == 0 ? "" : amount.ToString());
            }
            else
            {
                TransactionViewColumn col = this.view.GetColumn("Deposit");
                col.SetValue(this.item, amount == 0 ? "" : amount.ToString());
            }
        }

        // investment transactions
        internal string GetActivity()
        {
            TransactionViewColumn col = this.view.GetColumn("Activity");
            return col.GetValue(this.item);
        }

        internal void SetActivity(string activity)
        {
            TransactionViewColumn col = this.view.GetColumn("Activity");
            col.SetValue(this.item, activity);
        }

        internal string GetSecurity()
        {
            TransactionViewColumn col = this.view.GetColumn("Security");
            return col.GetValue(this.item);
        }
        internal void SetSecurity(string security)
        {
            TransactionViewColumn col = this.view.GetColumn("Security");
            col.SetValue(this.item, security);
        }

        internal decimal GetUnits()
        {
            return this.GetDecimalColumn("Units");
        }
        internal void SetUnits(decimal units)
        {
            TransactionViewColumn col = this.view.GetColumn("Units");
            col.SetValue(this.item, units == 0 ? "" : units.ToString());
        }

        internal decimal GetUnitPrice()
        {
            return this.GetDecimalColumn("UnitPrice");
        }
        internal void SetUnitPrice(decimal price)
        {
            TransactionViewColumn col = this.view.GetColumn("UnitPrice");
            col.SetValue(this.item, price == 0 ? "" : price.ToString());
        }

        // todo: toggle status button
        // todo: edit splits.

        public bool IsTransfer
        {
            get
            {
                try
                {
                    string payee = this.GetPayee();
                    return payee.StartsWith("Transfer");
                }
                catch
                {
                    // Sometimes get "Invoke operation failed. Cannot edit another cell or row while the current one has validation errors"
                    return false;
                }
            }
        }

        /// <summary>
        /// Parse the transfer string 
        /// </summary>
        /// <param name="tofrom">The string 'to' or 'from' dependending on direction of transfer</param>
        /// <returns>The account that we are transfering to or from</returns>
        public string ParseTransferPayee(out string tofrom)
        {
            string payee = this.GetPayee();

            if (payee.StartsWith("Transfer"))
            {
                payee = payee.Substring(8).Trim();
                int i = payee.IndexOf(':');
                if (i > 0)
                {
                    tofrom = payee.Substring(0, i);
                    return payee.Substring(i + 1).Trim();
                }
            }

            throw new Exception("Expecting a Transfer transaction");
        }

        internal void Focus()
        {
            TransactionViewColumn col = this.view.GetColumn("Payee");
            col.Focus(this.item);
        }
    }

    public class TransactionViewColumn
    {
        private string header;
        private string name;
        private string datatype;
        private int index;

        protected TransactionViewColumn(string header)
        {
            this.header = header;
        }

        public TransactionViewColumn(string header, string name, string datatype)
        {
            this.header = header;
            this.name = name;
            this.datatype = datatype;
        }

        public int Index
        {
            get { return this.index; }
            set { this.index = value; }
        }

        public CompoundTransactionViewColumn Parent { get; set; }

        public string Header { get { return this.header; } }

        public string Name { get { return this.name; } }

        public string DataType { get { return this.datatype; } }


        public AutomationElement GetCell(AutomationElement dataItem)
        {
            ScrollItemPattern scroll = (ScrollItemPattern)dataItem.GetCurrentPattern(ScrollItemPattern.Pattern);
            scroll.ScrollIntoView();

            // find the DataGridCell to activate.
            int index = this.index;
            if (this.Parent != null)
            {
                index = this.Parent.index;
            }

            int i = 0;
            AutomationElement cell = null;
            foreach (AutomationElement e in dataItem.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "DataGridCell")))
            {
                if (i == index)
                {
                    cell = e;
                    break;
                }
                i++;
            }

            if (cell == null)
            {
                throw new Exception("Expecting a DataGridCell to appear at index " + index + " in the DataItem" + this.Name + ". We found " + i + " DataGridCells.");
            }

            return cell;
        }

        private AutomationElement GetCellContent(AutomationElement dataItem, bool forEditing)
        {
            AutomationElement cell = this.GetCell(dataItem);

            string name = cell.Current.ClassName;
            if (!forEditing)
            {
                if (this.Parent == null && !forEditing)
                {
                    return cell;
                }

                AutomationElement found = null;
                int i = 0;
                foreach (AutomationElement child in cell.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.IsEnabledProperty, true)))
                {
                    found = child;
                    name = child.Current.ClassName;
                    // the cell should have children, TextBlocks and so forth from which we can get the value.
                    if (i == this.index)
                    {
                        return child;
                    }
                    i++;
                }

                // Sometimes we have some optional children (like the split buttons on the payment/deposit, so return the previous cell.
                return found;
            }
            else
            {
                AutomationElement editor = null;
                for (int retries = 5; retries > 0 && editor == null; retries--)
                {
                    // invoking the dataItem puts the cell into edit mode, revealing the inner controls
                    InvokePattern p = (InvokePattern)cell.GetCurrentPattern(InvokePattern.Pattern);
                    p.Invoke();

                    Thread.Sleep(50); // let editing mode kick in.

                    // Now find the editable control within cell 
                    editor = this.GetEditor(cell);
                    if (editor == null)
                    {
                        Thread.Sleep(500);
                    }
                }

                if (editor == null)
                {
                    throw new Exception("Editor not found in compound cell at index " + this.Index);
                }

                return editor;
            }

        }

        protected virtual AutomationElement GetEditor(AutomationElement cell)
        {
            int editorIndex = 0;
            string name = cell.Current.ClassName;

            if (this.Parent != null)
            {
                editorIndex = this.Index;
            }
            AutomationElement e = TreeWalker.RawViewWalker.GetFirstChild(cell);
            if (e == null)
            {
                return null;
            }
            name = e.Current.ClassName;
            if (name == "TransactionAmountControl")
            {
                e = TreeWalker.RawViewWalker.GetFirstChild(e);
            }
            int i = 0;
            while (i < editorIndex && e != null && e.Current.ClassName != "TextBox")
            {
                name = e.Current.ClassName;
                e = TreeWalker.RawViewWalker.GetNextSibling(e);
                if (e != null && e.Current.ClassName == "TransactionAmountControl")
                {
                    e = TreeWalker.RawViewWalker.GetFirstChild(e);
                }
                i++;
            }
            name = e.Current.ClassName;
            if (name == "TextBlock")
            {
                return null;
            }
            return e;
        }

        public string GetValue(AutomationElement dataItem)
        {
            int retries = 5;
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    switch (this.DataType)
                    {
                        case "Button":
                        case "Custom":
                            return "";
                        case "TextBox":
                        case "DatePicker":
                        case "ComboBox":
                            return this.GetCellValue(this.GetCellContent(dataItem, false));
                        default:
                            throw new Exception("Unrecognized datatype: " + this.DataType);
                    }
                }
                catch
                {
                    if (i == retries - 1)
                    {
                        throw;
                    }
                }
            }
            return null;
        }

        public void SetValue(AutomationElement dataItem, string value)
        {
            switch (this.DataType)
            {
                case "Button":
                case "TextBlock":
                case "Custom":
                    throw new Exception("Cannot set the value of a " + this.DataType + " column");
                case "DatePicker":
                case "ComboBox":
                case "TextBox":
                    this.SetCellValue(this.GetCellContent(dataItem, true), value);
                    break;
                default:
                    throw new Exception("Unrecognized datatype: " + this.DataType);
            }
        }

        public string GetCellValue(AutomationElement cell)
        {
            if (cell == null)
            {
                // This can happen on Payment/Deposit fields when one or the other has no value.
                return "";
            }

            AutomationElement text = cell.Current.ClassName == "TextBlock" ? cell : null;

            string name = cell.Current.ClassName;

            if (name == "TransactionAmountControl")
            {
                text = TreeWalker.RawViewWalker.GetFirstChild(cell);
                name = text.Current.ClassName;

            }
            else if (name == "DataGridCell")
            {
                text = cell.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, "TextBlock"));
            }

            object obj;
            if (text != null)
            {
                if (text.TryGetCurrentPattern(ValuePattern.Pattern, out obj))
                {
                    ValuePattern vp = (ValuePattern)obj;
                    return vp.Current.Value;
                }

                return text.Current.Name;
            }

            if (cell.TryGetCurrentPattern(ValuePattern.Pattern, out obj))
            {
                ValuePattern vp = (ValuePattern)obj;
                return vp.Current.Value;
            }

            throw new Exception("DataCell for column " + name + " at index " + this.index + " does not have a ValuePatten");
        }

        public void SetCellValue(AutomationElement cell, string value)
        {
            object obj;
            if (cell.TryGetCurrentPattern(ValuePattern.Pattern, out obj))
            {
                ValuePattern vp = (ValuePattern)obj;
                vp.SetValue(value);
                return;
            }
            throw new Exception("DataCell for column " + this.name + " at index " + this.index + " does not have a ValuePatten");
        }

        public void Invoke(AutomationElement dataItem)
        {
            if (this.DataType == "Button")
            {
                AutomationElement cell = this.GetCellContent(dataItem, true);

                object obj;
                if (cell.TryGetCurrentPattern(InvokePattern.Pattern, out obj))
                {
                    InvokePattern invoke = (InvokePattern)obj;
                    invoke.Invoke();
                    return;
                }

                throw new Exception("DataGridCell " + this.Name + " does not contain an InvokePattern");
            }
            else
            {
                throw new Exception("Cannot invoke column of this type, expecting a button column");
            }
        }

        internal void Focus(AutomationElement dataItem)
        {
            AutomationElement cell = this.GetCell(dataItem);
            string name = cell.Current.Name;
            string className = cell.Current.ClassName;
            cell.SetFocus();
        }
    }

    public class CompoundTransactionViewColumn : TransactionViewColumn
    {
        private List<TransactionViewColumn> columns = new List<TransactionViewColumn>();

        public CompoundTransactionViewColumn(string header, params TransactionViewColumn[] cols)
            : base(header)
        {
            if (cols != null)
            {
                int i = 0;
                foreach (TransactionViewColumn tc in cols)
                {
                    this.columns.Add(tc);
                    tc.Parent = this;
                    tc.Index = i++;
                }
            }
        }

        internal TransactionViewColumn GetColumn(string name)
        {
            foreach (TransactionViewColumn tc in this.columns)
            {
                if (tc.Name == name)
                {
                    return tc;
                }
            }
            return null;
        }

    }

    public class TransactionViewColumns
    {
        private List<TransactionViewColumn> columns = new List<TransactionViewColumn>();

        public TransactionViewColumns(params TransactionViewColumn[] cols)
        {
            int i = 0;
            foreach (TransactionViewColumn tc in cols)
            {
                this.columns.Add(tc);
                tc.Index = i++;
            }
        }

        public void AddColumn(string header, string name, string datatype)
        {
            this.columns.Add(new TransactionViewColumn(header, name, datatype));
        }

        public int Count
        {
            get { return this.columns.Count; }
        }

        public TransactionViewColumn GetColumn(string name)
        {
            foreach (TransactionViewColumn tc in this.columns)
            {
                CompoundTransactionViewColumn cc = tc as CompoundTransactionViewColumn;
                if (cc != null)
                {
                    TransactionViewColumn inner = cc.GetColumn(name);
                    if (inner != null)
                    {
                        return inner;
                    }
                }
                else if (tc.Name == name)
                {
                    return tc;
                }
            }
            throw new Exception("Column of name '" + name + "' not found");
        }


        internal TransactionViewColumn GetColumn(int index)
        {
            if (index < 0 || index >= this.columns.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return this.columns[index];
        }

    }

}
