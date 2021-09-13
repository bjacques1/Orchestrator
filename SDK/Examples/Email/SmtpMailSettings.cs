//*********************************************************
//
//    Copyright (c) Microsoft. All rights reserved.
//    This code is licensed under the Microsoft Public License.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

 

using System;
using System.Net.Mail;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Email
{
    /// <summary>
    /// An OpaliData class used to provide SMTP configuration settings.
    /// </summary>
    [ActivityData("SMTP Mail Settings")]
    public class SmtpMailSettings
    {
        private String host = string.Empty;
        private String userName = string.Empty;
        private String password = string.Empty;        

        [ActivityInput, ActivityOutput]
        public String MailServer
        {
            get { return host; }
            set { host = value; }
        }

        [ActivityInput]
        public String UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        [ActivityInput(PasswordProtected=true)]
        public String Password
        {
            get { return password;  }
            set { password = value; }
        }
    }
}
