using TaskScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskScheduler.Utils
{
    class TaskSchedulerDbContext : DbContext
    {
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Models.Action> Actions { get; set; }
        public DbSet<UriAction> UriActions { get; set; }
        public DbSet<NotificationAction> NotificationActions { get; set; }
        public DbSet<ApplicationAction> ApplicationActions { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=TaskSchedulerDb.db");
        }

        public Models.Action[] ActionsForActionPredicate(Func<Models.Action, bool> pred)
        {
            return Actions.Where(pred).ToArray();
        }

        public UriAction[] UriActionsForActionPredicate(Func<Models.Action, bool> pred)
        {
            int[] actions = Actions.Where(pred).Select(x => x.ActionId).ToArray();
            return UriActions.Where(x => actions.Contains(x.Id)).ToArray();
        }

        public NotificationAction[] NotificationActionsForActionPredicate(Func<Models.Action, bool> pred)
        {
            int[] actions = Actions.Where(pred).Select(x => x.ActionId).ToArray();
            return NotificationActions.Where(x => actions.Contains(x.Id)).ToArray();
        }

        public ApplicationAction[] ApplicationActionsForActionPredicate(Func<Models.Action, bool> pred)
        {
            int[] actions = Actions.Where(pred).Select(x => x.ActionId).ToArray();
            return ApplicationActions.Where(x => actions.Contains(x.Id)).ToArray();
        }
    }
}
