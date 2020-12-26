﻿using System.Collections.Generic;

namespace Fiar.ViewModels.Acl
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Nickname { get; set; }

        public List<string> RoleList { get; set; }
    }
}
