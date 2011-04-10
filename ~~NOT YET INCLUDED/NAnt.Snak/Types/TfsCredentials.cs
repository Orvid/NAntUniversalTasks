using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Snak.Types
{
    [Serializable()]
    public class TfsCredentials : Element
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

        private string _username;

        /// <summary>
        ///   Username that should be used.  Domain cannot be placed here, rather in domain property.
        /// </summary>
        [TaskAttribute("username", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _password;

        /// <summary>
        ///   The password in clear test of the domain user to be used.
        /// </summary>
        [TaskAttribute("password", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _domain;

        /// <summary>
        ///  The domain of the user to be used.
        /// </summary>
        [TaskAttribute("userDomain", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        public TfsCredentials()
        {
            If = true;
        }
    }
}
