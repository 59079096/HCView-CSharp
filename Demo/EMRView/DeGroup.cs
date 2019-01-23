using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;
using HC.View;
using System.IO;

namespace EMRView
{
    public class DeGroup : HCDomainItem
    {
        private bool FReadOnly;

        private Dictionary<string, string> FPropertys;

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        //
        protected string GetValue(string key)
        {
            return FPropertys[key];
        }

        protected void SetValue(string key, string value)
        {
            FPropertys[key] = value;
        }

        public DeGroup(HCCustomData aOwnerData) : base(aOwnerData)
        {
            FPropertys = new Dictionary<string, string>();
            FReadOnly = false;
        }

        ~DeGroup()
        {

        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void Assign(HCCustomItem source)
        {
            string vS = DeProp.GetPropertyString((source as DeItem).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
            FReadOnly = (source as DeGroup).ReadOnly;
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public bool ReadOnly
        {
            get { return FReadOnly; }
            set { FReadOnly = value; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }
}
