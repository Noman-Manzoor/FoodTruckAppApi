﻿using System;
using System.Collections.Generic;

namespace FoodTruckAppApi.Domain;

public partial class User
{
    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? UserName { get; set; }

    public string? UserType { get; set; }

    public string? ContactNo { get; set; }

    public string? Email { get; set; }

    public string? Location { get; set; }

    public string? Medium { get; set; }

    public string? Password { get; set; }

    public DateTime? CreatedDate { get; set; }
}
