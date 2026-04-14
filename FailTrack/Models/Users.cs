using System;
using System.Collections.Generic;

namespace FailTrack.Models;

public partial class Users
{
    public int Id { get; set; }

    public string UserName { get; set; }

    public string PasswordHash { get; set; }

    public string PasswordSalt { get; set; }

    public int? IdLine { get; set; }

    public string Role { get; set; }

    public virtual Lines IdLineNavigation { get; set; }
}