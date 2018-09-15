using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TaskScheduler.Utils;
using TaskScheduler.Models;

namespace TaskScheduler.Migrations
{
    [DbContext(typeof(TaskSchedulerDbContext))]
    partial class TaskSchedulerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.6");

            modelBuilder.Entity("TaskScheduler.Models.Action", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ActionId");

                    b.Property<int?>("JobId");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.HasIndex("JobId");

                    b.ToTable("Actions");
                });

            modelBuilder.Entity("TaskScheduler.Models.ApplicationAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationName");

                    b.HasKey("Id");

                    b.ToTable("ApplicationActions");
                });

            modelBuilder.Entity("TaskScheduler.Models.Job", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Cron");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("TaskScheduler.Models.NotificationAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("Audio");

                    b.Property<string>("Image");

                    b.Property<string>("Text");

                    b.Property<int?>("Timeout");

                    b.HasKey("Id");

                    b.ToTable("NotificationActions");
                });

            modelBuilder.Entity("TaskScheduler.Models.UriAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Uri");

                    b.HasKey("Id");

                    b.ToTable("UriActions");
                });

            modelBuilder.Entity("TaskScheduler.Models.Action", b =>
                {
                    b.HasOne("TaskScheduler.Models.Job", "Job")
                        .WithMany()
                        .HasForeignKey("JobId");
                });
        }
    }
}
