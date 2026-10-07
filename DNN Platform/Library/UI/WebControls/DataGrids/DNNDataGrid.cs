// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    /// <summary>The DNNDataGrid control provides an Enhanced Data Grid, that supports other column types.</summary>
    public class DNNDataGrid : DataGrid
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DNNDataGrid));
        public event DNNDataGridCheckedColumnEventHandler ItemCheckedChanged;

        /// <summary>Called when the grid is Data Bound.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDataBinding(EventArgs e)
        {
			Logger.LogError(">>> Entering OnDataBinding(e={E})", e);
            foreach (DataGridColumn column in this.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof(CheckBoxColumn)))
                {
                    // Manage CheckBox column events
                    var cbColumn = (CheckBoxColumn)column;
					Logger.LogError("OnDataBinding(e={E}): cbColumn → {CbColumn}", e, cbColumn);
                    cbColumn.CheckedChanged += this.OnItemCheckedChanged;
                }
            }
			Logger.LogWarning("<<< Exiting OnDataBinding(e={E})", e);
        }

        /// <inheritdoc />
        protected override void CreateControlHierarchy(bool useDataSource)
        {
			Logger.LogError(">>> Entering CreateControlHierarchy(useDataSource={UseDataSource})", useDataSource);
            base.CreateControlHierarchy(useDataSource);
			Logger.LogWarning("<<< Exiting CreateControlHierarchy(useDataSource={UseDataSource})", useDataSource);
        }

        /// <inheritdoc />
        protected override void PrepareControlHierarchy()
        {
			Logger.LogError(">>> Entering PrepareControlHierarchy()");
            base.PrepareControlHierarchy();
			Logger.LogWarning("<<< Exiting PrepareControlHierarchy()");
        }

        /// <summary>Centralised Event that is raised whenever a check box is changed.</summary>
        private void OnItemCheckedChanged(object sender, DNNDataGridCheckChangedEventArgs e)
        {
			Logger.LogError(">>> Entering OnItemCheckedChanged(sender={Sender},e={E})", sender, e);
            if (this.ItemCheckedChanged != null)
            {
                this.ItemCheckedChanged(sender, e);
            }
			Logger.LogWarning("<<< Exiting OnItemCheckedChanged(sender={Sender},e={E})", sender, e);
        }
    }
}
