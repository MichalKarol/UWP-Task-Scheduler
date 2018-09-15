using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskScheduler.Models
{
    public class Action
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public enum ActionType : int { URI, NOTIFICATION, APPLICATION };
        public ActionType Type { get; set; }

        public int? JobId { get; set; }
        public Job Job { get; set; }

        public int ActionId { get; set; }
    }
}
