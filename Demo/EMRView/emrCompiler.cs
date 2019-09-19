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

namespace EMRView
{
    public class ScriptResult
    {
        public string Text = "";
        public bool Cancel = false;
    }
    public class emrCompiler
    {
        //private ScriptEngine FScriptEngine;
        private string FErrorMessage;

        public emrCompiler()
        {
            
        }

        private void InitReferences(List<string> references)
        {
            // 手动添加需要的程序集，也可以通过20190916001的地方固定常用的程序集
            references.Add("EMRView.exe");
            references.Add("HCView.dll");
            references.Add("System.Windows.Forms.dll");
        }

        public bool RunScript(string script, DeItem deItem, PatientInfo patientInfo, RecordInfo recordInfo, ref string text, ref bool cancel)
        {
            FErrorMessage = "";
            List<string> references = new List<string>();
            InitReferences(references);

            ScriptEngine scriptEngine = new ScriptEngine(false, script, references);
            ScriptResult scriptResult = (ScriptResult)scriptEngine.Invoke("CheckValue", deItem, patientInfo, recordInfo, text, cancel);

            text = scriptResult.Text;
            cancel = scriptResult.Cancel;
            return true;
        }

        public bool CompileScript(string script)
        {
            FErrorMessage = "";
            List<string> references = new List<string>();
            InitReferences(references);

            ScriptEngine scriptEngine = new ScriptEngine(true, script, references);
            if (scriptEngine.CompileException != null)
            {
                FErrorMessage = scriptEngine.CompileException.Message;
                return false;
            }
            else
                return true;
        }

        public string ErrorMessage
        {
            get { return FErrorMessage; }
        }
    }
}
