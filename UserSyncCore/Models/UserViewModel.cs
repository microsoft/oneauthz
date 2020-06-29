// -------------------------------------------------------------------------------
// <copyright file="UserViewModel.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------

namespace UserSyncCore.Models
{
    using System.Collections.Generic;
    using Microsoft.Graph;

    /// <summary>
    /// Model for displaying MS Graph user information.
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserViewModel"/> class.
        /// </summary>
        /// <param name="users">List<User></param>
        public UserViewModel(List<User> users)
        {
            this.Users = users;
        }

        /// <summary>
        /// Gets a list of MS GRaph User objects.
        /// </summary>
        public List<User> Users { get; }
    }
}
