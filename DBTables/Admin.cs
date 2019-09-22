﻿using System.ComponentModel.DataAnnotations.Schema;
using Pomelo.EntityFrameworkCore.MySql.ValueGeneration;

namespace ArmA_Bot.DBTables {

    public class Admin {

        public int Id { get; set; }
        public long UserId { get; set; }
        public long GroupId { get; set; }
    }
}