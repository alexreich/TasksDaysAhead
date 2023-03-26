// See https://aka.ms/new-console-template for more information
using Microsoft.Win32.TaskScheduler;
using System.Configuration;
using Task = Microsoft.Win32.TaskScheduler.Task;

EnumFolderTasks(TaskService.Instance.RootFolder);

void EnumFolderTasks(TaskFolder fld)
{
    List<Task> tasks = new List<Task>();
    int Tasknum;
    foreach (Task task in fld.Tasks)
    {
        var isNumeric = int.TryParse(task.Name, out int n);
        if (isNumeric)
        {
            tasks.Add(task);
        }
    }
    //today at 7am
    var today = DateTime.Today;
    today = today.AddHours(7);

    int day = 1;
    tasks.OrderBy(o => int.Parse(o.Name)).ToList().ForEach(t =>
    {
        t.Enabled = true;

        Console.WriteLine(t.Name);
        TaskDefinition td = t.Definition;
        t.Definition.Triggers.First().StartBoundary = today.AddDays(day);
        Console.WriteLine(t.Definition.Triggers.First().StartBoundary);
        day++;
        td.Principal.RunLevel = TaskRunLevel.Highest;
        TaskService.Instance.RootFolder.RegisterTaskDefinition(t.Name, t.Definition, TaskCreation.Update, ConfigurationManager.AppSettings["username"], ConfigurationManager.AppSettings["password"]);
    });

}