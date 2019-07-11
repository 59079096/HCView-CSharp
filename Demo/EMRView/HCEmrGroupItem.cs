/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
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

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            if (value.IndexOf("=") >= 0)
                throw new Exception("属性值中不允许有=号");

            FPropertys[key] = value;
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
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
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            string vS = DeProp.GetPropertyString((source as DeGroup).Propertys);
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
