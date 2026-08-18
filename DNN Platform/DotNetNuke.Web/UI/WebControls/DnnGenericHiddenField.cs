// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Collections.Specialized;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// <summary>A hidden field with a typed value.</summary>
    /// <typeparam name="T">The type of value.</typeparam>
    public class DnnGenericHiddenField<T> : HiddenField
        where T : class, new()
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnGenericHiddenField));
        private T typedValue;

        private bool isValueSerialized;

        /// <summary>Gets the typed value or a new instance of <typeparamref name="T"/> if it hasn't been set.</summary>
        public T TypedValueOrDefault
        {
            get
            {
                return this.TypedValue ?? (this.TypedValue = new T());
            }
        }

        /// <summary>Gets a value indicating whether the hidden field has a value.</summary>
        public bool HasValue
        {
            get { return this.typedValue != null; }
        }

        /// <summary>Gets or sets the typed value.</summary>
        public T TypedValue
        {
            get
            {
                return this.typedValue;
            }

            set
            {
                this.typedValue = value;
                this.isValueSerialized = false;
            }
        }

        /// <inheritdoc />
        public override void RenderControl(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderControl(writer={Writer})", writer);
            this.EnsureValue();
            base.RenderControl(writer);
			Logger.LogDebug("<<< Exiting RenderControl(writer={Writer})", writer);
        }

        /// <inheritdoc />
        protected override object SaveViewState()
        {
			Logger.LogDebug(">>> Entering SaveViewState()");
            this.EnsureValue();
			Logger.LogDebug("<<< Exiting SaveViewState()");
            return base.SaveViewState();
        }

        /// <inheritdoc />
        protected override void LoadViewState(object savedState)
        {
			Logger.LogDebug(">>> Entering LoadViewState(savedState={SavedState})", savedState);
            base.LoadViewState(savedState);
            this.SetTypedValue();
			Logger.LogDebug("<<< Exiting LoadViewState(savedState={SavedState})", savedState);
        }

        /// <inheritdoc />
        protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			Logger.LogDebug(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            var controlsStateChanged = base.LoadPostData(postDataKey, postCollection);
            if (controlsStateChanged)
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): controlsStateChanged → {ControlsStateChanged}", postDataKey, postCollection, controlsStateChanged);
            {
                this.SetTypedValue();
            }

			Logger.LogDebug("<<< Exiting LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            return controlsStateChanged;
        }

        /// <inheritdoc />
        protected override void TrackViewState()
        {
			Logger.LogDebug(">>> Entering TrackViewState()");
            this.EnsureValue();
            base.TrackViewState();
			Logger.LogDebug("<<< Exiting TrackViewState()");
        }

        private void SetTypedValue()
        {
			Logger.LogDebug(">>> Entering SetTypedValue()");
            this.typedValue = string.IsNullOrEmpty(this.Value) ? null : Json.Deserialize<T>(this.Value);
			Logger.LogDebug("SetTypedValue(): typedValue → {TypedValue}", typedValue);
			Logger.LogDebug("<<< Exiting SetTypedValue()");
        }

        private void EnsureValue()
        {
			Logger.LogDebug(">>> Entering EnsureValue()");
            if (!this.isValueSerialized)
            {
                this.SerializeValue();
            }
			Logger.LogDebug("<<< Exiting EnsureValue()");
        }

        private void SerializeValue()
        {
			Logger.LogDebug(">>> Entering SerializeValue()");
            this.Value = this.typedValue == null ? string.Empty : Json.Serialize(this.typedValue);
            this.isValueSerialized = true;
			Logger.LogDebug("SerializeValue(): this.isValueSerialized → {Arg0}", this.isValueSerialized);
			Logger.LogDebug("<<< Exiting SerializeValue()");
        }
    }
}
