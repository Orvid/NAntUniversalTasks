using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace Snak.ConfigurationTransformation
{
    internal class IfElseEndIFConstruct
    {
        private readonly string _ifElseConstructInnerText = String.Empty;

        internal string IfElseConstructInnerText
        {
            get { return _ifElseConstructInnerText; }
        }

        private Dictionary<string, string> _ifElseBranchPartsByVariable = new Dictionary<string, string>();

        internal Dictionary<string, string> IfElseBranchPartsByVariable
        {
            get { return _ifElseBranchPartsByVariable; }
        }

        internal IfElseEndIFConstruct(string ifElseConstructInnerText)
        {
            this._ifElseConstructInnerText = ifElseConstructInnerText;
        }

        internal void AddIfBranchPart(string branchVariable, string branchInnerText)
        {
            _ifElseBranchPartsByVariable.Add(branchVariable, branchInnerText);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (KeyValuePair<string, string> keyValuePair in this._ifElseBranchPartsByVariable)
            {
                stringBuilder.Append(keyValuePair.Key);
                stringBuilder.Append(": ");
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(keyValuePair.Value);
                stringBuilder.Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }
    }
}
