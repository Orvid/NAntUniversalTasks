using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Snak.Types
{
    [Serializable()]
    public class TfsPolicyOverride : Element
    {
        private bool _if;

        /// <summary>
        /// If <c>true</c> then the entity will be included. The default is <c>true</c>.
        /// </summary>
        [TaskAttribute("if")]
        [BooleanValidator]
        public bool If
        {
            get { return _if; }
            set { _if = value; }
        }

        private string _overrideComment;

        [TaskAttribute("overrideComment", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string OverrideComment
        {
            get { return _overrideComment; }
            set { _overrideComment = value; }
        }

        public PolicyOverrideInfo CreatePolicyOverrideInfo()
        {
            // PolicyFailure p = new PolicyFailure(""
            return new PolicyOverrideInfo(_overrideComment, null);
        }


        public TfsPolicyOverride()
        {
            If = true;
        }
    }
}
