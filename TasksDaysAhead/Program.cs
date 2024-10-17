using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using SchedulerTask = Microsoft.Win32.TaskScheduler.Task;

using (var taskService = new TaskService())
{
    EnumFolderTasks(taskService.RootFolder);
}

void EnumFolderTasks(TaskFolder folder)
{
    int startHour = int.Parse(ConfigurationManager.AppSettings["startHour"]);
    int startMinute = int.Parse(ConfigurationManager.AppSettings["startMinute"]);
    int daysAhead = int.Parse(ConfigurationManager.AppSettings["daysahead"]);
    string username = ConfigurationManager.AppSettings["username"];
    string password = ConfigurationManager.AppSettings["password"];

    DateTime startDateTime = DateTime.Today.AddHours(startHour).AddMinutes(startMinute);
    List<SchedulerTask> tasks = folder.Tasks.Where(task => int.TryParse(task.Name, out _)).ToList();

    foreach (var task in tasks.OrderBy(t => int.Parse(t.Name)))
    {
        UpdateTask(task, ref startDateTime, ref daysAhead, username, password);
    }
}

void UpdateTask(SchedulerTask task, ref DateTime startDateTime, ref int dayOffset, string username, string password)
{
    try
    {
        task.Enabled = true;

        TaskDefinition td = task.Definition;
        Trigger trigger = td.Triggers.FirstOrDefault();
        if (trigger != null)
        {
            trigger.StartBoundary = startDateTime.AddDays(dayOffset);
        }

        Console.WriteLine($"Task: {task.Name}, StartBoundary: {trigger?.StartBoundary}");

        dayOffset++;

        td.Principal.RunLevel = TaskRunLevel.Highest;
        td.Principal.LogonType = TaskLogonType.InteractiveToken;
        td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

        TaskService.Instance.RootFolder.RegisterTaskDefinition(task.Name, td, TaskCreation.Update, username, password, TaskLogonType.InteractiveToken);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to update task '{task.Name}': {ex.Message}");
    }
}
