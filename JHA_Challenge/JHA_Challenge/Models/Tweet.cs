using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JHA_Challenge.Models.Entity;

namespace JHA_Challenge.Models
{
    public class Tweet
    {
        public string Id { get; init; }
        public string Text { get; init; }
        public Entities Entities { get; init; } = new Entities();
    }
}
